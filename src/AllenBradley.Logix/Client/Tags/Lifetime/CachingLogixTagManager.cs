using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and hands out one <see cref="ILogixTag"/> per data
/// point, keeping it for this manager's lifetime: an initialised tag is the reusable, expensive
/// resource (ADR-002), and the schema is the browse-once metadata joined onto it. Data points are
/// records, so the data point <em>is</em> the key — two points naming the same tag with the same shape
/// share one tag. Poll frequency and channels are part of that shape, so the same tag configured at two
/// frequencies is two points and gets two tags.
/// </summary>
/// <remarks>
/// One manager is scoped to one controller — that scope is the factory's connection information, the same
/// identity the browser reads over — which makes this the per-device tag cache and the per-device
/// schema owner: releasing the device disposes the manager, and with it every tag and the schema.
/// </remarks>
/// <param name="factory">Creates the tag access when a data point is first seen.</param>
/// <param name="schemaBrowser">Browses the controller's symbol table once at connect.</param>
/// <param name="logger">Records tags this manager could not free.</param>
internal sealed class CachingLogixTagManager(
    ILogixTagAccessFactory factory,
    ITagDefinitionsLoader schemaBrowser,
    ILogger<CachingLogixTagManager> logger)
    : ILogixTagManager
{
    private readonly Dictionary<ILogixDataPoint, ILogixTag> _tagByDataPoint = [];
    private readonly Lock _gate = new();

    // The browse is the one thing here that leaves the process, and it is the one thing that must happen
    // once. A sync Lock cannot be held across it, so the wait for it is its own async gate.
    //
    // Never disposed, deliberately: nothing takes its AvailableWaitHandle, so it owns no unmanaged
    // resource, and disposing it would open a failure mode it exists to close — a client disposed while
    // a browse is in flight would have the release below throw out of a finally.
    private readonly SemaphoreSlim _loadGate = new(1, 1);

    private TagDefinitions? _tagDefinitions;
    private bool _disposed;

    /// <inheritdoc />
    public async Task LoadTagDefinitionsAsync(CancellationToken cancellationToken)
    {
        // Idempotent, mirroring S7's _rootNodeHandle guard: a schema already in hand is left as is, so a
        // second call is a no-op rather than a second browse. The gate is what makes that true of
        // *concurrent* calls too — checking the field and then browsing is a check-then-act, and two
        // callers through it would both pay for a round trip to reach the same answer. Serialising here
        // rather than in a caller keeps the guarantee with the schema it is about, so it holds however
        // many callers ILogixTagManager ends up with.
        //
        // A gate and not a cached Lazy<Task>: a browse that failed must not be the answer every later
        // connect gets. Releasing this one lets the next caller retry against a controller that may since
        // have come back.
        await _loadGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (_tagDefinitions is not null)
                {
                    return;
                }
            }

            var schema = await schemaBrowser.LoadAsync(cancellationToken).ConfigureAwait(false);

            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _tagDefinitions = schema;
            }
        }
        finally
        {
            _loadGate.Release();
        }
    }

    /// <inheritdoc />
    public ILogixTag TagFor(ILogixDataPoint dataPoint)
    {
        // Creation happens under the same lock that guards disposal, so a tag can never be created and
        // then abandoned to its finalizer — neither by two callers racing to add the same key, nor by a
        // caller arriving after Dispose has drained the cache (see Dispose for why that must not happen).
        // The lock is cheap to hold: Create only allocates, since libplctag initialises the handle on its
        // first read, and the metadata is an in-memory lookup.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_tagDefinitions is not { } schema)
            {
                throw new InvalidOperationException(
                    "The controller schema must be loaded before a tag is requested; " +
                    "connect calls LoadSchemaAsync before the first TagFor.");
            }

            if (_tagByDataPoint.TryGetValue(dataPoint, out var cached))
            {
                return cached;
            }

            // Join the two immutable facts onto the tag at creation: the controller's metadata for it
            // (null when it is absent — the verifier's signal), and the access itself.
            var metadata = schema.Lookup(dataPoint.TagName);
            var access = factory.Create(dataPoint);
            var tag = new LogixTag(dataPoint, metadata, access);
            _tagByDataPoint.Add(dataPoint, tag);
            return tag;
        }
    }

    /// <inheritdoc />
    public void Drain()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            DrainCore();
        }
    }

    /// <summary>
    /// Drains the manager and ends its life. Every libplctag handle must be freed: one left to its
    /// finalizer is freed after CLR teardown, which fail-fasts the process (0xC0000602).
    /// </summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DrainCore();
        }
    }

    /// <remarks>Caller must hold <see cref="_gate"/>.</remarks>
    private void DrainCore()
    {
        // A throwing Dispose must not strand the tags queued behind it — an unfreed handle is exactly
        // what this drain exists to prevent — so the reason is logged, not raised.
        foreach (var (dataPoint, tag) in _tagByDataPoint)
        {
            try
            {
                tag.Dispose();
            }
            catch (Exception ex)
            {
                logger.FailedToFreeTagForDataPoint(dataPoint, ex);
            }
        }

        _tagByDataPoint.Clear();
        _tagDefinitions = null;
    }
}
