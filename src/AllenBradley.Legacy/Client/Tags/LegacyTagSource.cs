using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;

/// <summary>
/// Creates one tag handle per data point and reuses it for each later read and write. The source
/// disposes the handles, so a caller must not dispose a handle it gets from here. After
/// <see cref="Drain"/>, the source creates new handles. After <see cref="Dispose"/>, it cannot be used.
/// </summary>
internal sealed class LegacyTagSource(ILegacyTagAccessFactory factory, ILogger<LegacyTagSource> logger)
    : ITagSource<ILegacyDataPoint, ITagAccess>, IDisposable
{
    private readonly TagCache<ILegacyDataPoint, ITagAccess> _tags = new(logger);

    /// <inheritdoc />
    public ITagAccess TagFor(ILegacyDataPoint dataPoint) => _tags.GetOrAdd(dataPoint, factory.CreateAccessForDataPoint);

    /// <summary>Disposes every tag handle. The next request for a data point creates a new handle.</summary>
    internal void Drain() => _tags.Drain();

    /// <summary>Disposes every tag handle and ends the source.</summary>
    public void Dispose() => _tags.Dispose();
}
