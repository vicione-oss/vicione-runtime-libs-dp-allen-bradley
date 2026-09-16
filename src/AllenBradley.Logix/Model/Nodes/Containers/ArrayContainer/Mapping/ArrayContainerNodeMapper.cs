using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer.Mapping;

internal class ArrayContainerNodeMapper(string targetLinkedNodeTypeId, AllenBradleyDataType elementDataType)
    : IBranchConfigurationNodeMapper<ArrayContainerNode>
{
    private static readonly ArrayContainerNodePropertyValidator Validator = new();

    public string TargetLinkedNodeTypeId { get; } = targetLinkedNodeTypeId;

    private AllenBradleyDataType ElementDataType { get; } = elementDataType;

    public ArrayContainerNode Map(LinkedNode node)
    {
        var tagName = TagNamePropertyExtractor.GetTagName(node);
        return new ArrayContainerNode(node, tagName, ElementDataType);
    }

    public ValidationResult Validate(LinkedNode linkedNode) => Validator.Validate(linkedNode);
}
