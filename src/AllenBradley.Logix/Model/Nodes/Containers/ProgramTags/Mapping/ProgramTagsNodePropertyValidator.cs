using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags.Mapping;

/// <summary>
/// Checks the one thing a program container carries: a program name Studio 5000 could have declared.
/// </summary>
internal sealed class ProgramTagsNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ProgramTagsNodePropertyValidator()
    {
        MustBeValidProgramName();
    }

    // Skipped when the property is absent or not a string, which the required-property rule reports on
    // its own; without the guard this would throw out of GetRequiredPropertyValue instead.
    private void MustBeValidProgramName() =>
        RuleFor(static node => node)
            .Must(static node =>
                LogixIdentifier.IsWellFormed(
                    node.GetRequiredPropertyValue<string>(ProgramTagsNode.ProgramNamePropertyName)))
            .WithMessage(static node =>
                $"The program name '{node.GetRequiredPropertyValue<string>(ProgramTagsNode.ProgramNamePropertyName)}' in node '{node.Name}' ({node.DesignId}) is not a valid Logix program name.")
            .When(static node => node.HasPropertyOfType<string>(ProgramTagsNode.ProgramNamePropertyName))
            .WithName(ProgramTagsNode.ProgramNamePropertyName);
}
