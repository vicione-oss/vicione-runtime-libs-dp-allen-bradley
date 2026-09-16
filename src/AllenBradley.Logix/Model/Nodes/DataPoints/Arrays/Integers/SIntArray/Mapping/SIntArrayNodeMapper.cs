using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.SIntArray.Mapping;

/// <summary>Maps a configured <c>SIntArray</c> node onto an <see cref="SIntArrayDataPointNode"/>.</summary>
internal sealed class SIntArrayNodeMapper : LogixArrayNodeMapper<SIntArrayDataPointNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => SIntArrayDataPointNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override SIntArrayDataPointNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
