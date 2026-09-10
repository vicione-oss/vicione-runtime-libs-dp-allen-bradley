using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class IntArrayConverterTests
{
    private const int DeclaredElementCount = 10;

    private static readonly IDataPointConverter Converter = new IntArrayConverter();

    private static readonly IntArrayDataPoint Readings = new(
        new TagName("intArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

    // Least significant byte first, holding both ends of the range and one value asymmetric in its bytes.
    private static readonly byte[] TenStoredInts =
    [
        0x00, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0x67, 0x12, 0x00, 0x80,
        0xFF, 0x7F, 0x0A, 0x00, 0x14, 0x00, 0x1E, 0x00, 0x28, 0x00,
    ];

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheElementsInts()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Int);
    }

    [Fact]
    public void TheStoredBytesDecodeToTheDeclaredNumberOfShortsInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Readings, TenStoredInts);

        // Assert
        short[] expected = [0, 1, -1, 4711, short.MinValue, short.MaxValue, 10, 20, 30, 40];
        decoded.Value.Should().BeOfType<short[]>().Which.Should().Equal(expected);
    }

    [Fact]
    public void ABufferHoldingMoreThanTheDeclaredCountIsRefusedRatherThanTrimmed()
    {
        // Arrange
        byte[] elevenInts = [.. TenStoredInts, 0x2C, 0x00];

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(Readings, elevenInts));

        // Assert
        decoding.Should().Throw<LogixDecodeException>()
            .WithMessage("*intArray1*returned 11 elements*configured with 10*");
    }

    [Fact]
    public void ABufferHoldingFewerThanTheDeclaredCountIsRefusedRatherThanPartlyDecoded()
    {
        // Arrange
        var sixInts = new byte[12];

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(Readings, sixInts));

        // Assert
        decoding.Should().Throw<LogixDecodeException>()
            .WithMessage("*intArray1*returned 6 elements*configured with 10*");
    }

    [Fact]
    public void WritingAnIntArrayIsRefusedSayingItIsNotSupportedYet()
    {
        // Arrange
        var value = Readings.CreateLogixValue([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>()
            .WithMessage("*not supported yet*");
    }
}
