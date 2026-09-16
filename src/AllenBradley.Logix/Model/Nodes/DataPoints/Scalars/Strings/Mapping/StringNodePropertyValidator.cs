using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings.Mapping;

/// <summary>
/// The rules every tag node has plus the one only a string has: a declared capacity that is a positive
/// number of characters.
/// </summary>
internal sealed class StringNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public StringNodePropertyValidator()
    {
        Include(new DataPointNodePropertyValidator());
        MustHaveMaxLength();
        MustBePositiveMaxLength();
    }

    private void MustHaveMaxLength() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<int>(StringNode.MaxLengthPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{StringNode.MaxLengthPropertyName}' is required and must be an integer.")
            .WithName(StringNode.MaxLengthPropertyName);

    private void MustBePositiveMaxLength() =>
        RuleFor(static node => node)
            .Must(static node => node.GetRequiredPropertyValue<int>(StringNode.MaxLengthPropertyName) > 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{StringNode.MaxLengthPropertyName}' must be a positive integer.")
            .When(static node => node.HasPropertyOfType<int>(StringNode.MaxLengthPropertyName))
            .WithName(StringNode.MaxLengthPropertyName);
}
