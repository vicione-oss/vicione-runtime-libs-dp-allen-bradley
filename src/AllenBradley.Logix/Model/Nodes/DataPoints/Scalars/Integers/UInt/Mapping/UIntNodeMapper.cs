using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UInt.Mapping;

/// <summary>Maps a configured <c>UInt</c> node onto an <see cref="UIntNode"/>.</summary>
internal sealed class UIntNodeMapper : LogixTagNodeMapper<UIntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => UIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override UIntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
