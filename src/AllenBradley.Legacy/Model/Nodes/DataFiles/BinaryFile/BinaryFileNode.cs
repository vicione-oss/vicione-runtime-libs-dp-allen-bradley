using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.BinaryFile;

/// <summary>
/// A configured binary (<c>B</c>) file. It holds the file number, and its children the bits read from it,
/// so the address composes to <c>B3:2/5</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="FileNumber">The number of the binary file.</param>
internal sealed record BinaryFileNode(LinkedNode OriginalNode, FileNumber FileNumber)
    : DataFileNode(OriginalNode, FileNumber)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "BinaryFile";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Binary;
}
