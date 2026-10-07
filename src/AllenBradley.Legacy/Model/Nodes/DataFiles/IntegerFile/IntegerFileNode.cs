using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile;

/// <summary>
/// A configured integer (<c>N</c>) file. It holds the file number, and its children the elements read
/// from it, so the address composes to <c>N7:12</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="FileNumber">The number of the integer file.</param>
internal sealed record IntegerFileNode(LinkedNode OriginalNode, FileNumber FileNumber)
    : DataFileNode(OriginalNode, FileNumber)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "IntegerFile";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Integer;
}
