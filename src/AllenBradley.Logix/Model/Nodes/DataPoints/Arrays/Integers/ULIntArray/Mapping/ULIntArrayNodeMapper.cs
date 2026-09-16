using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.ULIntArray.Mapping;

/// <summary>Maps a configured <c>ULIntArray</c> node onto an <see cref="UlIntArrayDataPointNode"/>.</summary>
internal sealed class ULIntArrayNodeMapper : LogixArrayNodeMapper<UlIntArrayDataPointNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => UlIntArrayDataPointNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override UlIntArrayDataPointNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
