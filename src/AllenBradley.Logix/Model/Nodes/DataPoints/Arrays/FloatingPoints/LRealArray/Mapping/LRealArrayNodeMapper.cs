using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray.Mapping;

/// <summary>Maps a configured <c>LRealArray</c> node onto an <see cref="LRealArrayDataPointNode"/>.</summary>
internal sealed class LRealArrayNodeMapper : LogixArrayNodeMapper<LRealArrayDataPointNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => LRealArrayDataPointNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override LRealArrayDataPointNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
