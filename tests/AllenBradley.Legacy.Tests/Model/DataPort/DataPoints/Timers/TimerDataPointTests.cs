using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.DataPort.DataPoints.Timers;

public sealed class TimerDataPointTests
{
    [Theory]
    [InlineData((short)0, "a reset timer")]
    [InlineData(short.MaxValue, "the largest value the word holds")]
    public void AnAccumulatedValueThatIsNotNegativeIsInRange(short ticks, string inRangeBecause)
    {
        // Arrange
        var value = T4Element(0).CreateLegacyValue(ticks);

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeTrue(inRangeBecause);
    }

    [Theory]
    [InlineData((short)-1)]
    [InlineData(short.MinValue)]
    public void ANegativeAccumulatedValueIsOutOfRange(short ticks)
    {
        // Arrange
        var value = T4Element(0).CreateLegacyValue(ticks);

        // Act
        var isInValueRange = value.IsInValueRange();

        // Assert
        isInValueRange.Should().BeFalse();
    }

    [Fact]
    public void ATimerDataPointIsIdentifiedByTheAddressOfItsAccumulatedValue()
    {
        // Arrange
        var dataPoint = T4Element(0);

        // Act
        var identifier = dataPoint.Identifier;

        // Assert
        identifier.Value.Should().Be("T4:0.ACC");
    }
}
