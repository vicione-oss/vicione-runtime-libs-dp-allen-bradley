using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Binary;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Binary.Bit;

/// <summary>
/// A configured bit of a binary file element, the configuration-time half of <see cref="BitDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ElementNumber">The element inside the parent binary file.</param>
/// <param name="BitNumber">The bit inside the element.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record BitNode(
    LinkedNode OriginalNode,
    ElementNumber ElementNumber,
    BitNumber BitNumber,
    PollFrequency PollFrequency)
    : LegacyDataPointNode(OriginalNode, ElementNumber, PollFrequency)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "Bit";

    /// <summary>The manifest property carrying <see cref="BitNumber"/>.</summary>
    public const string BitNumberPropertyName = nameof(BitNumber);

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Binary;
}
