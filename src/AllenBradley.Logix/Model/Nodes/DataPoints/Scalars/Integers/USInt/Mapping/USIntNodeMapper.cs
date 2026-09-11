using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt.Mapping;

/// <summary>Maps a configured <c>USInt</c> node onto an <see cref="USIntNode"/>.</summary>
internal sealed class USIntNodeMapper : LogixTagNodeMapper<USIntNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => USIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override USIntNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency);
}
