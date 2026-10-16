using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.AsciiFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.AsciiFile.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles.AsciiFile.Mapping;

public sealed class AsciiFileNodeMapperTests
{
    private readonly AsciiFileNodeMapper _mapper = new();

    [Fact]
    public void AnAsciiFileIsTheFileNumberItWasConfiguredWith()
    {
        // Arrange
        const ushort fileNumber = 9;
        var node = AsciiFileWith(fileNumber);

        // Act
        var asciiFile = _mapper.Map(node);

        // Assert
        asciiFile.FileNumber.Should().Be(new FileNumber(fileNumber));
    }

    [Theory]
    [InlineData((ushort)3, "the first user file")]
    [InlineData((ushort)255, "the last file an SLC 500 or MicroLogix has")]
    public void AnAsciiFileInTheUserFilesIsAccepted(ushort fileNumber, string validBecause)
    {
        // Arrange
        var node = AsciiFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData((ushort)0, "the output image")]
    [InlineData((ushort)1, "the input image")]
    [InlineData((ushort)2, "the status file")]
    public void AnAsciiFileOnAReservedFileNumberIsRefused(ushort fileNumber, string refusedBecause)
    {
        // Arrange
        var node = AsciiFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle(refusedBecause)
            .Which.PropertyName.Should().Be(DataFileNode.FileNumberPropertyName);
    }

    private static LinkedNode AsciiFileWith(ushort fileNumber) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = AsciiFileNode.LinkedNodeTypeId,
                Name = AsciiFileNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = new Dictionary<string, Property>
                {
                    [DataFileNode.FileNumberPropertyName] = new() { Value = fileNumber },
                },
            },
        ]).Single();
}
