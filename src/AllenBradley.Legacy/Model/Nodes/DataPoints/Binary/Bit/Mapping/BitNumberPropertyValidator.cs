using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit.Mapping;

/// <summary>Checks a bit node's bit number: an element is one 16-bit word, so bit 15 is its last bit.</summary>
internal sealed class BitNumberPropertyValidator : AbstractValidator<LinkedNode>
{
    internal const ushort LastBit = 15;

    public BitNumberPropertyValidator()
    {
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<ushort>(BitNode.BitNumberPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{BitNode.BitNumberPropertyName}' is required and must be a number.")
            .WithName(BitNode.BitNumberPropertyName);

        RuleFor(static node => node)
            .Must(static node => node.GetRequiredPropertyValue<ushort>(BitNode.BitNumberPropertyName) <= LastBit)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{BitNode.BitNumberPropertyName}' must be between 0 and {LastBit}.")
            .When(static node => node.HasPropertyOfType<ushort>(BitNode.BitNumberPropertyName))
            .WithName(BitNode.BitNumberPropertyName);
    }
}
