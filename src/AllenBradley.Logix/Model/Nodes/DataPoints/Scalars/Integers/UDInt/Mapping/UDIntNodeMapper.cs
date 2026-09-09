using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UDInt.Mapping;

/// <summary>Maps a configured <c>UDInt</c> node onto a <see cref="UDIntNode"/>.</summary>
internal sealed class UDIntNodeMapper : IDataPointNodeMapper<UDIntNode>
{
    private readonly TagNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => UDIntNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public UDIntNode Map(LinkedNode node) => new(
        node,
        new TagName(node.GetRequiredPropertyValue<string>(ILogixTagNode.TagNamePropertyName)),
        PollFrequency.FromMilliseconds(
            node.GetRequiredPropertyValue<int>(ILogixTagNode.PollFrequencyPropertyName)));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}
