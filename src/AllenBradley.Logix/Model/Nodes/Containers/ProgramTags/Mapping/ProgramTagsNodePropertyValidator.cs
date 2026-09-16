using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;

/// <summary>
/// Checks the one thing a program container carries: a program name that is there, and that Studio 5000
/// could have declared.
/// </summary>
internal sealed class ProgramTagsNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ProgramTagsNodePropertyValidator()
    {
        MustHaveProgramName();
        MustBeValidProgramName();
    }

    private void MustHaveProgramName() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<string>(ProgramTagsNode.ProgramNamePropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ProgramTagsNode.ProgramNamePropertyName}' is required and must be a string.")
            .WithName(ProgramTagsNode.ProgramNamePropertyName);

    private void MustBeValidProgramName() =>
        RuleFor(static node => node)
            .Must(static node =>
                TagName.IsWellFormed(
                    node.GetRequiredPropertyValue<string>(ProgramTagsNode.ProgramNamePropertyName)))
            .WithMessage(static node =>
                $"The program name '{node.GetRequiredPropertyValue<string>(ProgramTagsNode.ProgramNamePropertyName)}' in node '{node.Name}' ({node.DesignId}) is not a valid Logix program name.")
            .When(static node => node.HasPropertyOfType<string>(ProgramTagsNode.ProgramNamePropertyName))
            .WithName(ProgramTagsNode.ProgramNamePropertyName);
}
