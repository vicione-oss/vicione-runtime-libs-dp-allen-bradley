using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>UDINT</c> tag — a 32-bit unsigned integer, carried as <see cref="uint"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record UDIntDataPoint(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<uint>(TagName, PollFrequency, Channels)
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.UDInt;

    /// <inheritdoc />
    internal override ILogixDataPointValue<uint> CreateLogixValue(uint value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, uint TypedValue) : ILogixDataPointValue<uint>
    {
        // A UDINT is exactly a uint: every value the type can hold, the tag can hold.
        public bool IsInValueRange() => true;
    }
}
