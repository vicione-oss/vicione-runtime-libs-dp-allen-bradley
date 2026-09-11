using FluentValidation;
using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Base;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

/// <summary>
/// What every tag node mapper does whatever the node's type: validate the configured node and read
/// the tag name and poll frequency off it. A concrete mapper adds only the <c>LinkedNodeTypeId</c> it
/// claims and the constructor call, plus whatever its own type needs on top.
/// </summary>
/// <typeparam name="TNode">The node the mapper produces.</typeparam>
/// <param name="validator">The rules the configured node must satisfy before it is mapped.</param>
internal abstract class LogixTagNodeMapper<TNode>(AbstractValidator<LinkedNode> validator)
    : IDataPointNodeMapper<TNode>
    where TNode : LogixTagNode
{
    protected LogixTagNodeMapper() : this(new TagNodePropertyValidator())
    {
    }

    /// <inheritdoc />
    public abstract string TargetLinkedNodeTypeId { get; }

    /// <inheritdoc />
    public TNode Map(LinkedNode node) =>
        CreateNode(node, TagNameExtractor.GetTagName(node), PollFrequencyExtractor.GetPollFrequency(node));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => validator.Validate(linkedNode);

    protected abstract TNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency);
}
