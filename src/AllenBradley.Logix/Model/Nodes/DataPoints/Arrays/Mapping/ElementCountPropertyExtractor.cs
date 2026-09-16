using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;

internal static class ElementCountPropertyExtractor
{
    internal static ElementCount GetElementCount(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<uint>(LogixArrayDataPointNode.ElementCountPropertyName));
}
