using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Floats.Float.Mapping;

/// <summary>Maps a configured <c>Float</c> node onto a <see cref="FloatNode"/>.</summary>
internal sealed class FloatNodeMapper : LegacyDataPointNodeMapper<FloatNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => FloatNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override FloatNode CreateNode(
        LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency) =>
        new(originalNode, elementNumber, pollFrequency);
}
