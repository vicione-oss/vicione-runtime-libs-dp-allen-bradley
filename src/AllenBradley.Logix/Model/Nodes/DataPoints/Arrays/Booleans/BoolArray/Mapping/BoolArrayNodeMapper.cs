using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray.Mapping;

/// <summary>Maps a configured <c>BoolArray</c> node onto a <see cref="BoolArrayNode"/>.</summary>
internal sealed class BoolArrayNodeMapper() : LogixArrayNodeMapper<BoolArrayNode>(
    new BoolArrayNodePropertyValidator())
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => BoolArrayNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override BoolArrayNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
