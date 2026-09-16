using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

/// <summary>
/// Checks what every tag node carries before it is mapped, whatever its type and shape: a well-formed
/// tag name and a poll frequency that is a positive number of milliseconds.
/// </summary>
internal sealed class DataPointNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public DataPointNodePropertyValidator()
    {
        Include(new TagNamePropertyValidator());
        MustHavePollFrequency();
        MustBePositivePollFrequency();
    }

    private void MustHavePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<int>(ILogixDataPointNode.PollFrequencyPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixDataPointNode.PollFrequencyPropertyName}' is required and must be an integer.")
            .WithName(ILogixDataPointNode.PollFrequencyPropertyName);

    private void MustBePositivePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<int>(ILogixDataPointNode.PollFrequencyPropertyName) > 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixDataPointNode.PollFrequencyPropertyName}' must be a positive integer.")
            .When(static node => node.HasPropertyOfType<int>(ILogixDataPointNode.PollFrequencyPropertyName))
            .WithName(ILogixDataPointNode.PollFrequencyPropertyName);
}
