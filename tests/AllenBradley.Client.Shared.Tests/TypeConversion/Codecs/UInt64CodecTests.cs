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
    public void TheStoredBitsDecodeToTheirValue(ulong storedBits, ulong expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void OnlyTheFirstEightBytesOfTheBufferAreRead()
    {
        // Arrange
        byte[] buffer = [0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(42ul);
    }

    [Fact]
    public void ALargeUnsignedValueIsWrittenUnchanged()
    {
        // Arrange
        ulong value = ulong.MaxValue;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
    }

    [Fact]
    public void AValueIsWrittenWithItsLeastSignificantByteFirst()
    {
        // Arrange
        ulong value = 0x0102030405060708;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01);
    }
}
