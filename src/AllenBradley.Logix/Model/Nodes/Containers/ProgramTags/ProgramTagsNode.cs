using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
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
/// <para>
/// The generation split controller scope carries is mirrored here, for the same reason and by the same
/// means: one type for the manifest's two containers, which differ only in what they may hold, and the
/// generation arriving with the node rather than from the device above — nothing above a container is
/// reachable while <see cref="IConfigurationNode.CanBeAdded(IDataPointNode)"/> runs. What a scope may
/// hold is the same question in both scopes, so <see cref="ITagScopeNode"/> answers it for both: an
/// <c>LREAL</c> is missing from a 5x70 whether it was configured under a program or under the controller.
/// </para>
/// </remarks>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ProgramName">The program these tags live in, and the segment their addresses carry.</param>
/// <param name="Generation">
/// The generation of the controller these tags are configured against, from the container's own node
/// type. It decides which types may hang off this container.
/// </param>
public sealed record ProgramTagsNode(
    LinkedNode OriginalNode,
    ProgramName ProgramName,
    LogixGeneration Generation) : ITagScopeNode
{
    /// <summary>The manifest's <c>MappingId</c> for a program's tag container on a 5x70 controller.</summary>
    public const string Logix5x70LinkedNodeTypeId = "ProgramTags5x70";

    /// <summary>The manifest's <c>MappingId</c> for a program's tag container on a 5x80 controller.</summary>
    public const string Logix5x80LinkedNodeTypeId = "ProgramTags5x80";

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

    /// <inheritdoc />
    public TagScope Scope() => TagScope.Program(ProgramName);
}
