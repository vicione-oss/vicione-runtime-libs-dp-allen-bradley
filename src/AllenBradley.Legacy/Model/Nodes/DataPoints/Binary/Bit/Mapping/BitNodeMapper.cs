using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit.Mapping;

/// <summary>Maps a configured <c>Bit</c> node onto a <see cref="BitNode"/>.</summary>
internal sealed class BitNodeMapper : LegacyDataPointNodeMapper<BitNode>
{
    private static readonly BitNumberPropertyValidator BitNumberValidator = new();

    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => BitNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public override ValidationResult Validate(LinkedNode linkedNode) =>
        new([.. base.Validate(linkedNode).Errors, .. BitNumberValidator.Validate(linkedNode).Errors]);

    /// <inheritdoc />
    protected override BitNode CreateNode(
        LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency) =>
        new(originalNode, elementNumber, GetBitNumber(originalNode), pollFrequency);

    private static BitNumber GetBitNumber(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<ushort>(BitNode.BitNumberPropertyName));
}
