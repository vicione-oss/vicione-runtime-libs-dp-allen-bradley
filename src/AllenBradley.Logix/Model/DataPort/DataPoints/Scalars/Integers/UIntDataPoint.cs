using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>UINT</c> tag — a 16-bit unsigned integer, carried as <see cref="ushort"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record UIntDataPoint(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<ushort>(TagName, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Uint;

    /// <inheritdoc />
    internal override ILogixDataPointValue<ushort> CreateLogixValue(ushort value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, ushort TypedValue) : ILogixDataPointValue<ushort>
    {
        public bool IsInValueRange() => true;
    }
}
