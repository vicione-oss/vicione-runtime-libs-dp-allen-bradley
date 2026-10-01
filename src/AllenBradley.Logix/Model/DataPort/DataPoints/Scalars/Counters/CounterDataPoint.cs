using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;

/// <summary>
/// A Logix <c>COUNTER</c> tag, carried as its accumulated value in counts, as <see cref="int"/>. The
/// point is the counter: it is verified and named as the tag the user configured, and only its
/// <see cref="HandleAddress"/> reaches inside it.
/// </summary>
/// <param name="TagPath">Where the counter lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record CounterDataPoint(TagPath TagPath, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<int>(TagPath, PollFrequency, Channels)
{
    private static readonly UdtMemberName AccumulatedValueMember = new("ACC");

    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Counter;

    /// <summary>
    /// The counter's <c>.ACC</c> member, which is all this point reads and writes. Writing the whole
    /// structure would put back <c>.PRE</c> and status bits the scan has changed since they were read.
    /// </summary>
    // Appended to the rendered address, because a TagPath holds no member behind an element: Parts[3].ACC.
    public override TagAddress HandleAddress => new($"{TagAddress.Value}.{AccumulatedValueMember.Value}");

    /// <inheritdoc />
    internal override ILogixDataPointValue<int> CreateLogixValue(int value) => new Value(this, value);

    // Every DINT is a valid count: CTD counts below zero, and the controller flags a wrap with .OV or .UN.
    private sealed record Value(ILogixDataPoint DataPoint, int TypedValue) : ILogixDataPointValue<int>
    {
        public bool IsInValueRange() => true;
    }
}
