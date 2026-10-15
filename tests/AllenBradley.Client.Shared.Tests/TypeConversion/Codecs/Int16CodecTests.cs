using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class Int16CodecTests
{
    private static readonly Int16Codec Codec = new();

    [Theory]
    [InlineData(0x0000, (short)0)]
    [InlineData(0xFFFF, (short)-1)]
    [InlineData(0x7FFF, short.MaxValue)]
    [InlineData(0x8000, short.MinValue)]
    public void TheStoredBitsDecodeToTheirValue(ushort storedBits, short expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void OnlyTheFirstTwoBytesOfTheBufferAreRead()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(4711);
    }

    [Fact]
    public void ANegativeValueIsWrittenInTwosComplement()
    {
        // Arrange
        short value = -1;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF);
    }

    [Fact]
    public void AValueIsWrittenWithItsLeastSignificantByteFirst()
    {
        // Arrange
        short value = 0x1234;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x34, 0x12);
    }
}
