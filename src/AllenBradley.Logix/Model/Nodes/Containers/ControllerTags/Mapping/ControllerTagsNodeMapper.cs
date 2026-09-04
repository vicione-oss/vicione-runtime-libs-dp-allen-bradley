using FluentValidation.Results;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.Mapping;

/// <summary>
/// Maps a controller-scope container. It reads no property and validates none: the container declares
/// none, because controller scope contributes no segment to a tag address.
/// One subclass per container node type: the YAML consistency test resolves a node's mapper by <see
/// cref="TargetLinkedNodeTypeId"/> alone, so one mapper cannot claim both ids.
/// </summary>
/// <param name="linkedNodeTypeId">
/// The node type this mapper claims. The generation comes off it through
/// <see cref="ControllerTagsNode.GenerationOf"/>, so a subclass names the pairing once.
/// </param>
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

/// <summary>Maps the tag container of a 5X70 controller, which has no <c>LREAL</c>.</summary>
internal sealed class ControllerTags5X70NodeMapper()
    : ControllerTagsNodeMapper(ControllerTagsNode.Logix5X70LinkedNodeTypeId);

/// <summary>Maps the tag container of a 5X80 controller, which adds <c>LREAL</c>.</summary>
internal sealed class ControllerTags5X80NodeMapper()
    : ControllerTagsNodeMapper(ControllerTagsNode.Logix5X80LinkedNodeTypeId);
