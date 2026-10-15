using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;

/// <summary>One element of a long (<c>L</c>) file — a 32-bit integer, carried as <see cref="int"/>.</summary>
/// <param name="FileNumber">The number of the long file.</param>
/// <param name="ElementNumber">The element inside it.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record LongDataPoint(
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    PollFrequency PollFrequency,
    Channels Channels)
    : LegacyDataPoint<int>(new DataFileAddress(DataFileType.Long, FileNumber, ElementNumber), PollFrequency, Channels)
{
    /// <inheritdoc />
    internal override ILegacyDataPointValue<int> CreateLegacyValue(int value) => new Value(this, value);

    // Every int is a valid element: an L file holds the whole two's-complement range.
    private sealed record Value(ILegacyDataPoint DataPoint, int TypedValue) : ILegacyDataPointValue<int>
    {
        public bool IsInValueRange() => true;
    }
}
