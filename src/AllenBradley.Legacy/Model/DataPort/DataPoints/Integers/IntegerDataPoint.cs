using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;

/// <summary>One element of an integer (<c>N</c>) file — a 16-bit word, carried as <see cref="short"/>.</summary>
/// <param name="FileNumber">The number of the integer file.</param>
/// <param name="ElementNumber">The element inside it.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record IntegerDataPoint(
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    PollFrequency PollFrequency,
    Channels Channels)
    : LegacyDataPoint<short>(new DataFileAddress(DataFileType.Integer, FileNumber, ElementNumber), PollFrequency, Channels)
{
    /// <inheritdoc />
    internal override ILegacyDataPointValue<short> CreateLegacyValue(short value) => new Value(this, value);

    // Every short is a valid element: an N file holds the whole two's-complement range.
    private sealed record Value(ILegacyDataPoint DataPoint, short TypedValue) : ILegacyDataPointValue<short>
    {
        public bool IsInValueRange() => true;
    }
}
