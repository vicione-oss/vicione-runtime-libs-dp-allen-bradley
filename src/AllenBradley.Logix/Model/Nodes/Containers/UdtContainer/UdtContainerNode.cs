using System.Collections.Frozen;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;

/// <summary>
/// A tag of a user-defined data type opened into its members. Each child is one member, named as the
/// member, and a member that is itself a UDT is another container nested here. The container states
/// no type: the controller's template says what the members are, and verification checks each child
/// against it. Whether a predefined structure — <c>TIMER</c>, <c>COUNTER</c>, <c>STRING</c> — or an
/// Add-On Instruction is opened the same way is not decided; this node is for UDTs.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">
/// The tag name under a scope container; the member name under another UDT container.
/// </param>
/// <param name="Generation">
/// The generation of the controller this UDT is configured against, which decides which member types
/// may hang off it — the same gate the scope above it has, carried down so a nested UDT gates the same
/// way.
/// </param>
internal sealed record UdtContainerNode(
    LinkedNode OriginalNode,
    TagName TagName,
    LogixGeneration Generation) : LogixContainerNode(OriginalNode)
{
    /// <summary>The manifest's <c>MappingId</c> for a UDT on a 5X70 controller.</summary>
    internal const string Logix5X70LinkedNodeTypeId = "Udt5X70";

    /// <summary>The manifest's <c>MappingId</c> for a UDT on a 5X80 controller.</summary>
    internal const string Logix5X80LinkedNodeTypeId = "Udt5X80";

    private static readonly FrozenSet<string> LinkedNodeTypeIds = FrozenSet.Create(
        StringComparer.Ordinal, Logix5X70LinkedNodeTypeId, Logix5X80LinkedNodeTypeId);

    /// <summary>The generation the container node type <paramref name="linkedNodeTypeId"/> stands for.</summary>
    internal static LogixGeneration GenerationOf(string linkedNodeTypeId) => linkedNodeTypeId switch
    {
        Logix5X70LinkedNodeTypeId => LogixGeneration.Logix5X70,
        Logix5X80LinkedNodeTypeId => LogixGeneration.Logix5X80,
        _ => throw new ArgumentOutOfRangeException(
            nameof(linkedNodeTypeId), $"'{linkedNodeTypeId}' is no UDT container."),
    };

    /// <summary>
    /// The container node type a <paramref name="generation"/> controller opens a UDT with —
    /// <see cref="GenerationOf"/> read the other way round.
    /// </summary>
    internal static string LinkedNodeTypeIdFor(LogixGeneration generation) => generation switch
    {
        LogixGeneration.Logix5X70 => Logix5X70LinkedNodeTypeId,
        LogixGeneration.Logix5X80 => Logix5X80LinkedNodeTypeId,
        _ => throw new ArgumentOutOfRangeException(
            nameof(generation), $"No UDT container for a {generation} controller."),
    };

    internal static bool IsUdtContainer(LinkedNode node) => LinkedNodeTypeIds.Contains(node.DesignId);

    /// <summary>
    /// A nested UDT of this controller's own generation, and nothing else. An array container is
    /// not accepted yet: an element behind a member path is an address the port has not proved.
    /// </summary>
    internal override bool CanBeAdded(ILogixContainerNode logixContainerNode) =>
        logixContainerNode is UdtContainerNode nested && nested.Generation == Generation;

    /// <summary>Whether a member's type is one this controller's generation has.</summary>
    internal override bool CanBeAdded(ILogixDataPointNode logixDataPointNode) =>
        logixDataPointNode.MinimumGeneration <= Generation;
}
