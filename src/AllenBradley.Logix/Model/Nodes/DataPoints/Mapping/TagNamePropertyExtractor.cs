using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

internal static class TagNamePropertyExtractor
{
    internal static TagName GetTagName(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<string>(ILogixDataPointNode.TagNamePropertyName));
}
