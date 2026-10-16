using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings.Mapping;

/// <summary>Maps a configured <c>String</c> node onto a <see cref="StringNode"/>.</summary>
internal sealed class StringNodeMapper : LegacyDataPointNodeMapper<StringNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => StringNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override StringNode CreateNode(
        LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency) =>
        new(originalNode, elementNumber, pollFrequency);
}
