using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

public sealed class RealConverterTests
{
    private static readonly IDataPointConverter Converter = new RealConverter();

    private static readonly RealDataPoint Measurement =
        new(TagPath.Parse("realValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void FourStoredBytesDecodeToTheIeee754SingleTheyHoldLeastSignificantFirst()
    {
        // Arrange
        byte[] buffer = [0x00, 0x00, 0x80, 0x3F];

        // Act
        var decoded = Converter.Decode(Measurement, buffer);

        // Assert
        decoded.Should().Be(Measurement.CreateLogixValue(1.0f));
    }

    [Theory]
    [InlineData(0x3F800000u, 1.0f)]
    [InlineData(0xBF800000u, -1.0f)]
    [InlineData(0x00000000u, 0.0f)]
    [InlineData(0x40490FDBu, MathF.PI)]
    public void AStoredBitPatternDecodesToTheRealTheControllerMeansByIt(uint storedBits, float expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Measurement, buffer);

        // Assert
        decoded.Should().Be(Measurement.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ARealEncodesToExactlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        var value = Measurement.CreateLogixValue(MathF.PI);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xDB, 0x0F, 0x49, 0x40);
    }
}
