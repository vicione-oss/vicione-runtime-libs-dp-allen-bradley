using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TypeConversion.Codecs;

public sealed class SingleCodecTests
{
    private static readonly SingleCodec Codec = new();

    [Theory]
    [InlineData(0x3F800000u, 1.0f)]
    [InlineData(0xBF800000u, -1.0f)]
    [InlineData(0x00000000u, 0.0f)]
    [InlineData(0x40490FDBu, MathF.PI)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(uint storedBits, float expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }
}
