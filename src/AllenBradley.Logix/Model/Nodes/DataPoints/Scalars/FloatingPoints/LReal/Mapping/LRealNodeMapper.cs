using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal.Mapping;

/// <summary>Maps a configured <c>LReal</c> node onto an <see cref="LRealNode"/>.</summary>
internal sealed class LRealNodeMapper : LogixTagNodeMapper<LRealNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => LRealNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override LRealNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
