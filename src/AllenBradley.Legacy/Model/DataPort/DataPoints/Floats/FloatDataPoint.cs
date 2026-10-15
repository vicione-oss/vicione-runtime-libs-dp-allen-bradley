using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Floats;

/// <summary>One element of a float (<c>F</c>) file — a 32-bit IEEE 754 value, carried as <see cref="float"/>.</summary>
/// <param name="FileNumber">The number of the float file.</param>
/// <param name="ElementNumber">The element inside it.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record FloatDataPoint(
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    PollFrequency PollFrequency,
    Channels Channels)
    : LegacyDataPoint<float>(new DataFileAddress(DataFileType.Float, FileNumber, ElementNumber), PollFrequency, Channels)
{
    /// <inheritdoc />
    internal override ILegacyDataPointValue<float> CreateLegacyValue(float value) => new Value(this, value);

    // Every float is a valid element: an F file holds the whole IEEE 754 range, infinity and NaN included.
    private sealed record Value(ILegacyDataPoint DataPoint, float TypedValue) : ILegacyDataPointValue<float>
    {
        public bool IsInValueRange() => true;
    }
}
