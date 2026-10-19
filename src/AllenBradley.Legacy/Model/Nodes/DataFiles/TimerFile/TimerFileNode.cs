using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.TimerFile;

/// <summary>
/// A configured timer (<c>T</c>) file. It holds the file number, and its children the elements read
/// from it, so the address composes to <c>T4:0.ACC</c>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="FileNumber">The number of the timer file.</param>
internal sealed record TimerFileNode(LinkedNode OriginalNode, FileNumber FileNumber)
    : DataFileNode(OriginalNode, FileNumber)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "TimerFile";

    /// <inheritdoc />
    public override DataFileType FileType => DataFileType.Timer;
}
