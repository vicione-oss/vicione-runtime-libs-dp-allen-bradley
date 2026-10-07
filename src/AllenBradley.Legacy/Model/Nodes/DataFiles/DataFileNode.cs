using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;

/// <summary>
/// What every configured data file carries whatever its type: the file number, which its elements take
/// their address from. A concrete file adds only its <c>LinkedNodeTypeId</c> and its type.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="FileNumber">The number of the data file.</param>
internal abstract record DataFileNode(LinkedNode OriginalNode, FileNumber FileNumber) : LegacyContainerNode(OriginalNode)
{
    /// <summary>The manifest property carrying <see cref="FileNumber"/>.</summary>
    public const string FileNumberPropertyName = nameof(FileNumber);

    /// <summary>The type of the file, which is the type of every element in it.</summary>
    public abstract DataFileType FileType { get; }

    /// <summary>A data file holds elements, never another file.</summary>
    internal sealed override bool CanBeAdded(ILegacyContainerNode legacyContainerNode) => false;

    /// <summary>An element of the file's own type: the file's type is every element's type.</summary>
    internal sealed override bool CanBeAdded(ILegacyDataPointNode legacyDataPointNode) =>
        legacyDataPointNode.FileType == FileType;
}
