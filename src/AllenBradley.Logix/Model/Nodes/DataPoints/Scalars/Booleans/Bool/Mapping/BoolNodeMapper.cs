using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool.Mapping;

/// <summary>Maps a configured <c>Bool</c> node onto a <see cref="BoolNode"/>.</summary>
internal sealed class BoolNodeMapper : LogixTagNodeMapper<BoolNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => BoolNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override BoolNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
