using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Counters;

public sealed class CounterConverterTests
{
    private static readonly IDataPointConverter Converter = new CounterConverter();

    private static readonly CounterDataPoint Parts = new(TagPath.Parse("Parts"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheFourBytesOfAccDecodeToTheAccumulatedCount()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0x00, 0x00];

        // Act
        var decoded = Converter.Decode(Parts, buffer);

        // Assert
        decoded.Should().Be(Parts.CreateLogixValue(4711));
    }

    [Fact]
    public void ANegativeAccumulatedCountEncodesToTheFourBytesOfAcc()
    {
        // Arrange
        var value = Parts.CreateLogixValue(-2);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFE, 0xFF, 0xFF, 0xFF);
    }
}
