using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.AsciiFile;

/// <summary>
/// A configured ASCII (<c>A</c>) file. It holds the file number, and its children the elements read
/// from it, so the address composes to <c>A9:0</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="FileNumber">The number of the ASCII file.</param>
internal sealed record AsciiFileNode(LinkedNode OriginalNode, FileNumber FileNumber)
    : DataFileNode(OriginalNode, FileNumber)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "AsciiFile";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Ascii;
}
