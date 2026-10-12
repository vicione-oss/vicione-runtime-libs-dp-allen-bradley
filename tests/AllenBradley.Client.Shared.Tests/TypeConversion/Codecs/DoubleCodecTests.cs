using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class DoubleCodecTests
{
    private static readonly DoubleCodec Codec = new();

    [Theory]
    [InlineData(0x3FF0000000000000, 1.0d)]
    [InlineData(0xBFF0000000000000, -1.0d)]
    [InlineData(0x0000000000000000, 0.0d)]
    [InlineData(0x400921FB54442D18, Math.PI)]
    public void AStoredBitPatternDecodesToTheValueItStandsFor(ulong storedBits, double expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }
}
