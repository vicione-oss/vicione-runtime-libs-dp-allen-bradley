using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer.Mapping;

/// <summary>
/// Maps a UDT container, reading the one property it carries: the tag name, or the member name
/// when nested. One subclass per container node type, because a mapper is resolved by
/// <see cref="TargetLinkedNodeTypeId"/> alone.
/// </summary>
internal abstract class UdtContainerNodeMapper(string linkedNodeTypeId)
    : IBranchConfigurationNodeMapper<UdtContainerNode>
{
    private readonly UdtContainerNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => linkedNodeTypeId;

    /// <inheritdoc />
    public UdtContainerNode Map(LinkedNode node) => new(
        node,
        TagNamePropertyExtractor.GetTagName(node),
        UdtContainerNode.GenerationOf(linkedNodeTypeId));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}

/// <summary>Maps a UDT container on a 5X70 controller.</summary>
internal sealed class Udt5X70NodeMapper()
    : UdtContainerNodeMapper(UdtContainerNode.Logix5X70LinkedNodeTypeId);

/// <summary>Maps a UDT container on a 5X80 controller.</summary>
internal sealed class Udt5X80NodeMapper()
    : UdtContainerNodeMapper(UdtContainerNode.Logix5X80LinkedNodeTypeId);
