using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>SINT</c> tag — an 8-bit signed integer, carried as <see cref="sbyte"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record SIntDataPoint(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<sbyte>(TagName, PollFrequency, Channels)
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.SInt;

    /// <inheritdoc />
    internal override ILogixDataPointValue<sbyte> CreateLogixValue(sbyte value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, sbyte TypedValue) : ILogixDataPointValue<sbyte>
    {
        // A SINT is exactly an sbyte: every value the type can hold, the tag can hold.
        public bool IsInValueRange() => true;
    }
}
