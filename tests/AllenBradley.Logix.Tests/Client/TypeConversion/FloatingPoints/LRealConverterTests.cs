using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

public sealed class LRealConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new LRealConverter();

    private static readonly LRealDataPoint Measurement =
        new(TagPath.Parse("PrecisionValue"), DefaultPollFrequency, NoChannels);

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
