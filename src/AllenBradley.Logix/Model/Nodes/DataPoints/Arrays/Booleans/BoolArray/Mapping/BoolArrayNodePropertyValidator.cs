using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray.Mapping;

/// <summary>
/// The rules every array node has plus the one only a <c>BOOL</c> array has: a declared count that
/// fills whole words, which is the only count Studio 5000 will declare one with.
/// </summary>
internal sealed class BoolArrayNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public BoolArrayNodePropertyValidator()
    {
        Include(new ArrayDataPointNodePropertyValidator());
        MustFillWholeWords();
    }

    // Caught here rather than left to the connect: Studio 5000 cannot declare a BOOL[10], so a count
    // that is not a multiple of 32 is a mistake no controller is needed to recognise.
    private void MustFillWholeWords() =>
        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<uint>(LogixArrayDataPointNode.ElementCountPropertyName)
                % BoolArrayDataPoint.BoolsPerWord == 0)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property " +
                $"'{LogixArrayDataPointNode.ElementCountPropertyName}' must be a multiple of " +
                $"{BoolArrayDataPoint.BoolsPerWord}, because a BOOL array is packed into 32-bit words.")
            .When(static node =>
                node.HasPropertyOfType<uint>(LogixArrayDataPointNode.ElementCountPropertyName)
                && node.GetRequiredPropertyValue<uint>(LogixArrayDataPointNode.ElementCountPropertyName) > 0)
            .WithName(LogixArrayDataPointNode.ElementCountPropertyName);
}
