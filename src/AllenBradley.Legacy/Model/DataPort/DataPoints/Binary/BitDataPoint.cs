using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Binary;

/// <summary>One bit of an element in a binary (<c>B</c>) file, carried as <see cref="bool"/>.</summary>
/// <param name="FileNumber">The number of the binary file.</param>
/// <param name="ElementNumber">The element inside it.</param>
/// <param name="BitNumber">The bit inside the element.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record BitDataPoint(
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    BitNumber BitNumber,
    PollFrequency PollFrequency,
    Channels Channels)
    : LegacyDataPoint<bool>(
        new DataFileAddress(DataFileType.Binary, FileNumber, ElementNumber, BitNumber), PollFrequency, Channels)
{
    /// <inheritdoc />
    internal override ILegacyDataPointValue<bool> CreateLegacyValue(bool value) => new Value(this, value);

    private sealed record Value(ILegacyDataPoint DataPoint, bool TypedValue) : ILegacyDataPointValue<bool>
    {
        public bool IsInValueRange() => true;
    }
}
