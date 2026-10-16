using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;

/// <summary>One element of a string (<c>ST</c>) file — up to 82 ASCII characters, carried as <see cref="string"/>.</summary>
/// <param name="FileNumber">The number of the string file.</param>
/// <param name="ElementNumber">The element inside it.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record StringDataPoint(
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    PollFrequency PollFrequency,
    Channels Channels)
    : LegacyDataPoint<string>(new DataFileAddress(DataFileType.String, FileNumber, ElementNumber), PollFrequency, Channels)
{
    /// <summary>How many characters an element holds. Every <c>ST</c> element has the same capacity.</summary>
    public const int MaxLength = 82;

    /// <inheritdoc />
    internal override ILegacyDataPointValue<string> CreateLegacyValue(string value) => new Value(this, value);

    private sealed record Value(ILegacyDataPoint DataPoint, string TypedValue) : ILegacyDataPointValue<string>
    {
        public bool IsInValueRange() => TypedValue.Length <= MaxLength && TypedValue.All(char.IsAscii);
    }
}
