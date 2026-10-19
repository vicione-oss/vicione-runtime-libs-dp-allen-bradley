using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.TimerFile.Mapping;

/// <summary>Maps a configured <c>TimerFile</c> node onto a <see cref="TimerFileNode"/>.</summary>
internal sealed class TimerFileNodeMapper : DataFileNodeMapper<TimerFileNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => TimerFileNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override TimerFileNode CreateNode(LinkedNode originalNode, FileNumber fileNumber) =>
        new(originalNode, fileNumber);
}
