using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;

/// <summary>
/// What every configured scalar tag carries whatever its type: where it sits in the tree, and the
/// channels the editor routed it to. Both are read off the untyped node and neither depends on the
/// type, so a concrete node adds only its <c>LinkedNodeTypeId</c> and whatever its own type needs —
/// a <c>STRING</c>'s declared capacity, an <c>LREAL</c>'s minimum generation.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal abstract record LogixScalarNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : ILogixScalarNode
{
    /// <inheritdoc />
    public IConfigurationNode? Parent { get; set; }

    /// <inheritdoc />
    public Channels Channels { get; } = new(
        [.. OriginalNode.AffectedChannels.Select(static channel => new AffectedChannel(channel))],
        [.. OriginalNode.TransferredChannels.Select(static channel => new TransferredChannel(channel))]);
}
