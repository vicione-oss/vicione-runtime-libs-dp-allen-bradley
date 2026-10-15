using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.BinaryFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.BinaryFile.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles.BinaryFile.Mapping;

public sealed class BinaryFileNodeMapperTests
{
    private readonly BinaryFileNodeMapper _mapper = new();

    [Fact]
    public void ABinaryFileIsTheFileNumberItWasConfiguredWith()
    {
        // Arrange
        const ushort fileNumber = 3;
        var node = BinaryFileWith(fileNumber);

        // Act
        var binaryFile = _mapper.Map(node);

        // Assert
        binaryFile.FileNumber.Should().Be(new FileNumber(fileNumber));
    }

    [Theory]
    [InlineData((ushort)3, "the default binary file")]
    [InlineData((ushort)255, "the last file an SLC 500 or MicroLogix has")]
    public void ABinaryFileInTheUserFilesIsAccepted(ushort fileNumber, string validBecause)
    {
        // Arrange
        var node = BinaryFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData((ushort)0, "the output image")]
    [InlineData((ushort)1, "the input image")]
    [InlineData((ushort)2, "the status file")]
    public void ABinaryFileOnAReservedFileNumberIsRefused(ushort fileNumber, string refusedBecause)
    {
        // Arrange
        var node = BinaryFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle(refusedBecause)
            .Which.PropertyName.Should().Be(DataFileNode.FileNumberPropertyName);
    }

    private static LinkedNode BinaryFileWith(ushort fileNumber) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = BinaryFileNode.LinkedNodeTypeId,
                Name = BinaryFileNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = new Dictionary<string, Property>
                {
                    [DataFileNode.FileNumberPropertyName] = new() { Value = fileNumber },
                },
            },
        ]).Single();
}
