using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

/// <summary>
/// Hands out one connected <see cref="ILogixClient"/> per <see cref="LogixClientInformation"/> and
/// reference-counts it, so the incoming and outgoing dataports for a controller share its connection
/// instead of opening two (the shared-connection ADR). Released down to zero, the client is
/// disconnected and disposed.
/// </summary>
/// <remarks>
/// <para>
/// A process-wide singleton, because sharing is the whole point and two pools would defeat it:
/// dataports built independently at startup all have to find the same one.
/// </para>
/// <para>
/// <b>The connect is the entry.</b> An entry holds its connect as a <see cref="Lazy{T}"/> of a task, so
/// every caller takes one path — increment the count under the lock, then await that one task. The
/// caller that created the entry and the callers that arrived while it was still browsing are the same
/// code, which is what keeps a second connect from starting and a second cleanup from disagreeing with
/// the first. The lazy runs the connect exactly once and <em>outside</em> the pool lock, so one slow
/// browse never blocks acquires for unrelated controllers.
/// </para>
/// <para>
/// <b>One way out.</b> Every acquire that does not hand a client back — the connect failed, the wait was
/// cancelled, the pool was disposed underneath it — gives its reference back through
/// <see cref="ReleaseReferenceAsync"/>, the same routine <see cref="ReleaseAsync"/> uses. Whichever of
/// those it was, the last reference out is what tears the entry down.
/// </para>
/// <para>
/// <b>The key is the controller.</b> That is the scoping the shared-connection ADR chose, and the axis a
/// later per-poll-class split would extend: it would widen <see cref="LogixClientInformation"/> rather
/// than change anything here.
/// </para>
/// </remarks>
internal sealed class LogixClientPool
    : IClientLifecycleManager<ILogixClient, LogixClientInformation>, IAsyncDisposable
{
    private readonly ILogger<LogixClientPool> _logger;
    private readonly Dictionary<LogixClientInformation, PooledClient> _pooledClients = [];
    private readonly Lock _poolLock = new();
    private readonly ILogixClientFactory _clientFactory;

    private bool _disposed;

    private static readonly Lock s_initLock = new();
    private static LogixClientPool? s_instance;

    private LogixClientPool(ILoggerFactory loggerFactory, ILogixClientFactory clientFactory)
    {
        _logger = loggerFactory.CreateLogger<LogixClientPool>();
        _clientFactory = clientFactory;
    }

    /// <summary>The pool every dataport shares. The first caller's <paramref name="loggerFactory"/> wins.</summary>
    internal static LogixClientPool GetInstance(ILoggerFactory loggerFactory)
    {
        lock (s_initLock)
        {
            return s_instance ??= new LogixClientPool(loggerFactory, new LogixClientFactory(loggerFactory));
        }
    }

    /// <summary>
    /// A pool of its own, over a factory the caller names. The singleton is deliberately unreachable
    /// from a test: one shared instance across a suite would carry entries between test cases.
    /// </summary>
    internal static LogixClientPool CreateTestInstance(
        ILoggerFactory loggerFactory, ILogixClientFactory clientFactory) => new(loggerFactory, clientFactory);

    internal IReadOnlyDictionary<LogixClientInformation, PooledClient> PooledClients => _pooledClients;

    /// <summary>
    /// Returns a connected client for <paramref name="clientInformation"/> — the pooled one if there is
    /// one, a newly connected one otherwise — and counts the caller as a holder of it.
    /// </summary>
    public async ValueTask<ILogixClient> AcquireConnectedAsync(
        LogixClientInformation clientInformation, CancellationToken cancellationToken)
    {
        _logger.AcquiringClient(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);

        var entry = GetOrCreatePooledClient(clientInformation, cancellationToken);

        try
        {
            // The one connect for this controller, whether this caller started it or found it running.
            // The token only abandons the wait: a caller walking away must not cancel the browse the
            // other holders are waiting on.
            var client = await entry.Connected.Value.WaitAsync(cancellationToken).ConfigureAwait(false);

            // The pool can have been disposed while this connect was in flight, in which case the entry
            // has already been torn down and this client is on its way out — handing it over would give
            // the caller a client about to be disposed under it.
            ThrowIfDisposed();

            return client;
        }
        catch (Exception ex)
        {
            // A cancelled acquire is the caller walking away, not the controller failing.
            if (ex is not OperationCanceledException)
            {
                _logger.AcquireClientFailed(ex, clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
            }

            await ReleaseReferenceAsync(entry, clientInformation).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Drops the caller's reference to the client for <paramref name="clientInformation"/>. The last one
    /// out disconnects and disposes it.
    /// </summary>
    public async ValueTask ReleaseAsync(
        LogixClientInformation clientInformation, CancellationToken cancellationToken)
    {
        _logger.ReleasingClient(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);

        PooledClient? entry;

        lock (_poolLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            entry = _pooledClients.GetValueOrDefault(clientInformation);
            if (entry is null)
            {
                _logger.ClientNotFoundInPool(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
                return;
            }
        }

        await ReleaseReferenceAsync(entry, clientInformation).ConfigureAwait(false);
    }

    /// <summary>Disconnects and disposes every pooled client, then ends the pool.</summary>
    public async ValueTask DisposeAsync()
    {
        KeyValuePair<LogixClientInformation, PooledClient>[] entries;

        lock (_poolLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Snapshot and clear under the lock rather than iterating the live dictionary: an acquire
            // that was already past the lock when this ran will come back to release its reference, and
            // it must not be mutating the collection this is walking. Finding its entry gone is also
            // what stops it tearing the same client down a second time.
            entries = [.. _pooledClients];
            _pooledClients.Clear();
        }

        // Deliberately not waiting on a connect still in flight: a browse that will not answer is
        // exactly the case a shutdown must not hang on. Its caller is turned away by ThrowIfDisposed.
        await Task.WhenAll(
                entries.Select(entry => DisconnectAndDisposeAsync(entry.Value.Client, entry.Key).AsTask()))
            .ConfigureAwait(false);

        _logger.PoolDisposed();
    }

    /// <summary>
    /// Finds or creates the entry for <paramref name="clientInformation"/> and counts the caller onto
    /// it. Creating does not connect: the entry's lazy does that on the first await, off this lock.
    /// </summary>
    private PooledClient GetOrCreatePooledClient(
        LogixClientInformation clientInformation, CancellationToken cancellationToken)
    {
        lock (_poolLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_pooledClients.TryGetValue(clientInformation, out var existing))
            {
                existing.IncrementRefCount();
                _logger.ReusingPooledClient(
                    clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value, existing.RefCount);
                return existing;
            }

            var entry = new PooledClient(_clientFactory.Create(clientInformation), cancellationToken);
            _pooledClients[clientInformation] = entry;
            _logger.CreatedPooledClient(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
            return entry;
        }
    }

    /// <summary>
    /// Gives one reference back, whether it was ever used or not, and tears the entry down when it was
    /// the last. The single exit for a release, a failed connect, a cancelled wait and a disposed pool,
    /// so none of them can free a client another holder is still using.
    /// </summary>
    private async ValueTask ReleaseReferenceAsync(
        PooledClient entry, LogixClientInformation clientInformation)
    {
        bool tearDown;

        lock (_poolLock)
        {
            if (!entry.DecrementRefCount())
            {
                _logger.RefCountUnderflow(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
            }

            // Removed by identity, not by key: this entry may already have been replaced by a later
            // acquire or taken by a dispose, and tearing down whatever is filed under the key now would
            // close a connection its own holders still expect.
            tearDown = entry.RefCount <= 0 && RemoveIfStillPooled(clientInformation, entry);

            if (!tearDown)
            {
                _logger.ClientReleased(
                    clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value, entry.RefCount);
            }
        }

        if (tearDown)
        {
            await DisconnectAndDisposeAsync(entry.Client, clientInformation).ConfigureAwait(false);
        }
    }

    /// <remarks>Caller must hold <see cref="_poolLock"/>.</remarks>
    private bool RemoveIfStillPooled(LogixClientInformation clientInformation, PooledClient entry) =>
        _pooledClients.TryGetValue(clientInformation, out var current)
        && ReferenceEquals(current, entry)
        && _pooledClients.Remove(clientInformation);

    /// <remarks>
    /// The disconnect is attempted even for a client whose connect failed: it is idempotent, and a
    /// connect that got as far as loading a schema before it threw has handles to drain.
    /// </remarks>
    private async ValueTask DisconnectAndDisposeAsync(
        ILogixClient client, LogixClientInformation clientInformation)
    {
        _logger.DisconnectingPooledClient(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
        try
        {
            await client.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.DisconnectPooledClientFailed(
                ex, clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
        }
        finally
        {
            // Not in the try: a disconnect that threw is exactly when the handles most need freeing, and
            // one left to its finalizer fail-fasts the process at CLR teardown.
            client.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        lock (_poolLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }

    /// <summary>One pooled client, its holder count, and the single connect every holder awaits.</summary>
    /// <remarks>
    /// <see cref="Connected"/> is <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/>, so the
    /// connect runs once however many callers reach it at once, and it runs on the first await rather
    /// than at construction — which is what keeps it off the pool's lock.
    /// </remarks>
    /// <param name="client">The client this entry pools.</param>
    /// <param name="cancellationToken">
    /// The creating caller's token. It drives the connect itself, so a later holder's cancellation
    /// abandons only that holder's wait.
    /// </param>
    internal sealed class PooledClient(ILogixClient client, CancellationToken cancellationToken)
    {
        public ILogixClient Client { get; } = client;

        public Lazy<Task<ILogixClient>> Connected { get; } = new(
            async () =>
            {
                await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
                return client;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);

        public int RefCount { get; private set; } = 1;

        public void IncrementRefCount() => RefCount++;

        /// <returns><c>true</c> if the count was decremented; <c>false</c> if it was already zero (caller bug).</returns>
        public bool DecrementRefCount()
        {
            if (RefCount <= 0)
            {
                return false;
            }

            RefCount--;
            return true;
        }
    }
}
