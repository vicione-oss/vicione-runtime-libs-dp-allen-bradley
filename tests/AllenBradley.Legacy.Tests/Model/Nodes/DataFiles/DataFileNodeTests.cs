using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles;

public sealed class DataFileNodeTests
{
    private readonly IntegerFileNode _integerFile =
        new(LinkedNodeOf(IntegerFileNode.LinkedNodeTypeId), new FileNumber(7));

    [Fact]
    public void AnIntegerFileHoldsIntegerElements()
    {
        // Arrange
        var element = new IntegerNode(
            LinkedNodeOf(IntegerNode.LinkedNodeTypeId), new ElementNumber(0), PollFrequency.FromMilliseconds(500));

        // Act
        var canBeAdded = _integerFile.CanBeAdded(element);

        // Assert
        canBeAdded.Should().BeTrue();
    }

    [Fact]
    public void ADataFileHoldsNoOtherDataFile()
    {
        // Arrange
        var nestedFile = new IntegerFileNode(LinkedNodeOf(IntegerFileNode.LinkedNodeTypeId), new FileNumber(10));

        // Act
        var canBeAdded = _integerFile.CanBeAdded(nestedFile);

        // Assert
        canBeAdded.Should().BeFalse();
    }

    private static LinkedNode LinkedNodeOf(string designId) =>
        LinkedNodeFactory.Create([new Node { DesignId = designId, Name = designId, Id = Guid.NewGuid() }]).Single();
}
