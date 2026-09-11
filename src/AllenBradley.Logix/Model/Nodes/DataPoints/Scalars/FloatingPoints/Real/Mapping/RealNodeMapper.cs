using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.Real.Mapping;

/// <summary>Maps a configured <c>Real</c> node onto a <see cref="RealNode"/>.</summary>
internal sealed class RealNodeMapper : LogixTagNodeMapper<RealNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => RealNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override RealNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
