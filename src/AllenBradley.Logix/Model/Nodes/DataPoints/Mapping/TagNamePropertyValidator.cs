using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

/// <summary>
/// Checks that a node names a tag Studio 5000 could have declared — the bare one, never an address,
/// since the tree walk composes the scope prefix. A node under an array container is the exception:
/// it names no tag but an element, so it carries a subscript, <c>[n]</c>, instead.
/// </summary>
internal sealed class TagNamePropertyValidator : AbstractValidator<LinkedNode>
{
    public TagNamePropertyValidator()
    {
        MustHaveTagName();
        When(HasTagName, () => When(IsArrayElement, MustBeValidSubscript).Otherwise(MustBeValidTagName));
    }

    private void MustHaveTagName() =>
        RuleFor(static node => node)
            .Must(static node => HasTagName(node))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixDataPointNode.TagNamePropertyName}' is required and must be a string.")
            .WithName(ILogixDataPointNode.TagNamePropertyName);

    private void MustBeValidTagName() =>
        RuleFor(static node => node)
            .Must(static node => TagName.IsWellFormed(GetTagName(node)))
            .WithMessage(static node =>
                $"The tag name '{GetTagName(node)}' in node '{node.Name}' ({node.DesignId}) is not a valid Logix tag name.")
            .WithName(ILogixDataPointNode.TagNamePropertyName);

    private void MustBeValidSubscript() =>
        RuleFor(static node => node)
            .Must(static node => TagName.IsWellFormedSubscript(GetTagName(node)))
            .WithMessage(static node =>
                $"The tag name '{GetTagName(node)}' in node '{node.Name}' ({node.DesignId}) is not a valid array subscript: an element under an array container is addressed as '[n]' with n a non-negative integer.")
            .WithName(ILogixDataPointNode.TagNamePropertyName);

    private static bool HasTagName(LinkedNode node) =>
        node.HasPropertyOfType<string>(ILogixDataPointNode.TagNamePropertyName);

    private static string GetTagName(LinkedNode node) =>
        node.GetRequiredPropertyValue<string>(ILogixDataPointNode.TagNamePropertyName);

    private static bool IsArrayElement(LinkedNode node) =>
        node.Parent is { } parent && ArrayContainerNode.IsArrayContainer(parent);
}
