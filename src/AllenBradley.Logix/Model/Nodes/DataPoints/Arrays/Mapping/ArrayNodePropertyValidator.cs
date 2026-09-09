using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;

/// <summary>
/// The rules every tag node has plus the one only an array has: a declared count that is a positive
/// number of elements.
/// </summary>
internal sealed class ArrayNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ArrayNodePropertyValidator()
    {
        Include(new TagNodePropertyValidator());
        MustHaveElementCount();
        MustBePositiveElementCount();
    }

    private void MustHaveElementCount() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<int>(LogixArrayNode.ElementCountPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{LogixArrayNode.ElementCountPropertyName}' is required and must be an integer.")
            .WithName(LogixArrayNode.ElementCountPropertyName);

    private void MustBePositiveElementCount() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<int>(LogixArrayNode.ElementCountPropertyName) > 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{LogixArrayNode.ElementCountPropertyName}' must be a positive integer.")
            .When(static node => node.HasPropertyOfType<int>(LogixArrayNode.ElementCountPropertyName))
            .WithName(LogixArrayNode.ElementCountPropertyName);
}
