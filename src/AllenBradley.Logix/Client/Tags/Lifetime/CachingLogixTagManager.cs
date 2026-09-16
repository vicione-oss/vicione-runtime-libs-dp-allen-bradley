using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and hands out one <see cref="ILogixTag"/> per data point, kept for
/// this manager's lifetime because an initialised tag is the expensive, reusable resource
/// (<c>ADR/2026-07-16-reusing-and-releasing-tag-handles.md</c>). Data points are records, so the whole
/// data point is the key: poll frequency is part of it, and one tag configured at two frequencies is two
/// points and two tags.
/// </summary>
internal sealed class CachingLogixTagManager(
    ILogixTagAccessFactory factory,
    ITagDefinitionsLoader schemaBrowser,
    ILogger<CachingLogixTagManager> logger)
    : ILogixTagManager
{
    private readonly Dictionary<ILogixDataPoint, ILogixTag> _tagByDataPoint = [];
    private readonly Lock _gate = new();

    // A sync Lock cannot be held across the browse, so the wait for it needs its own async gate. Never
    // disposed, deliberately: it owns no unmanaged resource, and disposing it would let a client disposed
    // mid-browse throw out of the release below's finally.
    private readonly SemaphoreSlim _loadGate = new(1, 1);

    private TagDefinitions? _tagDefinitions;
    private bool _disposed;

    /// <inheritdoc />
    public async Task LoadTagDefinitionsAsync(CancellationToken cancellationToken)
    {
        // Checking the field and then browsing is a check-then-act, so the gate is what makes a
        // concurrent second call a no-op rather than a second round trip. A gate and not a cached
        // Lazy<Task>: a browse that failed must not be the answer every later connect gets.
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
        // then abandoned to its finalizer. The lock is cheap to hold: Create only allocates, since
        // libplctag initialises the handle on its first read.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_tagDefinitions is not { } schema)
            {
                throw new InvalidOperationException(
                    "The controller schema must be loaded before a tag is requested; " +
                    "connect calls LoadTagDefinitionsAsync before the first TagFor.");
            }

            if (_tagByDataPoint.TryGetValue(dataPoint, out var cached))
            {
                return cached;
            }

            // The listing never names an element, so an element resolves to its array's declaration.
            var metadata = schema.Lookup(dataPoint.TagPath.TagDefinitionAddress);
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

    /// <summary>Caller must hold <see cref="_gate"/>.</summary>
    private void DrainCore()
    {
        // A throwing Dispose must not strand the tags queued behind it, so the reason is logged, not
        // raised: an unfreed handle is exactly what this drain exists to prevent.
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
