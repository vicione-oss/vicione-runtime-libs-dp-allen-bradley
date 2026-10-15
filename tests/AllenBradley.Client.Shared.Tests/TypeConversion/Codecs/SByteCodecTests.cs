using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class SByteCodecTests
{
    private static readonly SByteCodec Codec = new();

    [Theory]
    [InlineData(0x00, (sbyte)0)]
    [InlineData(0x2A, (sbyte)42)]
    [InlineData(0xFF, (sbyte)-1)]
    [InlineData(0x7F, sbyte.MaxValue)]
    [InlineData(0x80, sbyte.MinValue)]
    public void TheStoredBitsDecodeToTheirValue(byte storedBits, sbyte expectedValue)
    {
        // Arrange
        byte[] buffer = [storedBits];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void ANegativeValueIsWrittenInTwosComplement()
    {
        // Arrange
        sbyte value = -1;

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
        sbyte value = 42;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x2A);
    }
}
