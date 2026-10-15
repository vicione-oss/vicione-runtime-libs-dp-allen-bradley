using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class ByteCodecTests
{
    private static readonly ByteCodec Codec = new();

    [Theory]
    [InlineData(0x00, (byte)0)]
    [InlineData(0x2A, (byte)42)]
    [InlineData(0x7F, (byte)127)]
    [InlineData(0x80, (byte)128)]
    [InlineData(0xFF, byte.MaxValue)]
    public void TheStoredBitsDecodeToTheirValue(byte storedBits, byte expectedValue)
    {
        // Arrange
        byte[] buffer = [storedBits];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void ALargeUnsignedValueIsWrittenUnchanged()
    {
        // Arrange
        byte value = byte.MaxValue;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF);
    }

    [Fact]
    public void OnlyTheFirstByteOfTheBufferIsRead()
    {
        // Arrange
        byte[] buffer = [0x2A, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(42);
    }

    [Fact]
    public void AValueIsWrittenAsOneByte()
    {
        // Arrange
        byte value = 42;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x2A);
    }
}
