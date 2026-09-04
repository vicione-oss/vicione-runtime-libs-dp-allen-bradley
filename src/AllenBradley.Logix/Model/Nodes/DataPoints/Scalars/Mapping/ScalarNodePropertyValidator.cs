using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Mapping;

/// <summary>
/// Checks what a scalar node carries before it is mapped: a tag name Studio 5000 could have declared,
/// and a poll frequency that is a positive number of milliseconds.
/// The name is the bare one, never an address; the tree walk composes the scope prefix. Whether the
/// controller has the tag is verified against the symbol table on connect.
/// </summary>
internal sealed class ScalarNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ScalarNodePropertyValidator()
    {
        MustHaveTagName();
        MustBeValidTagName();
        MustHavePollFrequency();
        MustBePositivePollFrequency();
    }

    private void MustHaveTagName() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<string>(ILogixScalarNode.TagNamePropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixScalarNode.TagNamePropertyName}' is required and must be a string.")
            .WithName(ILogixScalarNode.TagNamePropertyName);

    private void MustBeValidTagName() =>
        RuleFor(static node => node)
            .Must(static node =>
                LogixIdentifier.IsWellFormed(
                    node.GetRequiredPropertyValue<string>(ILogixScalarNode.TagNamePropertyName)))
            .WithMessage(static node =>
                $"The tag name '{node.GetRequiredPropertyValue<string>(ILogixScalarNode.TagNamePropertyName)}' in node '{node.Name}' ({node.DesignId}) is not a valid Logix tag name.")
            .When(static node => node.HasPropertyOfType<string>(ILogixScalarNode.TagNamePropertyName))
            .WithName(ILogixScalarNode.TagNamePropertyName);

    private void MustHavePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<int>(ILogixScalarNode.PollFrequencyPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixScalarNode.PollFrequencyPropertyName}' is required and must be an integer.")
            .WithName(ILogixScalarNode.PollFrequencyPropertyName);

    private void MustBePositivePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<int>(ILogixScalarNode.PollFrequencyPropertyName) > 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixScalarNode.PollFrequencyPropertyName}' must be a positive integer.")
            .When(static node => node.HasPropertyOfType<int>(ILogixScalarNode.PollFrequencyPropertyName))
            .WithName(ILogixScalarNode.PollFrequencyPropertyName);
}
