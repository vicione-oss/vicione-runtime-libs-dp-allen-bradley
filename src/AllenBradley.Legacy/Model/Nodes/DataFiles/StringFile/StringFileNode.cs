using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.StringFile;

/// <summary>
/// A configured string (<c>ST</c>) file. It holds the file number, and its children the elements read
/// from it, so the address composes to <c>ST9:1</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="FileNumber">The number of the string file.</param>
internal sealed record StringFileNode(LinkedNode OriginalNode, FileNumber FileNumber)
    : DataFileNode(OriginalNode, FileNumber)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "StringFile";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.String;
}
