using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.RealArray.Mapping;

/// <summary>Maps a configured <c>RealArray</c> node onto a <see cref="RealArrayDataPointNode"/>.</summary>
internal sealed class RealArrayNodeMapper : LogixArrayNodeMapper<RealArrayDataPointNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => RealArrayDataPointNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override RealArrayDataPointNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount) =>
        new(originalNode, tagName, pollFrequency, elementCount);
}
