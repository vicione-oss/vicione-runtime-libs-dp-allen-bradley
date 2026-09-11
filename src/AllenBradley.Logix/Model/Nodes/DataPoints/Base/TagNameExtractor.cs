using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Base;

/// <summary>Reads the <see cref="ILogixTagNode.TagNamePropertyName"/> property off a configured node.</summary>
internal static class TagNameExtractor
{
    public static TagName GetTagName(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<string>(ILogixTagNode.TagNamePropertyName));
}
