using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings.Mapping;

/// <summary>Maps a configured <c>Ascii</c> node onto an <see cref="AsciiNode"/>.</summary>
internal sealed class AsciiNodeMapper : LegacyDataPointNodeMapper<AsciiNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => AsciiNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override AsciiNode CreateNode(
        LinkedNode originalNode, ElementNumber elementNumber, PollFrequency pollFrequency) =>
        new(originalNode, elementNumber, pollFrequency);
}
