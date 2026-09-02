using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.Mapping;

/// <summary>
/// Maps a controller-scope container. It reads no property and validates none: the container declares
/// none, because controller scope contributes no segment to a tag address.
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
    public ControllerTagsNode Map(LinkedNode node) => new(node, generation);

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => new();
}

/// <summary>Maps the tag container of a 5X70 controller, which has no <c>LREAL</c>.</summary>
internal sealed class ControllerTags5X70NodeMapper() : ControllerTagsNodeMapper(LogixGeneration.Logix5X70)
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ControllerTagsNode.Logix5X70LinkedNodeTypeId;
}

/// <summary>Maps the tag container of a 5X80 controller, which adds <c>LREAL</c>.</summary>
internal sealed class ControllerTags5X80NodeMapper() : ControllerTagsNodeMapper(LogixGeneration.Logix5X80)
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ControllerTagsNode.Logix5X80LinkedNodeTypeId;
}
