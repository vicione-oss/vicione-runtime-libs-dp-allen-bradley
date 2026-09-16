using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Booleans;

public sealed class BoolArrayConverterTests
{
    private const int DeclaredBitCount = 32;

    private static readonly IDataPointConverter Converter = new BoolArrayConverter();

    private static readonly BoolArrayDataPoint Flags = new(
        TagPath.Parse("boolArray1"), DefaultPollFrequency, NoChannels, new ElementCount(DeclaredBitCount));

    // One 32-bit word, least significant bit of the first byte first: bits 0, 7, 9 and 31 set. Both ends
    // of the word and a byte boundary, so a bit read at the wrong shift or out of the wrong byte shows.
    private static readonly byte[] OneStoredWord = [0x81, 0x02, 0x00, 0x80];

    private static readonly int[] SetBitsOfOneStoredWord = [0, 7, 9, 31];

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheElementsBools()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Bool);
    }

    [Fact]
    public void TheConverterExpectsAOneDimensionalTag()
    {
        // Arrange

        // Act
        var expectedDimensionCount = Converter.ExpectedDimensionCount;

        // Assert
        expectedDimensionCount.Should().Be(DimensionCount.OneDimensional);
    }

    [Fact]
    public void TheControllerMustDeclareAsManyElementsAsTheTagIsConfiguredWithBits()
    {
        // Arrange

        // Act
        var elementCount = Converter.ElementCountFor(Flags);

        // Assert
        elementCount.Should().Be(new ElementCount(DeclaredBitCount));
    }

    [Fact]
    public void ThePackedWordDecodesToOneBoolPerBitInIndexOrder()
    {
        // Arrange

        // Act
        var decoded = Converter.Decode(Flags, OneStoredWord);

        // Assert
        var expected = BitsSetAt(DeclaredBitCount, SetBitsOfOneStoredWord);
        decoded.Value.Should().BeOfType<bool[]>().Which.Should().Equal(expected);
    }

    [Fact]
    public void ABitInTheSecondWordDecodesAtTheIndexThatWordBeginsAt()
    {
        // Arrange
        // The lowest bit of the fifth byte, which is where the second 32-bit word starts.
        var sixtyFourFlags = Flags with { ElementCount = new ElementCount(64) };
        byte[] twoStoredWords = [0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00];

        // Act
        var decoded = Converter.Decode(sixtyFourFlags, twoStoredWords);

        // Assert
        var expected = BitsSetAt(64, [32]);
        decoded.Value.Should().BeOfType<bool[]>().Which.Should().Equal(expected);
    }

    [Fact]
    public void ABufferShorterThanTheWordsTheCountFillsIsRefusedRatherThanPartlyDecoded()
    {
        // Arrange
        var halfAWord = new byte[2];

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(Flags, halfAWord));

        // Assert
        decoding.Should().Throw<LogixDecodeException>()
            .WithMessage("*boolArray1*returned 2 bytes*32 elements packed into 4 bytes*");
    }

    [Fact]
    public void ABufferHoldingMoreWordsThanTheCountFillsIsRefusedRatherThanTrimmed()
    {
        // Arrange
        byte[] twoWords = [.. OneStoredWord, .. OneStoredWord];

        // Act
        var decoding = Converter.Invoking(converter => converter.Decode(Flags, twoWords));

        // Assert
        decoding.Should().Throw<LogixDecodeException>()
            .WithMessage("*boolArray1*returned 8 bytes*32 elements packed into 4 bytes*");
    }

    [Fact]
    public void TheDeclaredNumberOfBoolsEncodesToThePackedWordTheControllerStores()
    {
        // Arrange
        var value = Flags.CreateLogixValue(BitsSetAt(DeclaredBitCount, SetBitsOfOneStoredWord));

        // Act
        var encoded = Converter.Encode(value);

        // Assert
        encoded.Should().Equal(OneStoredWord);
    }

    [Fact]
    public void AValueHoldingMoreThanTheDeclaredCountIsRefusedRatherThanTruncated()
    {
        // Arrange
        var value = Flags.CreateLogixValue(new bool[DeclaredBitCount + 1]);

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>()
            .WithMessage("*boolArray1*holds 33 elements*configured with 32*");
    }

    [Fact]
    public void AValueHoldingFewerThanTheDeclaredCountIsRefusedRatherThanPartlyWritten()
    {
        // Arrange
        var value = Flags.CreateLogixValue(new bool[DeclaredBitCount - 1]);

        // Act
        var encoding = Converter.Invoking(converter => converter.Encode(value));

        // Assert
        encoding.Should().Throw<InvalidOperationException>()
            .WithMessage("*boolArray1*holds 31 elements*configured with 32*");
    }

    private static bool[] BitsSetAt(int bitCount, int[] setIndices)
    {
        var bits = new bool[bitCount];
        foreach (var index in setIndices)
        {
            bits[index] = true;
        }

        return bits;
    }
}
