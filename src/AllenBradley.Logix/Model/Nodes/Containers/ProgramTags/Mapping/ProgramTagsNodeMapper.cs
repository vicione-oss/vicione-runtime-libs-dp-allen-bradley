using FluentValidation.Results;
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
/// </remarks>
internal sealed class ProgramTagsNodeMapper : IBranchConfigurationNodeMapper<ProgramTagsNode>
{
    private readonly ProgramTagsNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => ProgramTagsNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public ProgramTagsNode Map(LinkedNode node) => new(
        node,
        new ProgramName(node.GetRequiredPropertyValue<string>(ProgramTagsNode.ProgramNamePropertyName)));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}
