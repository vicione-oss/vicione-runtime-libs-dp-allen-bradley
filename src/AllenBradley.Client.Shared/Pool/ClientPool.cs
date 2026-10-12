using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.Extensions.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;

/// <summary>
/// Hands out one connected client per key and reference-counts it, so both dataports for a controller
/// share its connection instead of opening two
/// (ADR/2026-07-16-maximizing-throughput-with-one-shared-connection.md). Released down to zero, the
/// client is disconnected and disposed.
/// </summary>
/// <param name="logger">Where the pool reports; its category is the dataport's.</param>
/// <param name="createClient">Builds a disconnected client for a key, for the pool to connect.</param>
/// <param name="addressOf">What the log messages name the controller behind a key by.</param>
internal sealed class ClientPool<TClient, TKey>(
    ILogger logger,
    Func<TKey, TClient> createClient,
    Func<TKey, ClientAddress> addressOf)
    : IClientLifecycleManager<TClient, TKey>, IAsyncDisposable
    where TClient : class, IConnectableClient
    where TKey : IClientInformation
{
    private readonly Dictionary<TKey, PooledClient> _pooledClients = [];
    private readonly Lock _poolLock = new();

    private bool _disposed;

    internal IReadOnlyDictionary<TKey, PooledClient> PooledClients => _pooledClients;

    /// <summary>
    /// Returns a connected client for <paramref name="clientInformation"/> — the pooled one if there is
    /// one, a newly connected one otherwise — and counts the caller as a holder of it.
    /// </summary>
    public async ValueTask<TClient> AcquireConnectedAsync(
        TKey clientInformation, CancellationToken cancellationToken)
    {
        var address = addressOf(clientInformation);

        logger.AcquiringClient(address.ConnectionEndpoint, address.CipRoutePath);

        var entry = GetOrCreatePooledClient(clientInformation, cancellationToken);

        try
        {
            // The token abandons only this caller's wait: walking away must not cancel the one connect
            // the other holders are waiting on.
            var client = await entry.Connected.Value.WaitAsync(cancellationToken).ConfigureAwait(false);

            // A pool disposed while this connect was in flight has already torn the entry down, so this
            // client is on its way out and must not be handed over.
            ThrowIfDisposed();

            return client;
        }
        catch (Exception ex)
        {
            // A cancelled acquire is the caller walking away, not the controller failing.
            if (ex is not OperationCanceledException)
            {
                logger.AcquireClientFailed(ex, address.ConnectionEndpoint, address.CipRoutePath);
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
        TKey clientInformation, CancellationToken cancellationToken)
    {
        var address = addressOf(clientInformation);

        logger.ReleasingClient(address.ConnectionEndpoint, address.CipRoutePath);

        PooledClient? entry;

        lock (_poolLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            entry = _pooledClients.GetValueOrDefault(clientInformation);
            if (entry is null)
            {
                logger.ClientNotFoundInPool(address.ConnectionEndpoint, address.CipRoutePath);
                return;
            }
        }

        await ReleaseReferenceAsync(entry, clientInformation).ConfigureAwait(false);
    }

    /// <summary>Disconnects and disposes every pooled client, then ends the pool.</summary>
    public async ValueTask DisposeAsync()
    {
        KeyValuePair<TKey, PooledClient>[] entries;

        lock (_poolLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Snapshot and clear under the lock: an acquire already past the lock will come back to
            // release its reference, and must neither mutate the collection this walks nor find its
            // entry still there and tear the same client down twice.
            entries = [.. _pooledClients];
            _pooledClients.Clear();
        }

        // Deliberately not waiting on a connect still in flight: a browse that will not answer is exactly
        // the case a shutdown must not hang on. Its caller is turned away by ThrowIfDisposed.
        await Task.WhenAll(
                entries.Select(entry => DisconnectAndDisposeAsync(entry.Value.Client, entry.Key).AsTask()))
            .ConfigureAwait(false);

        logger.PoolDisposed();
    }

    /// <summary>
    /// Finds or creates the entry for <paramref name="clientInformation"/> and counts the caller onto
    /// it. Creating does not connect: the entry's lazy does that on the first await, off this lock.
    /// </summary>
    private PooledClient GetOrCreatePooledClient(
        TKey clientInformation, CancellationToken cancellationToken)
    {
        var address = addressOf(clientInformation);

        lock (_poolLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_pooledClients.TryGetValue(clientInformation, out var existing))
            {
                existing.IncrementRefCount();
                logger.ReusingPooledClient(address.ConnectionEndpoint, address.CipRoutePath, existing.RefCount);
                return existing;
            }

            var entry = new PooledClient(createClient(clientInformation), cancellationToken);
            _pooledClients[clientInformation] = entry;
            logger.CreatedPooledClient(address.ConnectionEndpoint, address.CipRoutePath);
            return entry;
        }
    }

    /// <summary>
    /// Gives one reference back and tears the entry down when it was the last. The single exit for a
    /// release, a failed connect and a cancelled wait alike.
    /// </summary>
    private async ValueTask ReleaseReferenceAsync(
        PooledClient entry, TKey clientInformation)
    {
        var address = addressOf(clientInformation);

        bool tearDown;

        lock (_poolLock)
        {
            if (!entry.DecrementRefCount())
            {
                logger.RefCountUnderflow(address.ConnectionEndpoint, address.CipRoutePath);
            }

            // Removed by identity, not by key: a later acquire may have replaced this entry, and tearing
            // down whatever is filed under the key now would close a connection its holders still expect.
            tearDown = entry.RefCount <= 0 && RemoveIfStillPooled(clientInformation, entry);

            if (!tearDown)
            {
                logger.ClientReleased(address.ConnectionEndpoint, address.CipRoutePath, entry.RefCount);
            }
        }

        if (tearDown)
        {
            await DisconnectAndDisposeAsync(entry.Client, clientInformation).ConfigureAwait(false);
        }
    }

    /// <summary>Caller must hold <see cref="_poolLock"/>.</summary>
    private bool RemoveIfStillPooled(TKey clientInformation, PooledClient entry) =>
        _pooledClients.TryGetValue(clientInformation, out var current)
        && ReferenceEquals(current, entry)
        && _pooledClients.Remove(clientInformation);

    private async ValueTask DisconnectAndDisposeAsync(
        TClient client, TKey clientInformation)
    {
        var address = addressOf(clientInformation);

        logger.DisconnectingPooledClient(address.ConnectionEndpoint, address.CipRoutePath);
        try
        {
            await client.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.DisconnectPooledClientFailed(ex, address.ConnectionEndpoint, address.CipRoutePath);
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

    /// <summary>
    /// One pooled client, its holder count, and the single connect every holder awaits. Only the creating
    /// caller's <paramref name="cancellationToken"/> drives that connect.
    /// </summary>
    internal sealed class PooledClient(TClient client, CancellationToken cancellationToken)
    {
        public TClient Client { get; } = client;

        public Lazy<Task<TClient>> Connected { get; } = new(
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
