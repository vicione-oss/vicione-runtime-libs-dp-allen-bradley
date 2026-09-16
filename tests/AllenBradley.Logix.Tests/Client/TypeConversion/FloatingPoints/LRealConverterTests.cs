using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

public sealed class LRealConverterTests
{
    private static readonly IDataPointConverter Converter = new LRealConverter();

    private static readonly LRealDataPoint Measurement =
        new(new TagAddress("PrecisionValue"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagALReal()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Lreal);
    }

    [Fact]
    public void EightStoredBytesDecodeToTheIeee754DoubleTheyHoldLeastSignificantFirst()
    {
        // Arrange
        byte[] buffer = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F];

        // Act
        var decoded = Converter.Decode(Measurement, buffer);

        // Assert
        decoded.Should().Be(Measurement.CreateLogixValue(1.0d));
    }

    [Theory]
    [InlineData(0x3FF0000000000000, 1.0d)]
    [InlineData(0xBFF0000000000000, -1.0d)]
    [InlineData(0x0000000000000000, 0.0d)]
    [InlineData(0x400921FB54442D18, Math.PI)]
    public void AStoredBitPatternDecodesToTheLRealTheControllerMeansByIt(ulong storedBits, double expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Measurement, buffer);

        // Assert
        decoded.Should().Be(Measurement.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ALRealEncodesToExactlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        var value = Measurement.CreateLogixValue(Math.PI);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x18, 0x2D, 0x44, 0x54, 0xFB, 0x21, 0x09, 0x40);
    }
}
