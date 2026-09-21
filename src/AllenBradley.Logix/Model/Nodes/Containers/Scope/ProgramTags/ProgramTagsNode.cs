using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;

/// <summary>
/// A program-scope tag container: one program of the controller, and the tags configured inside it. A
/// peer of controller scope under the device, the way Studio 5000's own tree puts them. See
/// <c>explanation/tag-scoping.md</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ProgramName">The program these tags live in, and the segment their addresses carry.</param>
/// <param name="Generation">
/// The generation of the controller these tags are configured against, which decides which types may
/// hang off this container.
/// </param>
internal sealed record ProgramTagsNode(
    LinkedNode OriginalNode,
    ProgramName ProgramName,
    LogixGeneration Generation) : ILogixContainerNode
{
    /// <summary>The manifest's <c>MappingId</c> for a program's tag container on a 5X70 controller.</summary>
    internal const string Logix5X70LinkedNodeTypeId = "ProgramTags5X70";

    /// <summary>The manifest's <c>MappingId</c> for a program's tag container on a 5X80 controller.</summary>
    internal const string Logix5X80LinkedNodeTypeId = "ProgramTags5X80";

    /// <summary>The manifest property carrying <see cref="ProgramName"/>.</summary>
    internal const string ProgramNamePropertyName = nameof(ProgramName);

    /// <summary>
    /// The generation the container node type <paramref name="linkedNodeTypeId"/> stands for.
    /// </summary>
    internal static LogixGeneration GenerationOf(string linkedNodeTypeId) => linkedNodeTypeId switch
    {
        Logix5X70LinkedNodeTypeId => LogixGeneration.Logix5X70,
        Logix5X80LinkedNodeTypeId => LogixGeneration.Logix5X80,
        _ => throw new ArgumentOutOfRangeException(
            nameof(linkedNodeTypeId), $"'{linkedNodeTypeId}' is no program-scope container."),
    };

    /// <summary>
    /// The container node type a <paramref name="generation"/> controller holds a program's tags in —
    /// <see cref="GenerationOf"/> read the other way round.
    /// </summary>
    internal static string LinkedNodeTypeIdFor(LogixGeneration generation) => generation switch
    {
        LogixGeneration.Logix5X70 => Logix5X70LinkedNodeTypeId,
        LogixGeneration.Logix5X80 => Logix5X80LinkedNodeTypeId,
        _ => throw new ArgumentOutOfRangeException(
            nameof(generation), $"No program-scope container for a {generation} controller."),
    };

    /// <inheritdoc />
    public IConfigurationNode? ParentConfigurationNode { get; set; }

    /// <inheritdoc />
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];

    /// <inheritdoc />
    public List<IDataPointNode> DataPointNodes { get; } = [];

    /// <summary>
    /// An array container of a type this controller's generation has, or a UDT container of this
    /// controller's generation, and nothing else. No program nests inside a program: Studio 5000 v32 and later let programs nest, but whether the resulting tags
    /// address as <c>Program:Parent.Child.RootTagName</c> is unconfirmed against hardware.
    /// </summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => configurationNode switch
    {
        ArrayContainerNode arrayContainer => arrayContainer.ArrayDataType.MinimumGeneration <= Generation,
        UdtContainerNode udtContainer => udtContainer.Generation == Generation,
        _ => false,
    };

    /// <summary>Whether a tag's type is one this controller's generation has.</summary>
    public bool CanBeAdded(IDataPointNode dataPointNode) =>
        dataPointNode is not ILogixDataPointNode tagNode || tagNode.MinimumGeneration <= Generation;
}
