using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;

/// <summary>
/// A configured <c>DINT</c> tag. The configuration-time half of <see cref="DIntDataPoint"/>: this is
/// what the manifest produces, that is what the client reads.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record DIntNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : ILogixScalarNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "DInt";

    /// <inheritdoc />
    public IConfigurationNode? Parent { get; set; }

    /// <inheritdoc />
    public Channels Channels { get; } = new(
        [.. OriginalNode.AffectedChannels.Select(static channel => new AffectedChannel(channel))],
        [.. OriginalNode.TransferredChannels.Select(static channel => new TransferredChannel(channel))]);
}
