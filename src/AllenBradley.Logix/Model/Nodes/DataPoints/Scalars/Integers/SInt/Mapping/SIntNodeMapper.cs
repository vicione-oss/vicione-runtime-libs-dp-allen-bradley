using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt.Mapping;

/// <summary>Maps a configured <c>SInt</c> node onto an <see cref="SIntNode"/>.</summary>
internal sealed class SIntNodeMapper : LogixTagNodeMapper<SIntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => SIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override SIntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
