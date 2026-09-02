using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;

/// <summary>
/// The controller-scope tag container. It declares no properties, because controller scope contributes
/// no segment to a tag address: a tag configured under it addresses itself. Program scope is the one
/// that prefixes — <c>Program:MainProgram.Count</c> — and <c>ProgramTagsNode</c> is where its
/// <c>ProgramName</c> lives.
/// </summary>
/// <remarks>
/// One type for the manifest's two containers, which differ only in what they may hold — a difference
/// <see cref="IConfigurationNode.CanBeAdded(IDataPointNode)"/> enforces for both scopes at once. The
/// generation arrives with the node rather than from the device node above, because the engine assembles
/// a container's data points before it attaches the container to anything: nothing above it is reachable
/// while that guard runs. So this node takes its own node type's word for the generation, and
/// <see cref="Device.DeviceNode.CanBeAdded(IConfigurationNode)"/> is where that word is held against the
/// device's when the container is finally attached.
/// </remarks>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="Generation">
/// The generation of the controller these tags are configured against, from the container's own node
/// type. It decides which types may hang off this container.
/// </param>
public sealed record ControllerTagsNode(
    LinkedNode OriginalNode,
    LogixGeneration Generation) : ITagScopeNode
{
    /// <summary>The manifest's <c>MappingId</c> for a 5X70 controller's tag container.</summary>
    public const string Logix5X70LinkedNodeTypeId = "ControllerTags5X70";

    /// <summary>The manifest's <c>MappingId</c> for a 5X80 controller's tag container.</summary>
    public const string Logix5X80LinkedNodeTypeId = "ControllerTags5X80";

    /// <inheritdoc />
    public IConfigurationNode? ParentConfigurationNode { get; set; }

    /// <inheritdoc />
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];

    /// <inheritdoc />
    public List<IDataPointNode> DataPointNodes { get; } = [];

    /// <summary>Nothing nests inside controller scope yet — structures and programs are later slices.</summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => false;

    /// <inheritdoc />
    public TagScope Scope() => TagScope.Controller;
}
