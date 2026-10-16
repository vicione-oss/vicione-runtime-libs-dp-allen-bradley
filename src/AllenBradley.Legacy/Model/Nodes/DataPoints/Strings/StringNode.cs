using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings;

/// <summary>
/// A configured element of a string file, the configuration-time half of <see cref="StringDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ElementNumber">The element inside the parent string file.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record StringNode(LinkedNode OriginalNode, ElementNumber ElementNumber, PollFrequency PollFrequency)
    : LegacyDataPointNode(OriginalNode, ElementNumber, PollFrequency)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "String";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.String;
}
