using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class UInt64CodecTests
{
    private static readonly UInt64Codec Codec = new();

    [Theory]
    [InlineData(0x0000000000000000ul, 0ul)]
    [InlineData(0x7FFFFFFFFFFFFFFFul, 9223372036854775807ul)]
    [InlineData(0x8000000000000000ul, 9223372036854775808ul)]
    [InlineData(0xFFFFFFFFFFFFFFFEul, 18446744073709551614ul)]
    [InlineData(0xFFFFFFFFFFFFFFFFul, ulong.MaxValue)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(ulong storedBits, ulong expectedValue)
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
        byte[] buffer = [0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(42ul);
    }

    [Fact]
    public void AValueAboveTheSignedRangeEncodesAsItStands()
    {
        // Arrange
        ulong value = ulong.MaxValue;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
    }
}
