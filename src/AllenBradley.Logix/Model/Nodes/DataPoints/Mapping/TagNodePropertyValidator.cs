using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

/// <summary>
/// Checks what every tag node carries before it is mapped, whatever its type and shape: a tag name
/// Studio 5000 could have declared — the bare one, never an address, since the tree walk composes the
/// scope prefix — and a poll frequency that is a positive number of milliseconds.
/// </summary>
internal sealed class TagNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public TagNodePropertyValidator()
    {
        MustHaveTagName();
        MustBeValidTagName();
        MustHavePollFrequency();
        MustBePositivePollFrequency();
    }

    private void MustHaveTagName() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<string>(ILogixTagNode.TagNamePropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixTagNode.TagNamePropertyName}' is required and must be a string.")
            .WithName(ILogixTagNode.TagNamePropertyName);

    private void MustBeValidTagName() =>
        RuleFor(static node => node)
            .Must(static node =>
                LogixIdentifier.IsWellFormed(
                    node.GetRequiredPropertyValue<string>(ILogixTagNode.TagNamePropertyName)))
            .WithMessage(static node =>
                $"The tag name '{node.GetRequiredPropertyValue<string>(ILogixTagNode.TagNamePropertyName)}' in node '{node.Name}' ({node.DesignId}) is not a valid Logix tag name.")
            .When(static node => node.HasPropertyOfType<string>(ILogixTagNode.TagNamePropertyName))
            .WithName(ILogixTagNode.TagNamePropertyName);

    private void MustHavePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<int>(ILogixTagNode.PollFrequencyPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixTagNode.PollFrequencyPropertyName}' is required and must be an integer.")
            .WithName(ILogixTagNode.PollFrequencyPropertyName);

    private void MustBePositivePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<int>(ILogixTagNode.PollFrequencyPropertyName) > 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixTagNode.PollFrequencyPropertyName}' must be a positive integer.")
            .When(static node => node.HasPropertyOfType<int>(ILogixTagNode.PollFrequencyPropertyName))
            .WithName(ILogixTagNode.PollFrequencyPropertyName);
}
