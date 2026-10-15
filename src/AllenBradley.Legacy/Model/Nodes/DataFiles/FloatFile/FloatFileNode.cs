using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.FloatFile;

/// <summary>
/// A configured float (<c>F</c>) file. It holds the file number, and its children the elements read
/// from it, so the address composes to <c>F8:12</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="FileNumber">The number of the float file.</param>
internal sealed record FloatFileNode(LinkedNode OriginalNode, FileNumber FileNumber)
    : DataFileNode(OriginalNode, FileNumber)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "FloatFile";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Float;
}
