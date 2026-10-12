using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TypeConversion.Codecs;

public sealed class Int16CodecTests
{
    private static readonly Int16Codec Codec = Int16Codec.Instance;

    [Theory]
    [InlineData(0x0000, (short)0)]
    [InlineData(0xFFFF, (short)-1)]
    [InlineData(0x7FFF, short.MaxValue)]
    [InlineData(0x8000, short.MinValue)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(ushort storedBits, short expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void ADecodeReadsOnlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(4711);
    }

    [Fact]
    public void ANegativeValueEncodesInTwosComplement()
    {
        // Arrange
        short value = -1;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF);
    }
}
