using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class UDIntConverterTests
{
    private static readonly IDataPointConverter Converter = new UDIntConverter();

    private static readonly UDIntDataPoint Runtime = new(TagPath.Parse("udintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void FourStoredBytesDecodeToTheUnsignedIntegerTheyHoldLeastSignificantFirst()
    {
        // Arrange
        // Every byte differs, so a swapped or rotated read cannot land on the same value.
        byte[] buffer = [0x78, 0x56, 0x34, 0x12];

        // Act
        var decoded = Converter.Decode(Runtime, buffer);

        // Assert
        decoded.Should().Be(Runtime.CreateLogixValue(0x12345678u));
    }

    [Theory]
    [InlineData(0x00000000u, 0u)]
    [InlineData(0x0001E240u, 123456u)]
    [InlineData(0x7FFFFFFFu, 2147483647u)]
    [InlineData(0x80000000u, 2147483648u)]
    [InlineData(0xFFFFFFFFu, uint.MaxValue)]
    public void AStoredBitPatternDecodesToTheUDIntTheControllerMeansByIt(uint storedBits, uint expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Runtime, buffer);

        // Assert
        decoded.Should().Be(Runtime.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADecodeReadsOnlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x40, 0xE2, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Runtime, buffer);

        // Assert
        decoded.Should().Be(Runtime.CreateLogixValue(123456u));
    }

    [Fact]
    public void AUDIntEncodesToExactlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        var value = Runtime.CreateLogixValue(0x12345678u);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x78, 0x56, 0x34, 0x12);
    }

    [Fact]
    public void AUDIntAboveTheSignedRangeEncodesAsItStands()
    {
        // Arrange
        var value = Runtime.CreateLogixValue(uint.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF);
    }
}
