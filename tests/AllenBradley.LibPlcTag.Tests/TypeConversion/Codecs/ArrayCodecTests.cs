using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tests.TypeConversion.Codecs;

public sealed class ArrayCodecTests
{
    private static readonly ArrayCodec<short> ThreeShorts = new(Int16Codec.Instance, 3);

    // Least significant byte first, each element asymmetric in its bytes.
    private static readonly byte[] StoredShorts = [0x01, 0x00, 0x34, 0x12, 0xFF, 0xFF];

    [Fact]
    public void AnArrayOccupiesItsElementWidthOnceForEveryElement()
    {
        // Arrange

        // Act
        var size = ThreeShorts.Size;

        // Assert
        size.Should().Be(6);
    }

    [Fact]
    public void TheStoredBytesDecodeToTheElementsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = ThreeShorts.Decode(StoredShorts);

        // Assert
        decoded.Should().Equal(1, 0x1234, -1);
    }

    [Fact]
    public void TheElementsEncodeToTheStoredBytesInIndexOrder()
    {
        // Arrange
        short[] values = [1, 0x1234, -1];

        // Act
        var encoded = ThreeShorts.Encode(values);

        // Assert
        encoded.Should().Equal(StoredShorts);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(8)]
    public void ABufferThatIsNotExactlyTheArraysWidthIsRefused(int bufferLength)
    {
        // Arrange
        var buffer = new byte[bufferLength];

        // Act
        var decoding = () => ThreeShorts.Decode(buffer);

        // Assert
        decoding.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void AValueThatDoesNotHoldTheCountedElementsIsRefused(int elementCount)
    {
        // Arrange
        var values = new short[elementCount];

        // Act
        var encoding = () => ThreeShorts.Encode(values);

        // Assert
        encoding.Should().Throw<ArgumentException>();
    }
}
