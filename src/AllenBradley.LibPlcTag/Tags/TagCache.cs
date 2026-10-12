using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tags;

/// <summary>
/// Keeps one tag per data point until it is drained, because an initialised libplctag handle is the
/// expensive, reusable resource (<c>ADR/2026-07-16-reusing-and-releasing-tag-handles.md</c>). The whole
/// data point is the key, so a data point that is a record reuses its tag by value. A tag handed out is
/// borrowed: the cache disposes it, and a holder that disposes one breaks every other holder.
/// <see cref="Drain"/> is reversible; <see cref="Dispose"/> is terminal.
/// </summary>
/// <param name="logger">Where a tag that will not free is reported; its category is the owner's.</param>
internal sealed class TagCache<TDataPoint, TTag>(ILogger logger) : IDisposable
    where TDataPoint : notnull
    where TTag : IDisposable
{
    private readonly Dictionary<TDataPoint, TTag> _tagByDataPoint = [];
    private readonly Lock _gate = new();

    private bool _disposed;

    /// <summary>
    /// Returns the tag kept for <paramref name="dataPoint"/>, or creates it with <paramref name="createTag"/>
    /// and keeps it.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The cache has been disposed.</exception>
    internal TTag GetOrAdd(TDataPoint dataPoint, Func<TDataPoint, TTag> createTag)
    {
        // Creation happens under the same lock that guards disposal, so a tag can never be created and
        // then abandoned to its finalizer. The lock is cheap to hold: creating a libplctag tag only
        // allocates, since libplctag initialises the handle on its first read.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_tagByDataPoint.TryGetValue(dataPoint, out var cached))
            {
                return cached;
            }

            var tag = createTag(dataPoint);
            _tagByDataPoint.Add(dataPoint, tag);
            return tag;
        }
    }

    /// <summary>Disposes every tag and forgets it, so the next request for a data point creates a new one.</summary>
    /// <exception cref="ObjectDisposedException">The cache has been disposed.</exception>
    internal void Drain()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            DrainCore();
        }
    }

    /// <summary>
    /// Drains the cache and ends its life. Every libplctag handle must be freed: one left to its
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
    }
}
