using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>LINT</c> tag — a 64-bit signed integer, carried as <see cref="long"/>.</summary>
/// <param name="TagAddress">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record LIntDataPoint(TagAddress TagAddress, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<long>(TagAddress, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Lint;

    /// <inheritdoc />
    internal override ILogixDataPointValue<long> CreateLogixValue(long value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, long TypedValue) : ILogixDataPointValue<long>
    {
        public bool IsInValueRange() => true;
    }
}
