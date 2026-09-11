using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt.Mapping;

/// <summary>Maps a configured <c>LInt</c> node onto an <see cref="LIntNode"/>.</summary>
internal sealed class LIntNodeMapper : LogixTagNodeMapper<LIntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => LIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override LIntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
