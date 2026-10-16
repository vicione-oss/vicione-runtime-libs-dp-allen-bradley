using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.AsciiFile.Mapping;

/// <summary>Maps a configured <c>AsciiFile</c> node onto an <see cref="AsciiFileNode"/>.</summary>
internal sealed class AsciiFileNodeMapper : DataFileNodeMapper<AsciiFileNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => AsciiFileNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override AsciiFileNode CreateNode(LinkedNode originalNode, FileNumber fileNumber) =>
        new(originalNode, fileNumber);
}
