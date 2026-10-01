using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Timers;

public sealed class TimerDataPointTests
{
    private static readonly TimerDataPoint Delay = new(TagPath.Parse("Delay"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Delay.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("TIMER"));
    }

    [Fact]
    public void ItIsAddressedAsTheTimerWithoutItsAccMember()
    {
        // Arrange

        // Act
        var tagAddress = Delay.TagAddress;

        // Assert
        tagAddress.Should().Be(new TagAddress("Delay"));
    }

    [Theory]
    [InlineData("Delay", "Delay.ACC")]
    [InlineData("Program:MainProgram.Delay", "Program:MainProgram.Delay.ACC")]
    [InlineData("Machine.StartDelay", "Machine.StartDelay.ACC")]
    [InlineData("Delays[3]", "Delays[3].ACC")]
    [InlineData("Program:MainProgram.Machine.Delays[3]", "Program:MainProgram.Machine.Delays[3].ACC")]
    public void ItsHandleReachesTheAccMemberOfTheTimer(string timerAddress, string expectedAddress)
    {
        // Arrange
        var timer = new TimerDataPoint(TagPath.Parse(timerAddress), DefaultPollFrequency, NoChannels);

        // Act
        var handleAddress = timer.HandleAddress;

        // Assert
        handleAddress.Should().Be(new TagAddress(expectedAddress));
    }

    [Fact]
    public void AnIntegerEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Delay.ConvertValue(4711);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(4711);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void AnAccumulatedTimeOfZeroOrMoreIsInRange(int milliseconds)
    {
        // Arrange

        // Act
        var value = Delay.CreateTypedValue(milliseconds);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ANegativeAccumulatedTimeIsOutOfRange(int milliseconds)
    {
        // Arrange

        // Act
        var value = Delay.CreateTypedValue(milliseconds);

        // Assert
        value.IsInValueRange().Should().BeFalse();
    }
}
