using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Timers.Mapping;

/// <summary>Maps a configured <c>Timer</c> node onto a <see cref="TimerNode"/>.</summary>
internal sealed class TimerNodeMapper : LogixTagNodeMapper<TimerNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => TimerNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override TimerNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
