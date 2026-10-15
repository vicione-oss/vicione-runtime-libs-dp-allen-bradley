using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Long.Mapping;

/// <summary>Maps a configured <c>Long</c> node onto a <see cref="LongNode"/>.</summary>
internal sealed class LongNodeMapper : LegacyDataPointNodeMapper<LongNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => LongNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override LongNode CreateNode(
        LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency) =>
        new(originalNode, elementNumber, pollFrequency);
}
