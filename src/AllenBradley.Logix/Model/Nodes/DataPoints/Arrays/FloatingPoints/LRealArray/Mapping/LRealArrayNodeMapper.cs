using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray.Mapping;

/// <summary>Maps a configured <c>LRealArray</c> node onto an <see cref="LRealArrayNode"/>.</summary>
internal sealed class LRealArrayNodeMapper : LogixArrayNodeMapper<LRealArrayNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => LRealArrayNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override LRealArrayNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
