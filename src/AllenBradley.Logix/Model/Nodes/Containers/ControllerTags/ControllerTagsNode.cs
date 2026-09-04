using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;

/// <summary>
/// The controller-scope tag container. It declares no properties, because controller scope contributes
/// no segment to a tag address: a tag configured under it addresses itself. Program scope is the one
/// that prefixes — <c>Program:MainProgram.Count</c> — and <c>ProgramTagsNode</c> is where its
/// <c>ProgramName</c> lives.
/// The generation arrives with the node type rather than from the device above, because the engine assembles
/// a container's data points before it attaches the container to anything; <see
/// cref="Device.DeviceNode.CanBeAdded(IConfigurationNode)"/> holds it against the device's later.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="Generation">
/// The generation of the controller these tags are configured against, from the container's own node
/// type. It decides which types may hang off this container./mode
/// </param>
public sealed record ControllerTagsNode(
    LinkedNode OriginalNode,
    LogixGeneration Generation) : ITagScopeNode
{
    /// <summary>The manifest's <c>MappingId</c> for a 5X70 controller's tag container.</summary>
    public const string Logix5X70LinkedNodeTypeId = "ControllerTags5X70";

    /// <summary>The manifest's <c>MappingId</c> for a 5X80 controller's tag container.</summary>
    public const string Logix5X80LinkedNodeTypeId = "ControllerTags5X80";

    /// <summary>
    /// The generation the container node type <paramref name="linkedNodeTypeId"/> stands for.
    /// </summary>
    public static LogixGeneration GenerationOf(string linkedNodeTypeId) => linkedNodeTypeId switch
    {
        Logix5X70LinkedNodeTypeId => LogixGeneration.Logix5X70,
        Logix5X80LinkedNodeTypeId => LogixGeneration.Logix5X80,
        _ => throw new ArgumentOutOfRangeException(
            nameof(linkedNodeTypeId), $"'{linkedNodeTypeId}' is no controller-scope container."),
    };

    /// <summary>
    /// The container node type a <paramref name="generation"/> controller holds its controller-scope tags
    /// in — <see cref="GenerationOf"/> read the other way round.
    /// </summary>
    public static string LinkedNodeTypeIdFor(LogixGeneration generation) => generation switch
    {
        LogixGeneration.Logix5X70 => Logix5X70LinkedNodeTypeId,
        LogixGeneration.Logix5X80 => Logix5X80LinkedNodeTypeId,
        _ => throw new ArgumentOutOfRangeException(
            nameof(generation), $"No controller-scope container for a {generation} controller."),
    };

    /// <inheritdoc />
    public IConfigurationNode? ParentConfigurationNode { get; set; }

    /// <inheritdoc />
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];

    /// <inheritdoc />
    public List<IDataPointNode> DataPointNodes { get; } = [];

    /// <summary>Nothing nests inside controller scope yet — structures and programs are later slices.</summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => false;

    /// <summary>Whether a tag's type is one this controller's generation has. See <see cref="ITagScopeNode.CanHold"/>.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) => ITagScopeNode.CanHold(dataPointNode, Generation);

    /// <inheritdoc />
    public TagScope Scope() => TagScope.Controller;
}
