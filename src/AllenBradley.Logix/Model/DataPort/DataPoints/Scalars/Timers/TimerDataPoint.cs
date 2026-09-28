using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;

/// <summary>
/// A Logix <c>TIMER</c> tag, carried as its accumulated time in milliseconds, as <see cref="int"/>. The
/// point is the timer: it is verified and named as the tag the user configured, and only its
/// <see cref="HandleAddress"/> reaches inside it.
/// </summary>
/// <param name="TagPath">Where the timer lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record TimerDataPoint(TagPath TagPath, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<int>(TagPath, PollFrequency, Channels)
{
    private static readonly UdtMemberName AccumulatedValueMember = new("ACC");

    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Timer;

    /// <summary>
    /// The timer's <c>.ACC</c> member, which is all this point reads and writes. Writing the whole
    /// structure would put back <c>.PRE</c> and status bits the scan has changed since they were read.
    /// </summary>
    public override TagAddress HandleAddress => AccumulatedValuePath.ToTagAddress();

    private TagPath AccumulatedValuePath => TagPath with
    {
        UdtMemberPath = TagPath.UdtMemberPath is { } members
            ? members.Append(AccumulatedValueMember)
            : UdtMemberPath.Of(AccumulatedValueMember),
    };

    /// <summary>
    /// Whether <paramref name="milliseconds"/> may be written: a negative accumulated time is a major
    /// fault, and the controller stops all logic.
    /// </summary>
    internal static bool IsWritableAccumulatedTime(int milliseconds) => milliseconds >= 0;

    /// <inheritdoc />
    internal override ILogixDataPointValue<int> CreateLogixValue(int value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, int TypedValue) : ILogixDataPointValue<int>
    {
        public bool IsInValueRange() => IsWritableAccumulatedTime(TypedValue);
    }
}
