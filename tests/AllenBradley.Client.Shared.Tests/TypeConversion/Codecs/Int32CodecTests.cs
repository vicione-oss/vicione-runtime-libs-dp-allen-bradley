using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class Int32CodecTests
{
    private static readonly Int32Codec Codec = new();

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

    [Fact]
    public void AValueEncodesLeastSignificantByteFirst()
    {
        // Arrange
        int value = 0x12345678;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x78, 0x56, 0x34, 0x12);
    }

    [Fact]
    public void ADecodeReadsOnlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(4711);
    }
}
