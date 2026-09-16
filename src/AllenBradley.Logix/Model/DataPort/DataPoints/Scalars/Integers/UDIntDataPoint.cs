using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>UDINT</c> tag — a 32-bit unsigned integer, carried as <see cref="uint"/>.</summary>
/// <param name="TagPath">Where the tag's value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record UDIntDataPoint(TagPath TagPath, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<uint>(TagPath, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Udint;

    /// <inheritdoc />
    internal override ILogixDataPointValue<uint> CreateLogixValue(uint value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, uint TypedValue) : ILogixDataPointValue<uint>
    {
        public bool IsInValueRange() => true;
    }
}
