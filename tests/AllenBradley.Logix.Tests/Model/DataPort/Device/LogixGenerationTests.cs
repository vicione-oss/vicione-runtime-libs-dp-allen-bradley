using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.Device;

/// <summary>
/// A member inserted or renumbered in the wrong place would silently change what every minimum-generation
/// comparison in the dataport means.
/// </summary>
public sealed class LogixGenerationTests
{
    [Fact]
    public void GenerationsAreOrderedOldestFirst()
    {
        // Arrange

        // Act
        var olderFirst = LogixGeneration.Logix5X70 < LogixGeneration.Logix5X80;

        // Assert
        olderFirst.Should().BeTrue();
    }

    [Theory]
    [InlineData(LogixGeneration.Logix5X70, 70)]
    [InlineData(LogixGeneration.Logix5X80, 80)]
    public void AGenerationIsNumberedAfterTheControllerLine(LogixGeneration generation, int expectedNumber)
    {
        // Arrange

        // Act
        var number = (int)generation;

        // Assert
        number.Should().Be(expectedNumber);
    }
}
