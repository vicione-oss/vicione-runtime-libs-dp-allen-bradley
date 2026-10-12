using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TypeConversion.Codecs;

public sealed class SByteCodecTests
{
    private static readonly SByteCodec Codec = new();

    [Theory]
    [InlineData(0x00, (sbyte)0)]
    [InlineData(0x2A, (sbyte)42)]
    [InlineData(0xFF, (sbyte)-1)]
    [InlineData(0x7F, sbyte.MaxValue)]
    [InlineData(0x80, sbyte.MinValue)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(byte storedBits, sbyte expectedValue)
    {
        // Arrange
        byte[] buffer = [storedBits];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void ANegativeValueEncodesInTwosComplement()
    {
        // Arrange
        sbyte value = -1;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF);
    }
}
