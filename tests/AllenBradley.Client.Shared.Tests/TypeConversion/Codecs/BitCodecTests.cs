using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class BitCodecTests
{
    private static readonly BitCodec Codec = new();

    [Theory]
    [InlineData(0x00, false)]
    [InlineData(0x01, true)]
    [InlineData(0xFF, true)]
    public void AByteDecodesToTrueUnlessItIsZero(byte storedByte, bool expectedValue)
    {
        // Arrange
        byte[] buffer = [storedByte];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(true, 0x01)]
    [InlineData(false, 0x00)]
    public void ABitIsWrittenAsOneByteOfOneOrZero(bool value, byte expectedByte)
    {
        // Arrange

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(expectedByte);
    }
}
