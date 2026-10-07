using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile.Mapping;

/// <summary>Maps a configured <c>IntegerFile</c> node onto an <see cref="IntegerFileNode"/>.</summary>
internal sealed class IntegerFileNodeMapper : DataFileNodeMapper<IntegerFileNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => IntegerFileNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override IntegerFileNode CreateNode(LinkedNode originalNode, FileNumber fileNumber) =>
        new(originalNode, fileNumber);
}
