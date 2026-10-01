using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer.Mapping;

internal sealed class ArrayContainerNodeMapper : IBranchConfigurationNodeMapper<ArrayContainerNode>
{
    private static readonly ArrayContainerNodePropertyValidator Validator = new();

    private ArrayContainerNodeMapper(string targetLinkedNodeTypeId, AllenBradleyDataType elementDataType)
    {
        TargetLinkedNodeTypeId = targetLinkedNodeTypeId;
        ElementDataType = elementDataType;
    }

    public string TargetLinkedNodeTypeId { get; }

    private AllenBradleyDataType ElementDataType { get; }

    /// <summary>One mapper per element type an array container can hold.</summary>
    internal static ArrayContainerNodeMapper[] All() =>
        [SInt(), Int(), DInt(), LInt(), USInt(), UInt(), UDInt(), ULInt(), Real(), LReal(), Timer()];

    internal static ArrayContainerNodeMapper SInt() =>
        new(ArrayContainerNode.SIntLinkedNodeTypeId, AllenBradleyDataType.Sint);

    internal static ArrayContainerNodeMapper Int() =>
        new(ArrayContainerNode.IntLinkedNodeTypeId, AllenBradleyDataType.Int);

    internal static ArrayContainerNodeMapper DInt() =>
        new(ArrayContainerNode.DIntLinkedNodeTypeId, AllenBradleyDataType.Dint);

    internal static ArrayContainerNodeMapper LInt() =>
        new(ArrayContainerNode.LIntLinkedNodeTypeId, AllenBradleyDataType.Lint);

    internal static ArrayContainerNodeMapper USInt() =>
        new(ArrayContainerNode.USIntLinkedNodeTypeId, AllenBradleyDataType.Usint);

    internal static ArrayContainerNodeMapper UInt() =>
        new(ArrayContainerNode.UIntLinkedNodeTypeId, AllenBradleyDataType.Uint);

    internal static ArrayContainerNodeMapper UDInt() =>
        new(ArrayContainerNode.UDIntLinkedNodeTypeId, AllenBradleyDataType.Udint);

    internal static ArrayContainerNodeMapper ULInt() =>
        new(ArrayContainerNode.ULIntLinkedNodeTypeId, AllenBradleyDataType.Ulint);

    internal static ArrayContainerNodeMapper Real() =>
        new(ArrayContainerNode.RealLinkedNodeTypeId, AllenBradleyDataType.Real);

    internal static ArrayContainerNodeMapper LReal() =>
        new(ArrayContainerNode.LRealLinkedNodeTypeId, AllenBradleyDataType.Lreal);

    internal static ArrayContainerNodeMapper Timer() =>
        new(ArrayContainerNode.TimerLinkedNodeTypeId, AllenBradleyDataType.Timer);

    public ArrayContainerNode Map(LinkedNode node)
    {
        var tagName = TagNamePropertyExtractor.GetTagName(node);
        return new ArrayContainerNode(node, tagName, ElementDataType);
    }

    public ValidationResult Validate(LinkedNode linkedNode) => Validator.Validate(linkedNode);
}
