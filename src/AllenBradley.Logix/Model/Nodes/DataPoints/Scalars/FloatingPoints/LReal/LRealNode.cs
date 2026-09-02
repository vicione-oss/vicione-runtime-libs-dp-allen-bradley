using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;

/// <summary>
/// A configured <c>LREAL</c> tag. The configuration-time half of <see cref="LRealDataPoint"/>: this is
/// what the manifest produces, that is what the client reads.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record LRealNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : ILogixScalarNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "LReal";

    /// <summary>
    /// The 5X80 controllers are the ones that have an <c>LREAL</c>; a 5X70 cannot resolve a tag of this
    /// type at all.
    /// </summary>
    /// <remarks>
    /// Implemented explicitly, because it is the one thing this node says that is not configuration: the
    /// YAML consistency test reads a data point node's public properties and expects every one of them to
    /// be a property the manifest declares.
    /// </remarks>
    LogixGeneration ILogixScalarNode.MinimumGeneration => LogixGeneration.Logix5X80;

    /// <inheritdoc />
    public IConfigurationNode? Parent { get; set; }

    /// <inheritdoc />
    public Channels Channels { get; } = new(
        [.. OriginalNode.AffectedChannels.Select(static channel => new AffectedChannel(channel))],
        [.. OriginalNode.TransferredChannels.Select(static channel => new TransferredChannel(channel))]);
}
