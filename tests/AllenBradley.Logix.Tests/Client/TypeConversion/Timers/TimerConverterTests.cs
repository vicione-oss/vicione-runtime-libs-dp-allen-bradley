using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Timers;

public sealed class TimerConverterTests
{
    private static readonly IDataPointConverter Converter = new TimerConverter();

    private static readonly TimerDataPoint Delay = new(TagPath.Parse("Delay"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheFourBytesOfAccDecodeToTheAccumulatedMilliseconds()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0x00, 0x00];

        // Act
        var decoded = Converter.Decode(Delay, buffer);

        // Assert
        decoded.Should().Be(Delay.CreateLogixValue(4711));
    }

    [Fact]
    public void AnAccumulatedTimeEncodesToTheFourBytesOfAcc()
    {
        // Arrange
        var value = Delay.CreateLogixValue(4711);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x67, 0x12, 0x00, 0x00);
    }

    [Fact]
    public void ANegativeAccumulatedTimeIsRefusedNamingTheTimer()
    {
        // Arrange
        var value = Delay.CreateLogixValue(-1);

        // Act
        var encoding = Converter.Invoking(c => c.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>().WithMessage("*Delay*");
    }
}
