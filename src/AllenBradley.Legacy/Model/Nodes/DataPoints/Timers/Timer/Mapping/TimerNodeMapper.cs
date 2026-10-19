using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Timers.Timer.Mapping;

/// <summary>Maps a configured <c>Timer</c> node onto a <see cref="TimerNode"/>.</summary>
internal sealed class TimerNodeMapper : LegacyDataPointNodeMapper<TimerNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => TimerNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override TimerNode CreateNode(
        LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency) =>
        new(originalNode, elementNumber, pollFrequency);
}
