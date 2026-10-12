using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TypeConversion.Codecs;

public sealed class Int32CodecTests
{
    private static readonly Int32Codec Codec = Int32Codec.Instance;

    [Theory]
    [InlineData(0x00000000u, 0)]
    [InlineData(0xFFFFFFFFu, -1)]
    [InlineData(0x7FFFFFFFu, int.MaxValue)]
    [InlineData(0x80000000u, int.MinValue)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(uint storedBits, int expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void ANegativeValueEncodesInTwosComplement()
    {
        // Arrange
        int value = -1;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF);
    }
}
