using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;

/// <summary>
/// The controller-scope tag container. It declares no properties, because controller scope contributes
/// no segment to a tag address: a tag configured under it addresses itself. See
/// <c>explanation/tag-scoping.md</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="Generation">
/// The generation of the controller these tags are configured against, which decides which types may
/// hang off this container.
/// </param>
internal sealed record ControllerTagsNode(
    LinkedNode OriginalNode,
    LogixGeneration Generation) : ILogixContainerNode
{
    /// <summary>The manifest's <c>MappingId</c> for a 5X70 controller's tag container.</summary>
    internal const string Logix5X70LinkedNodeTypeId = "ControllerTags5X70";

    /// <summary>The manifest's <c>MappingId</c> for a 5X80 controller's tag container.</summary>
    internal const string Logix5X80LinkedNodeTypeId = "ControllerTags5X80";

    /// <summary>
    /// The generation the container node type <paramref name="linkedNodeTypeId"/> stands for.
    /// </summary>
    internal static LogixGeneration GenerationOf(string linkedNodeTypeId) => linkedNodeTypeId switch
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
    internal static string LinkedNodeTypeIdFor(LogixGeneration generation) => generation switch
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

    /// <summary>Nothing nests inside controller scope yet — structures are a later slice.</summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => false;

    /// <summary>Whether a tag's type is one this controller's generation has.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) =>
        dataPointNode is not ILogixDataPointNode tagNode || tagNode.MinimumGeneration <= Generation;
}
