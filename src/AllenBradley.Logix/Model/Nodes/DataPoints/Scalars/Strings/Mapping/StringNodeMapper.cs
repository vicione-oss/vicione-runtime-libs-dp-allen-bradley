using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings.Mapping;

/// <summary>Maps a configured <c>String</c> node onto a <see cref="StringNode"/>.</summary>
internal sealed class StringNodeMapper() : LogixTagNodeMapper<StringNode>(new StringNodePropertyValidator())
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => StringNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override StringNode CreateNode(LinkedNode originalNode, TagName tagName, PollFrequency pollFrequency) =>
        new(originalNode, tagName, pollFrequency, GetMaxLength(originalNode));

    private static StringMaxLength GetMaxLength(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<int>(StringNode.MaxLengthPropertyName));
}
