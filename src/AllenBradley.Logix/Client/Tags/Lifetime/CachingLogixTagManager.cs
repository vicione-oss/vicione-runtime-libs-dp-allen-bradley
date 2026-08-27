using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and hands out one <see cref="ILogixTag"/> per data
/// point, keeping it for this manager's lifetime: an initialised tag is the reusable, expensive
/// resource (ADR-002), and the schema is the browse-once metadata joined onto it. Data points are
/// records, so the data point <em>is</em> the key — two points naming the same tag with the same shape
/// share one tag.
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
    ILogixSchemaBrowser schemaBrowser,
    ILogger<CachingLogixTagManager> logger)
    : ILogixTagManager, IDisposable
{
    private readonly Dictionary<ILogixDataPoint, ILogixTag> _tagByDataPoint = [];
    private readonly Lock _gate = new();
    private LogixControllerSchema? _schema;
    private bool _disposed;

    /// <inheritdoc />
    public async Task LoadSchemaAsync(CancellationToken cancellationToken)
    {
        // Idempotent, mirroring S7's _rootNodeHandle guard: a schema already in hand is left as is, so a
        // second connect call is a no-op rather than a second browse. Connect is the single caller, before
        // any polling, so the browse runs off the lock without racing a real second load.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_schema is not null)
            {
                return;
            }
        }

        var schema = await schemaBrowser.BrowseAsync(cancellationToken).ConfigureAwait(false);

        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _schema ??= schema;
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

            if (_schema is not { } schema)
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

    /// <summary>
    /// Disposes every tag this manager created and drops the schema. Every libplctag handle must be
    /// disposed: one left to its finalizer is freed after CLR teardown, which fail-fasts the process
    /// (0xC0000602).
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
            _schema = null;
        }
    }
}
