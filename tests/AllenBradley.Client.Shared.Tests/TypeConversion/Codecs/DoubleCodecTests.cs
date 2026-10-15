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
    [InlineData(0x7FEFFFFFFFFFFFFF, double.MaxValue)]
    [InlineData(0xFFEFFFFFFFFFFFFF, double.MinValue)]
    public void TheStoredBitsDecodeToTheirValue(ulong storedBits, double expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Fact]
    public void AValueIsWrittenWithItsLeastSignificantByteFirst()
    {
        // Arrange
        double value = 1.0d;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F);
    }
}
