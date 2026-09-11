using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.USIntArray.Mapping;

/// <summary>Maps a configured <c>USIntArray</c> node onto an <see cref="USIntArrayNode"/>.</summary>
internal sealed class USIntArrayNodeMapper : LogixArrayNodeMapper<USIntArrayNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => USIntArrayNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override USIntArrayNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
