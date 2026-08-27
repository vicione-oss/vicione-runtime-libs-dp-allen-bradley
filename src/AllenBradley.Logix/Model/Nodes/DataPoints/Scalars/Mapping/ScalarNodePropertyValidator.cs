using System.Text.RegularExpressions;
using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Mapping;

/// <summary>
/// Checks what a scalar node carries before it is mapped: a tag name Studio 5000 could have declared,
/// and a poll frequency that is a positive number of milliseconds.
/// </summary>
/// <remarks>
/// Whether the controller actually has that tag, and whether it has the type the node claims, is
/// verified against the symbol table on connect. This validator only rejects what is decidable from the
/// configuration alone.
/// </remarks>
internal sealed partial class ScalarNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ScalarNodePropertyValidator()
    {
        MustHaveTagName();
        MustBeValidTagName();
        MustHavePollFrequency();
        MustBePositivePollFrequency();
    }

    /// <summary>
    /// The Studio 5000 rule for a tag name:
    /// <list type="bullet">
    ///   <item><c>(?=.{1,40}\z)</c> — 1 to 40 characters in total.</item>
    ///   <item><c>[A-Za-z_]</c> — starts with a letter or an underscore, never a digit.</item>
    ///   <item><c>(?:_?[A-Za-z0-9])*</c> — letters, digits and single underscores after that; two
    ///     underscores in a row and a trailing underscore are both rejected.</item>
    /// </list>
    /// A lone <c>"_"</c> passes this pattern and Studio 5000 would not accept it — a name no controller
    /// declares, which the symbol-table verification rejects anyway.
    /// </summary>
    [GeneratedRegex(@"\A(?=.{1,40}\z)[A-Za-z_](?:_?[A-Za-z0-9])*\z")]
    private static partial Regex ValidTagName();

    private void MustHaveTagName() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<string>(ILogixScalarNode.TagNamePropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILogixScalarNode.TagNamePropertyName}' is required and must be a string.")
            .WithName(ILogixScalarNode.TagNamePropertyName);

    private void MustBeValidTagName() =>
        RuleFor(static node => node)
            .Must(static node =>
                ValidTagName().IsMatch(node.GetRequiredPropertyValue<string>(ILogixScalarNode.TagNamePropertyName)))
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
