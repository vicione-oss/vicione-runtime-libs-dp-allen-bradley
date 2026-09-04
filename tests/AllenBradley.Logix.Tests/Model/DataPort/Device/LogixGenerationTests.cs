using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.Device;

/// <summary>
/// The order of the generations, which is what a type node's minimum generation is compared against. It
/// is a property of the enum rather than of any one caller, and a member inserted or renumbered in the
/// wrong place would change what every one of those comparisons means without changing a line of theirs.
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
