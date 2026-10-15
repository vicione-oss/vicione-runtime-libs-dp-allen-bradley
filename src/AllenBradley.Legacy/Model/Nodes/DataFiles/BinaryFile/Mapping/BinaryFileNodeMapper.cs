using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.BinaryFile.Mapping;

/// <summary>Maps a configured <c>BinaryFile</c> node onto a <see cref="BinaryFileNode"/>.</summary>
internal sealed class BinaryFileNodeMapper : DataFileNodeMapper<BinaryFileNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => BinaryFileNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override BinaryFileNode CreateNode(LinkedNode originalNode, FileNumber fileNumber) =>
        new(originalNode, fileNumber);
}
