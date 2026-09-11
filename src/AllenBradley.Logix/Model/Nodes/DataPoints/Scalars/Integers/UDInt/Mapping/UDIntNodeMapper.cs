using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UDInt.Mapping;

/// <summary>Maps a configured <c>UDInt</c> node onto an <see cref="UDIntNode"/>.</summary>
internal sealed class UDIntNodeMapper : LogixTagNodeMapper<UDIntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => UDIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override UDIntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
