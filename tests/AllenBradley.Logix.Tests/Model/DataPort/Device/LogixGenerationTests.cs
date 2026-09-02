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
    public void Generations_AreOrderedOldestFirst()
    {
        (LogixGeneration.Logix5X70 < LogixGeneration.Logix5X80).Should().BeTrue();
    }

    /// <remarks>
    /// The values are the numbers in the names, so a 5X90 slots in at 90 and nothing before it moves.
    /// </remarks>
    [Theory]
    [InlineData(LogixGeneration.Logix5X70, 70)]
    [InlineData(LogixGeneration.Logix5X80, 80)]
    public void Generation_IsNumberedAfterTheControllerLine(LogixGeneration generation, int expected)
    {
        ((int)generation).Should().Be(expected);
    }
}
