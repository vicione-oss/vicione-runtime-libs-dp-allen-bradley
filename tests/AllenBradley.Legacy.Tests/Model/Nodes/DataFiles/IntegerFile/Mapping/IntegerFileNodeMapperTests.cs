using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles.IntegerFile.Mapping;

public sealed class IntegerFileNodeMapperTests
{
    private readonly IntegerFileNodeMapper _mapper = new();

    [Fact]
    public void AnIntegerFileIsTheFileNumberItWasConfiguredWith()
    {
        // Arrange
        const ushort fileNumber = 10;
        var node = LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = IntegerFileNode.LinkedNodeTypeId,
                Name = IntegerFileNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = new Dictionary<string, Property>
                {
                    [DataFileNode.FileNumberPropertyName] = new() { Value = fileNumber },
                },
            },
        ]).Single();

        // Act
        var integerFile = _mapper.Map(node);

        // Assert
        integerFile.FileNumber.Should().Be(new FileNumber(fileNumber));
    }
}
