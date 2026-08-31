using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;

/// <summary>
/// The controller-scope tag container. It declares no properties, because controller scope contributes
/// no segment to a tag address: a tag configured under it addresses itself. Program scope is the one
/// that prefixes — <c>Program:MainProgram.Count</c> — and <c>ProgramTagsNode</c> is where its
/// <c>ProgramName</c> lives.
/// </summary>
/// <remarks>
/// One type for the manifest's two containers, which differ only in what they may hold. The generation
/// arrives with the node rather than from the device node above, because the engine assembles a
/// container's data points before it attaches the container to anything: nothing above it is reachable
/// while <see cref="CanBeAdded(IDataPointNode)"/> runs. So this node takes its own node type's word for
/// the generation, and <see cref="Device.DeviceNode.CanBeAdded(IConfigurationNode)"/> is where that word
/// is held against the device's when the container is finally attached.
/// </remarks>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="Generation">
/// The generation of the controller these tags are configured against, from the container's own node
/// type. It decides which types may hang off this container.
/// </param>
public sealed record ControllerTagsNode(
    LinkedNode OriginalNode,
    LogixGeneration Generation) : ILogixTagScopeNode
{
    /// <summary>The manifest's <c>MappingId</c> for a 5x70 controller's tag container.</summary>
    public const string Logix5x70LinkedNodeTypeId = "ControllerTags5x70";

    /// <summary>The manifest's <c>MappingId</c> for a 5x80 controller's tag container.</summary>
    public const string Logix5x80LinkedNodeTypeId = "ControllerTags5x80";

    /// <inheritdoc />
    public IConfigurationNode? ParentConfigurationNode { get; set; }

    /// <inheritdoc />
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];

    /// <inheritdoc />
    public List<IDataPointNode> DataPointNodes { get; } = [];

    /// <summary>Nothing nests inside controller scope yet — structures and programs are later slices.</summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => false;

    /// <summary>
    /// Any tag the controller has a type for. The manifest already keeps a 5x70's editor from offering
    /// an <c>LREAL</c> — the <c>ControllerTags5x70</c> node does not list one — so a configuration that
    /// reaches here holding one was not built through the editor, and it names a type the controller
    /// cannot resolve.
    /// </summary>
    public bool CanBeAdded(IDataPointNode dataPointNode)
    {
        if (dataPointNode is LRealNode && Generation is LogixGeneration.Logix5x70)
        {
            throw new InvalidConfigurationException(
                "LREAL is not a data type of a Logix 5x70 controller.");
        }

        return true;
    }

    /// <inheritdoc />
    public TagScope Scope() => TagScope.Controller;
}
