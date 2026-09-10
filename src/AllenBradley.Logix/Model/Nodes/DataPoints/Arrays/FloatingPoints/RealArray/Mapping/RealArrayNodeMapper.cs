using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.RealArray.Mapping;

/// <summary>Maps a configured <c>RealArray</c> node onto a <see cref="RealArrayNode"/>.</summary>
internal sealed class RealArrayNodeMapper : IDataPointNodeMapper<RealArrayNode>
{
    private readonly ArrayNodePropertyValidator _validator = new();

    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => RealArrayNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public RealArrayNode Map(LinkedNode node) => new(
        node,
        new TagName(node.GetRequiredPropertyValue<string>(ILogixTagNode.TagNamePropertyName)),
        PollFrequency.FromMilliseconds(
            node.GetRequiredPropertyValue<int>(ILogixTagNode.PollFrequencyPropertyName)),
        new ElementCount(node.GetRequiredPropertyValue<int>(LogixArrayNode.ElementCountPropertyName)));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => _validator.Validate(linkedNode);
}
