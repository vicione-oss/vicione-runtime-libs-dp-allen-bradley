using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer.Mapping;

/// <summary>Maps a configured <c>Integer</c> node onto an <see cref="IntegerNode"/>.</summary>
internal sealed class IntegerNodeMapper : LegacyDataPointNodeMapper<IntegerNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => IntegerNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override IntegerNode CreateNode(
        LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency) =>
        new(originalNode, elementNumber, pollFrequency);
}
