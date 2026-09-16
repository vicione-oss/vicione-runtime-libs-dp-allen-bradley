using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;

internal sealed record ArrayContainerNode(
    LinkedNode OriginalNode,
    TagName TagName,
    AllenBradleyDataType ArrayDataType) : LogixContainerNode(OriginalNode)
{
    internal const string LinkedNodeTypeIdSuffix = "ArrayContainer";

    internal static bool IsArrayContainer(LinkedNode node) =>
        node.DesignId.EndsWith(LinkedNodeTypeIdSuffix, StringComparison.Ordinal);

    internal override bool CanBeAdded(ILogixContainerNode logixContainerNode) => false;

    internal override bool CanBeAdded(ILogixDataPointNode dataPointNode) =>
        dataPointNode.DataType == ArrayDataType;
}
