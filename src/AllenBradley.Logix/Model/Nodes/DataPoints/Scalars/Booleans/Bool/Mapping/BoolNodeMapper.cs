using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool.Mapping;

/// <summary>Maps a configured <c>Bool</c> node onto a <see cref="BoolNode"/>.</summary>
internal sealed class BoolNodeMapper : IDataPointNodeMapper<BoolNode>
{
    private readonly TagNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => BoolNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public BoolNode Map(LinkedNode node) => new(
        node,
        new TagName(node.GetRequiredPropertyValue<string>(ILogixTagNode.TagNamePropertyName)),
        PollFrequency.FromMilliseconds(
            node.GetRequiredPropertyValue<int>(ILogixTagNode.PollFrequencyPropertyName)));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}
