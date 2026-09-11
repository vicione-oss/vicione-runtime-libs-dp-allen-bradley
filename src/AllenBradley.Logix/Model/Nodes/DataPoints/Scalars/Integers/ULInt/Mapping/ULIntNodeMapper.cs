using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.ULInt.Mapping;

/// <summary>Maps a configured <c>ULInt</c> node onto an <see cref="ULIntNode"/>.</summary>
internal sealed class ULIntNodeMapper : LogixTagNodeMapper<ULIntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ULIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override ULIntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
