using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.LIntArray.Mapping;

/// <summary>Maps a configured <c>LIntArray</c> node onto an <see cref="LIntArrayNode"/>.</summary>
internal sealed class LIntArrayNodeMapper : LogixArrayNodeMapper<LIntArrayNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => LIntArrayNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override LIntArrayNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
