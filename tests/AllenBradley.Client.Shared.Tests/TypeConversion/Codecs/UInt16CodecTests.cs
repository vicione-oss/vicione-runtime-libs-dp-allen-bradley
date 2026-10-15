using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tests.TypeConversion.Codecs;

public sealed class UInt16CodecTests
{
    private static readonly UInt16Codec Codec = new();

    [Theory]
    [InlineData(0x0000, (ushort)0)]
    [InlineData(0x1092, (ushort)4242)]
    [InlineData(0x7FFF, (ushort)32767)]
    [InlineData(0x8000, (ushort)32768)]
    [InlineData(0xFFFF, ushort.MaxValue)]
    public void TheStoredBitsDecodeToTheirValue(ushort storedBits, ushort expectedValue)
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
        byte[] buffer = [0x92, 0x10, 0xFF, 0xFF];

        // Act
        var decoded = Codec.Decode(buffer);

        // Assert
        decoded.Should().Be(4242);
    }

    [Fact]
    public void ALargeUnsignedValueIsWrittenUnchanged()
    {
        // Arrange
        ushort value = ushort.MaxValue;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF);
    }

    [Fact]
    public void AValueIsWrittenWithItsLeastSignificantByteFirst()
    {
        // Arrange
        ushort value = 0x1234;

        // Act
        var bytes = Codec.Encode(value);

        // Assert
        bytes.Should().Equal(0x34, 0x12);
    }
}
