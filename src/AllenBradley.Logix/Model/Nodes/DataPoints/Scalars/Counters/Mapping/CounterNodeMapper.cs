using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Counters.Mapping;

/// <summary>Maps a configured <c>Counter</c> node onto a <see cref="CounterNode"/>.</summary>
internal sealed class CounterNodeMapper : LogixTagNodeMapper<CounterNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => CounterNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override CounterNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
