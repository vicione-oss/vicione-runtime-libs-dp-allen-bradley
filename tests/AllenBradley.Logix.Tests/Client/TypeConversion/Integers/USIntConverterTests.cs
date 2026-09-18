using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class USIntConverterTests
{
    private static readonly IDataPointConverter Converter = new USIntConverter();

    private static readonly USIntDataPoint Level = new(TagPath.Parse("usintValue1"), DefaultPollFrequency, NoChannels);

    [Theory]
    [InlineData(0x00, (byte)0)]
    [InlineData(0x2A, (byte)42)]
    [InlineData(0x7F, (byte)127)]
    [InlineData(0x80, (byte)128)]
    [InlineData(0xFF, byte.MaxValue)]
    public void AStoredBitPatternDecodesToTheUSIntTheControllerMeansByIt(byte storedBits, byte expectedValue)
    {
        // Arrange
        byte[] buffer = [storedBits];

        // Act
        var decoded = Converter.Decode(Level, buffer);

        // Assert
        decoded.Should().Be(Level.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADecodeReadsOnlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x2A, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Level, buffer);

        // Assert
        decoded.Should().Be(Level.CreateLogixValue(42));
    }

    [Fact]
    public void AUSIntEncodesToExactlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        var value = Level.CreateLogixValue(42);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x2A);
    }

    [Fact]
    public void AUSIntAboveTheSignedRangeEncodesAsItStands()
    {
        // Arrange
        var value = Level.CreateLogixValue(byte.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF);
    }
}
