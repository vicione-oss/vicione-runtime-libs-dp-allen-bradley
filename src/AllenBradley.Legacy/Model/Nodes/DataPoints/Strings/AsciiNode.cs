using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings;

/// <summary>
/// A configured element of an ASCII file, the configuration-time half of <see cref="AsciiDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ElementNumber">The element inside the parent ASCII file.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record AsciiNode(LinkedNode OriginalNode, ElementNumber ElementNumber, PollFrequency PollFrequency)
    : LegacyDataPointNode(OriginalNode, ElementNumber, PollFrequency)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "Ascii";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Ascii;
}
