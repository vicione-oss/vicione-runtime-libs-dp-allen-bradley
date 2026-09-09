using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int.Mapping;

/// <summary>Maps a configured <c>Int</c> node onto an <see cref="IntNode"/>.</summary>
internal sealed class IntNodeMapper : IDataPointNodeMapper<IntNode>
{
    private readonly TagNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => IntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public IntNode Map(LinkedNode node) => new(
        node,
        new TagName(node.GetRequiredPropertyValue<string>(ILogixTagNode.TagNamePropertyName)),
        PollFrequency.FromMilliseconds(
            node.GetRequiredPropertyValue<int>(ILogixTagNode.PollFrequencyPropertyName)));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}
