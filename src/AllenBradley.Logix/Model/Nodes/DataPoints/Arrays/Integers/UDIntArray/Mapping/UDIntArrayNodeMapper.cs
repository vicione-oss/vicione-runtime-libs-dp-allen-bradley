using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UDIntArray.Mapping;

/// <summary>Maps a configured <c>UDIntArray</c> node onto an <see cref="UDIntArrayNode"/>.</summary>
internal sealed class UDIntArrayNodeMapper : LogixArrayNodeMapper<UDIntArrayNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => UDIntArrayNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override UDIntArrayNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
