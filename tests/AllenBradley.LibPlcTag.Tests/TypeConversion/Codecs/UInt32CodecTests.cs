using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TypeConversion.Codecs;

public sealed class UInt32CodecTests
{
    private static readonly UInt32Codec Codec = UInt32Codec.Instance;

    [Theory]
    [InlineData(0x00000000u, 0u)]
    [InlineData(0x0001E240u, 123456u)]
    [InlineData(0x7FFFFFFFu, 2147483647u)]
    [InlineData(0x80000000u, 2147483648u)]
    [InlineData(0xFFFFFFFFu, uint.MaxValue)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(uint storedBits, uint expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void ADecodeReadsOnlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x40, 0xE2, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(123456u);
    }

    [Fact]
    public void AValueAboveTheSignedRangeEncodesAsItStands()
    {
        // Arrange
        uint value = uint.MaxValue;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF);
    }
}
