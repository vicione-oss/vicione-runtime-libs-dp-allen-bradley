using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;

/// <summary>
/// What every configured element carries whatever its file type: where it sits in the tree, and the
/// channels the editor routed it to. A concrete node adds only its <c>LinkedNodeTypeId</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ElementNumber">The element inside the parent data file.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal abstract record LegacyDataPointNode(LinkedNode OriginalNode, ElementNumber ElementNumber, PollFrequency PollFrequency)
    : ILegacyDataPointNode
{
    /// <inheritdoc />
    public IConfigurationNode? Parent { get; set; }

    /// <inheritdoc />
    public Channels Channels { get; } = new(
        [.. OriginalNode.AffectedChannels.Select(static channel => new AffectedChannel(channel))],
        [.. OriginalNode.TransferredChannels.Select(static channel => new TransferredChannel(channel))]);

    /// <inheritdoc />
    public abstract DataFileType FileType { get; }
}
