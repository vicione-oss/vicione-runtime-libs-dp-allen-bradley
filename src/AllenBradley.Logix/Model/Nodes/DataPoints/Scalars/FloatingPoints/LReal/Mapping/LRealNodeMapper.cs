using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal.Mapping;

/// <summary>Maps a configured <c>LReal</c> node onto an <see cref="LRealNode"/>.</summary>
internal sealed class LRealNodeMapper : IDataPointNodeMapper<LRealNode>
{
    private readonly ScalarNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => LRealNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public LRealNode Map(LinkedNode node) => new(
        node,
        new TagName(node.GetRequiredPropertyValue<string>(ILogixScalarNode.TagNamePropertyName)),
        PollFrequency.FromMilliseconds(
            node.GetRequiredPropertyValue<int>(ILogixScalarNode.PollFrequencyPropertyName)));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}
