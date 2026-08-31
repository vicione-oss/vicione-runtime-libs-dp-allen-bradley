using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.Mapping;

/// <summary>
/// Maps a controller-scope container. It reads one property and validates none: the container's
/// <see cref="ControllerTagsNode.ControllerName"/> is a placeholder that reaches no tag address.
/// </summary>
/// <remarks>
/// One subclass per container node type, because the manifest gives each generation its own
/// <c>MappingId</c> — which is what puts the generation on the mapped node, where the container needs it
/// to gate its own children. Claiming both ids from a single mapper through <c>IsTargetMapperFor</c>
/// would work at run time and fail the YAML consistency test, which resolves a node's mapper by
/// <see cref="TargetLinkedNodeTypeId"/> alone.
/// </remarks>
/// <param name="generation">The generation the node type this mapper claims stands for.</param>
internal abstract class ControllerTagsNodeMapper(LogixGeneration generation)
    : IBranchConfigurationNodeMapper<ControllerTagsNode>
{
    /// <inheritdoc />
    public abstract string TargetLinkedNodeTypeId { get; }

    /// <inheritdoc />
    public ControllerTagsNode Map(LinkedNode node) => new(
        node,
        node.GetRequiredPropertyValue<string>(ControllerTagsNode.ControllerNamePropertyName),
        generation);

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => new();
}

/// <summary>Maps the tag container of a 5x70 controller, which has no <c>LREAL</c>.</summary>
internal sealed class ControllerTags5x70NodeMapper() : ControllerTagsNodeMapper(LogixGeneration.Logix5x70)
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ControllerTagsNode.Logix5x70LinkedNodeTypeId;
}

/// <summary>Maps the tag container of a 5x80 controller, which adds <c>LREAL</c>.</summary>
internal sealed class ControllerTags5x80NodeMapper() : ControllerTagsNodeMapper(LogixGeneration.Logix5x80)
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ControllerTagsNode.Logix5x80LinkedNodeTypeId;
}
