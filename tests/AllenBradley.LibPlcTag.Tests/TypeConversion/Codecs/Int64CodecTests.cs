using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TypeConversion.Codecs;

public sealed class Int64CodecTests
{
    private static readonly Int64Codec Codec = Int64Codec.Instance;

    [Theory]
    [InlineData(0UL, 0L)]
    [InlineData(ulong.MaxValue, -1L)]
    [InlineData(0x7FFFFFFFFFFFFFFFUL, long.MaxValue)]
    [InlineData(0x8000000000000000UL, long.MinValue)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(ulong storedBits, long expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void ADecodeReadsOnlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(1L);
    }

    [Fact]
    public void ANegativeValueEncodesInTwosComplement()
    {
        // Arrange
        long value = -1L;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
    }
}
