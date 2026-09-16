using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray.Mapping;

/// <summary>Maps a configured <c>IntArray</c> node onto an <see cref="IntArrayDataPointNode"/>.</summary>
internal sealed class IntArrayNodeMapper : LogixArrayNodeMapper<IntArrayDataPointNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => IntArrayDataPointNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override IntArrayDataPointNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
