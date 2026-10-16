using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.StringFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.StringFile.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles.StringFile.Mapping;

public sealed class StringFileNodeMapperTests
{
    private readonly StringFileNodeMapper _mapper = new();

    [Fact]
    public void AStringFileIsTheFileNumberItWasConfiguredWith()
    {
        // Arrange
        const ushort fileNumber = 9;
        var node = StringFileWith(fileNumber);

        // Act
        var stringFile = _mapper.Map(node);

        // Assert
        stringFile.FileNumber.Should().Be(new FileNumber(fileNumber));
    }

    [Theory]
    [InlineData((ushort)3, "the first user file")]
    [InlineData((ushort)255, "the last file an SLC 500 or MicroLogix has")]
    public void AStringFileInTheUserFilesIsAccepted(ushort fileNumber, string validBecause)
    {
        // Arrange
        var node = StringFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData((ushort)0, "the output image")]
    [InlineData((ushort)1, "the input image")]
    [InlineData((ushort)2, "the status file")]
    public void AStringFileOnAReservedFileNumberIsRefused(ushort fileNumber, string refusedBecause)
    {
        // Arrange
        var node = StringFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle(refusedBecause)
            .Which.PropertyName.Should().Be(DataFileNode.FileNumberPropertyName);
    }

    private static LinkedNode StringFileWith(ushort fileNumber) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = StringFileNode.LinkedNodeTypeId,
                Name = StringFileNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = new Dictionary<string, Property>
                {
                    [DataFileNode.FileNumberPropertyName] = new() { Value = fileNumber },
                },
            },
        ]).Single();
}
