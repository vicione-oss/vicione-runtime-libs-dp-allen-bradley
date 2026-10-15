using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.LongFile.Mapping;

/// <summary>Maps a configured <c>LongFile</c> node onto a <see cref="LongFileNode"/>.</summary>
internal sealed class LongFileNodeMapper : DataFileNodeMapper<LongFileNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => LongFileNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override LongFileNode CreateNode(LinkedNode originalNode, FileNumber fileNumber) =>
        new(originalNode, fileNumber);
}
