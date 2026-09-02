using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;

/// <summary>
/// Maps a program-scope container, reading the one property it carries: the program whose tags are
/// configured under it.
/// </summary>
/// <remarks>
/// Whether the controller actually has a program of that name is settled against the symbol table on
/// connect, not here. That the name is one Studio 5000 could have declared is decidable from the
/// configuration alone, and <see cref="ProgramTagsNodePropertyValidator"/> is where that is decided.
/// <para>
/// One subclass per container node type, for the reason controller scope has two: the manifest gives
/// each generation its own <c>MappingId</c>, which is what puts the generation on the mapped node, and
/// the YAML consistency test resolves a node's mapper by <see cref="TargetLinkedNodeTypeId"/> alone.
/// </para>
/// </remarks>
/// <param name="generation">The generation the node type this mapper claims stands for.</param>
internal abstract class ProgramTagsNodeMapper(LogixGeneration generation)
    : IBranchConfigurationNodeMapper<ProgramTagsNode>
{
    private readonly ProgramTagsNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public abstract string TargetLinkedNodeTypeId { get; }

    /// <inheritdoc />
    public ProgramTagsNode Map(LinkedNode node) => new(
        node,
        new ProgramName(node.GetRequiredPropertyValue<string>(ProgramTagsNode.ProgramNamePropertyName)),
        generation);

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}

/// <summary>Maps a program's tag container on a 5X70 controller, which has no <c>LREAL</c>.</summary>
internal sealed class ProgramTags5X70NodeMapper() : ProgramTagsNodeMapper(LogixGeneration.Logix5X70)
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ProgramTagsNode.Logix5X70LinkedNodeTypeId;
}

/// <summary>Maps a program's tag container on a 5X80 controller, which adds <c>LREAL</c>.</summary>
internal sealed class ProgramTags5X80NodeMapper() : ProgramTagsNodeMapper(LogixGeneration.Logix5X80)
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => ProgramTagsNode.Logix5X80LinkedNodeTypeId;
}
