using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class SingleCodecTests
{
    private static readonly SingleCodec Codec = new();

    [Theory]
    [InlineData(0x3F800000u, 1.0f)]
    [InlineData(0xBF800000u, -1.0f)]
    [InlineData(0x00000000u, 0.0f)]
    [InlineData(0x40490FDBu, MathF.PI)]
    [InlineData(0x7F7FFFFFu, float.MaxValue)]
    [InlineData(0xFF7FFFFFu, float.MinValue)]
    public void TheStoredBitsDecodeToTheirValue(uint storedBits, float expectedValue)
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
        float value = 1.0f;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x00, 0x00, 0x80, 0x3F);
    }
}
