using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Timers;

/// <summary>
/// A configured <c>TIMER</c> tag, the configuration-time half of <see cref="TimerDataPoint"/>. It names the
/// timer and no member of it.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The timer's name.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record TimerNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : LogixDataPointNode(OriginalNode, TagName, PollFrequency)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "Timer";

    public override AllenBradleyDataType DataType => AllenBradleyDataType.Timer;
}
