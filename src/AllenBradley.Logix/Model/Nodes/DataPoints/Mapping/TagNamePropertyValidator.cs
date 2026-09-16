using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

/// <summary>
/// Checks that a node names a tag Studio 5000 could have declared — the bare one, never an address,
/// since the tree walk composes the scope prefix.
/// </summary>
internal sealed class TagNamePropertyValidator : AbstractValidator<LinkedNode>
{
    public TagNamePropertyValidator()
    {
        MustHaveTagName();
        MustBeValidTagName();
    }

    private void MustHaveTagName() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<string>(ILogixDataPointNode.TagNamePropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixDataPointNode.TagNamePropertyName}' is required and must be a string.")
            .WithName(ILogixDataPointNode.TagNamePropertyName);

    private void MustBeValidTagName() =>
        RuleFor(static node => node)
            .Must(static node =>
                TagName.IsWellFormed(
                    node.GetRequiredPropertyValue<string>(ILogixDataPointNode.TagNamePropertyName)))
            .WithMessage(static node =>
                $"The tag name '{node.GetRequiredPropertyValue<string>(ILogixDataPointNode.TagNamePropertyName)}' in node '{node.Name}' ({node.DesignId}) is not a valid Logix tag name.")
            .When(static node => node.HasPropertyOfType<string>(ILogixDataPointNode.TagNamePropertyName))
            .WithName(ILogixDataPointNode.TagNamePropertyName);
}
