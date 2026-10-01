using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Counters;

/// <summary>
/// A configured <c>COUNTER</c> tag, the configuration-time half of <see cref="CounterDataPoint"/>. It names
/// the counter and no member of it.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The counter's name.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record CounterNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : LogixDataPointNode(OriginalNode, TagName, PollFrequency)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "Counter";

    public override AllenBradleyDataType DataType => AllenBradleyDataType.Counter;
}
