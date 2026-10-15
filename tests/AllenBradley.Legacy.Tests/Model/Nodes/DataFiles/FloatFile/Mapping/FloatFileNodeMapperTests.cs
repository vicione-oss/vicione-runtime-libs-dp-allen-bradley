using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.FloatFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.FloatFile.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles.FloatFile.Mapping;

public sealed class FloatFileNodeMapperTests
{
    private readonly FloatFileNodeMapper _mapper = new();

    [Fact]
    public void AFloatFileIsTheFileNumberItWasConfiguredWith()
    {
        // Arrange
        const ushort fileNumber = 8;
        var node = FloatFileWith(fileNumber);

        // Act
        var floatFile = _mapper.Map(node);

        // Assert
        floatFile.FileNumber.Should().Be(new FileNumber(fileNumber));
    }

    [Theory]
    [InlineData((ushort)3, "the first user file")]
    [InlineData((ushort)255, "the last file an SLC 500 or MicroLogix has")]
    public void AFloatFileInTheUserFilesIsAccepted(ushort fileNumber, string validBecause)
    {
        // Arrange
        var node = FloatFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData((ushort)0, "the output image")]
    [InlineData((ushort)1, "the input image")]
    [InlineData((ushort)2, "the status file")]
    public void AFloatFileOnAReservedFileNumberIsRefused(ushort fileNumber, string refusedBecause)
    {
        // Arrange
        var node = FloatFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle(refusedBecause)
            .Which.PropertyName.Should().Be(DataFileNode.FileNumberPropertyName);
    }

    private static LinkedNode FloatFileWith(ushort fileNumber) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = FloatFileNode.LinkedNodeTypeId,
                Name = FloatFileNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = new Dictionary<string, Property>
                {
                    [DataFileNode.FileNumberPropertyName] = new() { Value = fileNumber },
                },
            },
        ]).Single();
}
