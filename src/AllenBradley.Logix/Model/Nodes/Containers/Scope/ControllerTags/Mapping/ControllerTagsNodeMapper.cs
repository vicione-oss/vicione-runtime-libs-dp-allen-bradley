using FluentValidation.Results;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags.Mapping;

/// <summary>
/// Maps a controller-scope container, which carries no property of its own. One subclass per container
/// node type, because a mapper is resolved by <see cref="TargetLinkedNodeTypeId"/> alone.
/// </summary>
internal abstract class ControllerTagsNodeMapper(string linkedNodeTypeId)
    : IBranchConfigurationNodeMapper<ControllerTagsNode>
{
    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => linkedNodeTypeId;

    /// <inheritdoc />
    public ControllerTagsNode Map(LinkedNode node) =>
        new(node, ControllerTagsNode.GenerationOf(linkedNodeTypeId));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => new();
}

/// <summary>Maps the tag container of a 5X70 controller.</summary>
internal sealed class ControllerTags5X70NodeMapper()
    : ControllerTagsNodeMapper(ControllerTagsNode.Logix5X70LinkedNodeTypeId);

/// <summary>Maps the tag container of a 5X80 controller.</summary>
internal sealed class ControllerTags5X80NodeMapper()
    : ControllerTagsNodeMapper(ControllerTagsNode.Logix5X80LinkedNodeTypeId);
