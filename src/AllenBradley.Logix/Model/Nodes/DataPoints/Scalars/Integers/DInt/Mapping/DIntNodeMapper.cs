using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt.Mapping;

/// <summary>Maps a configured <c>DInt</c> node onto a <see cref="DIntNode"/>.</summary>
internal sealed class DIntNodeMapper : LogixTagNodeMapper<DIntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => DIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override DIntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
