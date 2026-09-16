using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;

/// <summary>A Logix <c>STRING</c> tag — carried as <see cref="string"/>.</summary>
/// <param name="TagPath">Where the tag's value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
/// <param name="MaxLength">
/// The declared character capacity; <see cref="StringMaxLength.Standard"/> for the built-in <c>STRING</c>.
/// </param>
public sealed record StringDataPoint(
    TagPath TagPath,
    PollFrequency PollFrequency,
    Channels Channels,
    StringMaxLength MaxLength)
    : LogixDataPoint<string>(TagPath, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.String;

    /// <inheritdoc />
    internal override ILogixDataPointValue<string> CreateLogixValue(string value) => new Value(this, value);

    private sealed record Value(StringDataPoint StringDataPoint, string TypedValue)
        : ILogixDataPointValue<string>
    {
        public ILogixDataPoint DataPoint => StringDataPoint;

        // One byte per character in .DATA, so the character count is the byte count.
        public bool IsInValueRange() => TypedValue.Length <= StringDataPoint.MaxLength.Value;
    }
}
