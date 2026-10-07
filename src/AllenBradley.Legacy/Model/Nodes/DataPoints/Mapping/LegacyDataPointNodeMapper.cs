using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;

/// <summary>
/// What every element node mapper does whatever the file type: validate the configured node and read the
/// element number and poll frequency off it. A concrete mapper adds only the <c>LinkedNodeTypeId</c> it
/// claims and the constructor call.
/// </summary>
/// <typeparam name="TNode">The node the mapper produces.</typeparam>
internal abstract class LegacyDataPointNodeMapper<TNode> : IDataPointNodeMapper<TNode>
    where TNode : LegacyDataPointNode
{
    private static readonly DataPointNodePropertyValidator Validator = new();

    /// <inheritdoc />
    public abstract string TargetLinkedNodeTypeId { get; }

    /// <inheritdoc />
    public TNode Map(LinkedNode node) => CreateNode(node, GetElementNumber(node), GetPollFrequency(node));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => Validator.Validate(linkedNode);

    protected abstract TNode CreateNode(LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency);

    private static ElementNumber GetElementNumber(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<ushort>(ILegacyDataPointNode.ElementNumberPropertyName));

    private static PollFrequency GetPollFrequency(LinkedNode node) =>
        PollFrequency.FromMilliseconds(
            node.GetRequiredPropertyValue<int>(ILegacyDataPointNode.PollFrequencyPropertyName));
}
