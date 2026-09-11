using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;

/// <summary>
/// What every array node mapper does on top of <see cref="LogixTagNodeMapper{TNode}"/>: read the
/// declared element count off the configured node.
/// </summary>
/// <typeparam name="TNode">The array node the mapper produces.</typeparam>
internal abstract class LogixArrayNodeMapper<TNode>() : LogixTagNodeMapper<TNode>(new ArrayNodePropertyValidator())
    where TNode : LogixArrayNode
{
    protected sealed override TNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        CreateNode(originalNode, tagName, pollFrequency, GetElementCount(originalNode));

    protected abstract TNode CreateNode(
        LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency, ElementCount elementCount);

    private static ElementCount GetElementCount(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<int>(LogixArrayNode.ElementCountPropertyName));
}
