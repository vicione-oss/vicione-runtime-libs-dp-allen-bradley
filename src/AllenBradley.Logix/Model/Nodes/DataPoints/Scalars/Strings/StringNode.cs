using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;

/// <summary>
/// A configured <c>STRING</c> tag. The configuration-time half of <see cref="StringDataPoint"/>: this is
/// what the manifest produces, that is what the client reads.
/// </summary>
/// <remarks>
/// It carries one thing the elementary scalars do not — the declared capacity. A string's size follows
/// its declaration rather than its type (a <c>STRING_20</c> is the same structure and 24 bytes), so the
/// capacity is configuration, and it is what verification holds the controller to.
/// </remarks>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
/// <param name="MaxLength">The character capacity the tag is declared with in Studio 5000.</param>
internal sealed record StringNode(
    LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency, StringMaxLength MaxLength)
    : ILogixScalarNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "String";

    /// <summary>The manifest property carrying <see cref="MaxLength"/>.</summary>
    public const string MaxLengthPropertyName = nameof(MaxLength);

    /// <inheritdoc />
    public IConfigurationNode? Parent { get; set; }

    /// <inheritdoc />
    public Channels Channels { get; } = new(
        [.. OriginalNode.AffectedChannels.Select(static channel => new AffectedChannel(channel))],
        [.. OriginalNode.TransferredChannels.Select(static channel => new TransferredChannel(channel))]);
}
