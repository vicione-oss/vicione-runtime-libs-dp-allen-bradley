using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.ULIntArray.Mapping;

/// <summary>Maps a configured <c>ULIntArray</c> node onto an <see cref="ULIntArrayNode"/>.</summary>
internal sealed class ULIntArrayNodeMapper : LogixArrayNodeMapper<ULIntArrayNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ULIntArrayNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override ULIntArrayNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
