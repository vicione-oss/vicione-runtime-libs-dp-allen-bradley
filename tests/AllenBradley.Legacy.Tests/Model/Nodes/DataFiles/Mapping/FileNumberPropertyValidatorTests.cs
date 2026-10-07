using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles.Mapping;

public sealed class FileNumberPropertyValidatorTests
{
    private readonly FileNumberPropertyValidator _validator = new();

    [Theory]
    [InlineData((ushort)3, "the first file a program types itself")]
    [InlineData((ushort)7, "the default integer file")]
    [InlineData((ushort)255, "the last file an SLC 500 or MicroLogix has")]
    public void AFileNumberFromThreeToTwoHundredFiftyFiveIsAccepted(ushort fileNumber, string validBecause)
    {
        // Arrange
        var node = IntegerFileWith(fileNumber);

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData((ushort)0, "the output image")]
    [InlineData((ushort)1, "the input image")]
    [InlineData((ushort)2, "the status file")]
    [InlineData((ushort)256, "past the last file")]
    public void AFileNumberOutsideTheUserFilesIsRefusedOnTheFileNumberProperty(ushort fileNumber, string refusedBecause)
    {
        // Arrange
        var node = IntegerFileWith(fileNumber);

        // Act
        var validation = _validator.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle(refusedBecause)
            .Which.PropertyName.Should().Be(DataFileNode.FileNumberPropertyName);
    }

    private static LinkedNode IntegerFileWith(ushort fileNumber) =>
        LinkedNodeFactory.Create(
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
}
