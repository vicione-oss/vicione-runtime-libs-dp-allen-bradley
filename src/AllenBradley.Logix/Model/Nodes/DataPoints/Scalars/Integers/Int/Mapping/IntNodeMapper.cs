using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int.Mapping;

/// <summary>Maps a configured <c>Int</c> node onto an <see cref="IntNode"/>.</summary>
internal sealed class IntNodeMapper : LogixTagNodeMapper<IntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => IntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override IntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
