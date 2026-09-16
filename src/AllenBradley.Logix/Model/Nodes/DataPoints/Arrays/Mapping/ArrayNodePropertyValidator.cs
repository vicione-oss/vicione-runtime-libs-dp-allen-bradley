using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;

internal sealed class ArrayNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ArrayNodePropertyValidator()
    {
        MustHaveElementCount();
        MustBePositiveElementCount();
    }


    private void MustHaveElementCount() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<uint>(LogixArrayDataPointNode.ElementCountPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{LogixArrayDataPointNode.ElementCountPropertyName}' is required and must be an unsigned integer.")
            .WithName(LogixArrayDataPointNode.ElementCountPropertyName);

    private void MustBePositiveElementCount() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<uint>(LogixArrayDataPointNode.ElementCountPropertyName) > 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{LogixArrayDataPointNode.ElementCountPropertyName}' must be a positive integer.")
            .When(static node => node.HasPropertyOfType<uint>(LogixArrayDataPointNode.ElementCountPropertyName))
            .WithName(LogixArrayDataPointNode.ElementCountPropertyName);
}
