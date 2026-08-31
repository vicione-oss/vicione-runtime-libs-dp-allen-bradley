using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;

/// <summary>
/// A program-scope tag container: one program of the controller, and the tags configured inside it. It
/// is a peer of controller scope under the device rather than a child of it, the way Studio 5000's own
/// tree puts them.
/// </summary>
/// <remarks>
/// Unlike controller scope, this container carries a property that reaches an address: every tag below
/// it addresses as <c>Program:{ProgramName}.{TagName}</c>. The prefix is composed during the tree walk
/// from the segment this node supplies, so a configured tag name stays bare and the leaf stays ignorant
/// of which scope it hangs under.
/// </remarks>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ProgramName">The program these tags live in, and the segment their addresses carry.</param>
public sealed record ProgramTagsNode(
    LinkedNode OriginalNode,
    ProgramName ProgramName) : IBranchConfigurationNode
{
    /// <summary>The manifest's <c>MappingId</c> for a program's tag container.</summary>
    public const string LinkedNodeTypeId = "ProgramTags";

    /// <summary>The manifest property carrying <see cref="ProgramName"/>.</summary>
    public const string ProgramNamePropertyName = nameof(ProgramName);

    /// <inheritdoc />
    public IConfigurationNode? ParentConfigurationNode { get; set; }

    /// <inheritdoc />
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];

    /// <inheritdoc />
    public List<IDataPointNode> DataPointNodes { get; } = [];

    /// <summary>
    /// Nothing nests inside a program. Studio 5000 v32 and later let programs nest, but whether the
    /// resulting tags address as <c>Program:Parent.Child.Tag</c> is unconfirmed against hardware.
    /// </summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => false;

    /// <summary>Any tag the controller has a type for.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) => true;
}
