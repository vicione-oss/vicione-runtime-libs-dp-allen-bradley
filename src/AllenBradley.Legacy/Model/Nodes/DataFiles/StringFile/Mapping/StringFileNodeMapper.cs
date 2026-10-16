using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.StringFile.Mapping;

/// <summary>Maps a configured <c>StringFile</c> node onto a <see cref="StringFileNode"/>.</summary>
internal sealed class StringFileNodeMapper : DataFileNodeMapper<StringFileNode>
{
    /// <inheritdoc />
    public override string TargetLinkedNodeTypeId => StringFileNode.LinkedNodeTypeId;

    /// <inheritdoc />
    protected override StringFileNode CreateNode(LinkedNode originalNode, FileNumber fileNumber) =>
        new(originalNode, fileNumber);
}
