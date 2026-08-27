using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;

/// <summary>
/// A Logix <c>STRING</c> tag — carried as <see cref="string"/>.
/// </summary>
/// <remarks>
/// A scalar in the model and a structure on the wire: one value, not an array, but stored as
/// <c>.LEN : DINT</c> + <c>.DATA : SINT[n]</c>. Unlike the elementary types, that shape is not fixed by
/// the type alone, so the point carries its own <paramref name="MaxLength" /> — the <c>n</c> that sizes
/// the buffer and that verification holds the controller to.
/// </remarks>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
/// <param name="MaxLength">The declared character capacity; <see cref="StringMaxLength.Standard"/> for the built-in <c>STRING</c>.</param>
public sealed record StringDataPoint(
    TagName TagName, PollFrequency PollFrequency, Channels Channels, StringMaxLength MaxLength)
    : LogixDataPoint<string>(TagName, PollFrequency, Channels)
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.String;

    /// <inheritdoc />
    internal override ILogixDataPointValue<string> CreateLogixValue(string value) => new Value(this, value);

    // The only modelled type whose range is not its .NET type: a string longer than the tag was
    // declared to hold has nowhere to go. The value keeps the concrete point rather than the interface,
    // because the capacity it is judged against is the point's own configuration.
    private sealed record Value(StringDataPoint StringDataPoint, string TypedValue)
        : ILogixDataPointValue<string>
    {
        public ILogixDataPoint DataPoint => StringDataPoint;

        // One byte per character in .DATA, so the character count is the byte count. This is the gate
        // that turns an over-long write into a reported validation failure; LogixStringConverter still
        // throws on one, for the callers that reach the client without passing through here.
        public bool IsInValueRange() => TypedValue.Length <= StringDataPoint.MaxLength.Value;
    }
}
