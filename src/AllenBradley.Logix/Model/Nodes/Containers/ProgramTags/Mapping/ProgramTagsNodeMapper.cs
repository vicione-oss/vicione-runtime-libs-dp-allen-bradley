using FluentValidation.Results;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;

/// <summary>
/// Maps a program-scope container, reading the one property it carries: the program whose tags are
/// configured under it. One subclass per container node type, because a mapper is resolved by
/// <see cref="TargetLinkedNodeTypeId"/> alone.
/// </summary>
internal abstract class ProgramTagsNodeMapper(string linkedNodeTypeId)
    : IBranchConfigurationNodeMapper<ProgramTagsNode>
{
    private readonly ProgramTagsNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => linkedNodeTypeId;

    /// <inheritdoc />
    public ProgramTagsNode Map(LinkedNode node) => new(
        node,
        new ProgramName(node.GetRequiredPropertyValue<string>(ProgramTagsNode.ProgramNamePropertyName)),
        ProgramTagsNode.GenerationOf(linkedNodeTypeId));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}

/// <summary>Maps a program's tag container on a 5X70 controller.</summary>
internal sealed class ProgramTags5X70NodeMapper()
    : ProgramTagsNodeMapper(ProgramTagsNode.Logix5X70LinkedNodeTypeId);

/// <summary>Maps a program's tag container on a 5X80 controller.</summary>
internal sealed class ProgramTags5X80NodeMapper()
    : ProgramTagsNodeMapper(ProgramTagsNode.Logix5X80LinkedNodeTypeId);
