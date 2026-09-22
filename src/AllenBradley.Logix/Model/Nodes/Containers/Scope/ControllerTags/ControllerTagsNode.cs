using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;

/// <summary>
/// The controller-scope tag container. It declares no properties, because controller scope contributes
/// no segment to a tag address: a tag configured under it addresses itself. See
/// <c>explanation/model/node-model.md</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="Generation">
/// The generation of the controller these tags are configured against, which decides which types may
/// hang off this container.
/// </param>
internal sealed record ControllerTagsNode(
    LinkedNode OriginalNode,
    LogixGeneration Generation) : LogixContainerNode(OriginalNode)
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

    /// <summary>
    /// An array container of a type this controller's generation has, or a UDT container of this
    /// controller's generation, and nothing else.
    /// </summary>
    internal override bool CanBeAdded(ILogixContainerNode logixContainerNode) => logixContainerNode switch
    {
        ArrayContainerNode arrayContainer => arrayContainer.ArrayDataType.MinimumGeneration <= Generation,
        UdtContainerNode udtContainer => udtContainer.Generation == Generation,
        _ => false,
    };

    /// <summary>Whether a tag's type is one this controller's generation has.</summary>
    internal override bool CanBeAdded(ILogixDataPointNode logixDataPointNode) =>
        logixDataPointNode.MinimumGeneration <= Generation;
}
