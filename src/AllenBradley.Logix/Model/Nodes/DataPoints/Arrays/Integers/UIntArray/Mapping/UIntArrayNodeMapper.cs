using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UIntArray.Mapping;

/// <summary>Maps a configured <c>UIntArray</c> node onto an <see cref="UIntArrayDataPointNode"/>.</summary>
internal sealed class UIntArrayNodeMapper : LogixArrayNodeMapper<UIntArrayDataPointNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => UIntArrayDataPointNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override UIntArrayDataPointNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
