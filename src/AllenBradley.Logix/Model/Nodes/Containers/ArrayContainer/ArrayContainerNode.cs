using System.Collections.Frozen;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;

internal sealed record ArrayContainerNode(
    LinkedNode OriginalNode,
    TagName TagName,
    AllenBradleyDataType ArrayDataType) : LogixContainerNode(OriginalNode)
{
    internal const string SIntLinkedNodeTypeId = "SIntArrayContainer";

    internal const string IntLinkedNodeTypeId = "IntArrayContainer";

    internal const string DIntLinkedNodeTypeId = "DIntArrayContainer";

    internal const string LIntLinkedNodeTypeId = "LIntArrayContainer";

    internal const string USIntLinkedNodeTypeId = "USIntArrayContainer";

    internal const string UIntLinkedNodeTypeId = "UIntArrayContainer";

    internal const string UDIntLinkedNodeTypeId = "UDIntArrayContainer";

    internal const string ULIntLinkedNodeTypeId = "ULIntArrayContainer";

    internal const string RealLinkedNodeTypeId = "RealArrayContainer";

    internal const string LRealLinkedNodeTypeId = "LRealArrayContainer";

    internal const string TimerLinkedNodeTypeId = "TimerArrayContainer";

    private static readonly FrozenSet<string> LinkedNodeTypeIds = FrozenSet.Create(
        StringComparer.Ordinal,
        SIntLinkedNodeTypeId, IntLinkedNodeTypeId, DIntLinkedNodeTypeId, LIntLinkedNodeTypeId,
        USIntLinkedNodeTypeId, UIntLinkedNodeTypeId, UDIntLinkedNodeTypeId, ULIntLinkedNodeTypeId,
        RealLinkedNodeTypeId, LRealLinkedNodeTypeId, TimerLinkedNodeTypeId);

    internal static bool IsArrayContainer(LinkedNode node) => LinkedNodeTypeIds.Contains(node.DesignId);

    internal override bool CanBeAdded(ILogixContainerNode logixContainerNode) => false;

    internal override bool CanBeAdded(ILogixDataPointNode dataPointNode) =>
        dataPointNode.DataType == ArrayDataType;
}
