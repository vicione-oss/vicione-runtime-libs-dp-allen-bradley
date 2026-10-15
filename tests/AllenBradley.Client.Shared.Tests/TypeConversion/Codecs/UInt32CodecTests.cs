using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class UInt32CodecTests
{
    private static readonly UInt32Codec Codec = new();

    [Theory]
    [InlineData(0x00000000u, 0u)]
    [InlineData(0x0001E240u, 123456u)]
    [InlineData(0x7FFFFFFFu, 2147483647u)]
    [InlineData(0x80000000u, 2147483648u)]
    [InlineData(0xEE6B2800u, 4000000000u)]
    [InlineData(0xFFFFFFFFu, uint.MaxValue)]
    public void TheStoredBitsDecodeToTheirValue(uint storedBits, uint expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void OnlyTheFirstFourBytesOfTheBufferAreRead()
    {
        // Arrange
        byte[] buffer = [0x40, 0xE2, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(123456u);
    }

    [Fact]
    public void ALargeUnsignedValueIsWrittenUnchanged()
    {
        // Arrange
        uint value = uint.MaxValue;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF);
    }

    [Fact]
    public void AValueIsWrittenWithItsLeastSignificantByteFirst()
    {
        // Arrange
        uint value = 0x12345678;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x78, 0x56, 0x34, 0x12);
    }
}
