using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.TimerFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.TimerFile.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.DataFiles.TimerFile.Mapping;

public sealed class TimerFileNodeMapperTests
{
    private readonly TimerFileNodeMapper _mapper = new();

    [Fact]
    public void ATimerFileIsTheFileNumberItWasConfiguredWith()
    {
        // Arrange
        const ushort fileNumber = 4;
        var node = TimerFileWith(fileNumber);

        // Act
        var timerFile = _mapper.Map(node);

        // Assert
        timerFile.FileNumber.Should().Be(new FileNumber(fileNumber));
    }

    [Theory]
    [InlineData((ushort)3, "the first file a program types itself")]
    [InlineData((ushort)4, "the default timer file")]
    [InlineData((ushort)255, "the last file an SLC 500 or MicroLogix has")]
    public void ATimerFileFromThreeToTwoHundredFiftyFiveIsAccepted(ushort fileNumber, string validBecause)
    {
        // Arrange
        var node = TimerFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.IsValid.Should().BeTrue(validBecause);
    }

    [Theory]
    [InlineData((ushort)0, "the output image")]
    [InlineData((ushort)1, "the input image")]
    [InlineData((ushort)2, "the status file")]
    public void ATimerFileInTheOutputInputOrStatusFileIsRefused(ushort fileNumber, string refusedBecause)
    {
        // Arrange
        var node = TimerFileWith(fileNumber);

        // Act
        var validation = _mapper.Validate(node);

        // Assert
        validation.Errors.Should().ContainSingle(refusedBecause)
            .Which.PropertyName.Should().Be(DataFileNode.FileNumberPropertyName);
    }

    private static LinkedNode TimerFileWith(ushort fileNumber) =>
        LinkedNodeFactory.Create(
        [
            new Node
            {
                DesignId = TimerFileNode.LinkedNodeTypeId,
                Name = TimerFileNode.LinkedNodeTypeId,
                Id = Guid.NewGuid(),
                Properties = new Dictionary<string, Property>
                {
                    [DataFileNode.FileNumberPropertyName] = new() { Value = fileNumber },
                },
            },
        ]).Single();
}
