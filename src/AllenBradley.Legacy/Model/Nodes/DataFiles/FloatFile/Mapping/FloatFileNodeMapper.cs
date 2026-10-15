using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.FloatFile.Mapping;

/// <summary>Maps a configured <c>FloatFile</c> node onto a <see cref="FloatFileNode"/>.</summary>
internal sealed class FloatFileNodeMapper : DataFileNodeMapper<FloatFileNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => FloatFileNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override FloatFileNode CreateNode(LinkedNode originalNode, FileNumber fileNumber) =>
        new(originalNode, fileNumber);
}
