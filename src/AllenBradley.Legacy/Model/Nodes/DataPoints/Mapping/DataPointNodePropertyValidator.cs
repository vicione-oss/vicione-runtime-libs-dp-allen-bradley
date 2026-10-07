using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;

/// <summary>
/// Checks what every element node carries before it is mapped, whatever its file type: an element number
/// a data file can have, and a poll frequency that is a positive number of milliseconds.
/// </summary>
internal sealed class DataPointNodePropertyValidator : AbstractValidator<LinkedNode>
{
    /// <summary>The last element a data file of an SLC 500 or MicroLogix can have.</summary>
    internal const ushort LastElement = 255;

    public DataPointNodePropertyValidator()
    {
        MustHaveElementNumber();
        MustBeElementNumberWithinFile();
        MustHavePollFrequency();
        MustBePositivePollFrequency();
    }

    private void MustHaveElementNumber() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<ushort>(ILegacyDataPointNode.ElementNumberPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILegacyDataPointNode.ElementNumberPropertyName}' is required and must be a number.")
            .WithName(ILegacyDataPointNode.ElementNumberPropertyName);

    private void MustBeElementNumberWithinFile() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<ushort>(ILegacyDataPointNode.ElementNumberPropertyName) <= LastElement)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILegacyDataPointNode.ElementNumberPropertyName}' must be between 0 and {LastElement}.")
            .When(static node => node.HasPropertyOfType<ushort>(ILegacyDataPointNode.ElementNumberPropertyName))
            .WithName(ILegacyDataPointNode.ElementNumberPropertyName);

    private void MustHavePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<int>(ILegacyDataPointNode.PollFrequencyPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILegacyDataPointNode.PollFrequencyPropertyName}' is required and must be an integer.")
            .WithName(ILegacyDataPointNode.PollFrequencyPropertyName);

    private void MustBePositivePollFrequency() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<int>(ILegacyDataPointNode.PollFrequencyPropertyName) > 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{ILegacyDataPointNode.PollFrequencyPropertyName}' must be a positive integer.")
            .When(static node => node.HasPropertyOfType<int>(ILegacyDataPointNode.PollFrequencyPropertyName))
            .WithName(ILegacyDataPointNode.PollFrequencyPropertyName);
}
