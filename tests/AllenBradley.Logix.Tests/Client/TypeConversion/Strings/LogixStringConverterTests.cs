using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Strings;

/// <summary>
/// Buffers are built by hand from the documented structure — <c>.LEN : DINT</c> at offset 0, then
/// <c>.DATA : SINT[82]</c> — so no decode test agrees with the encoder by construction.
/// </summary>
public sealed class LogixStringConverterTests
{
    private const int StructureSize = 88;
    private const int LengthPrefixSize = 4;

    // The padding up to Logix's 32-bit structure boundary is the tag buffer's, not the converter's.
    private const int PayloadSize = LengthPrefixSize + 82;

    private static readonly IDataPointConverter Converter = new LogixStringConverter();

    private static readonly StringDataPoint Label = new(
        new TagName("Program:MainProgram.strValue1"), DefaultPollFrequency, NoChannels, StringMaxLength.Standard);

    private static readonly StringDataPoint ShortLabel =
        new(Label.TagName, DefaultPollFrequency, NoChannels, new StringMaxLength(20));

    [Fact]
    public void LenIsReadFromOffsetZeroAndTheCharactersFromBehindIt()
    {
        // Arrange
        // The offsets hang off libplctag stripping the A0 02 abbreviated-structure prefix.
        var buffer = StructureHolding("Hi");

        // Act
        var decoded = Converter.Decode(Label, buffer);

        // Assert
        decoded.Should().Be(Label.CreateLogixValue("Hi"));
    }

    [Fact]
    public void AnAllZeroStructureDecodesToAnEmptyString()
    {
        // Arrange
        var buffer = new byte[StructureSize];

        // Act
        var decoded = Converter.Decode(Label, buffer);

        // Assert
        decoded.Should().Be(Label.CreateLogixValue(string.Empty));
    }

    [Fact]
    public void WhateverFollowsLenIsIgnored()
    {
        // Arrange
        // A shorter value written over a longer one leaves the old tail in .DATA on the controller.
        var buffer = StructureHolding("Hi");
        "stale"u8.CopyTo(buffer.AsSpan(LengthPrefixSize + 2));

        // Act
        var decoded = Converter.Decode(Label, buffer);

        // Assert
        decoded.Should().Be(Label.CreateLogixValue("Hi"));
    }

    [Fact]
    public void CharactersAboveAsciiSurviveTheDecodeAsLatin1()
    {
        // Arrange
        var buffer = StructureHolding("ÀÉÑÖß");

        // Act
        var decoded = Converter.Decode(Label, buffer);

        // Assert
        decoded.Should().Be(Label.CreateLogixValue("ÀÉÑÖß"));
    }

    [Fact]
    public void AFullLengthValueDecodesToEveryOneOfItsCharacters()
    {
        // Arrange
        var full = new string('X', StringMaxLength.Standard.Value);
        var buffer = StructureHolding(full);

        // Act
        var decoded = Converter.Decode(Label, buffer);

        // Assert
        decoded.Should().Be(Label.CreateLogixValue(full));
    }

    [Fact]
    public void ALenBeyondTheCapacityIsClampedToTheCapacity()
    {
        // Arrange
        var buffer = StructureHolding("Hi");
        buffer[0] = 200;

        // Act
        var decodedText = DecodedTextOf(buffer);

        // Assert
        decodedText.Should().HaveLength(StringMaxLength.Standard.Value);
    }

    [Fact]
    public void ALenBeyondTheBufferIsClampedToWhatArrived()
    {
        // Arrange
        var buffer = StructureHolding("Hi", structureSize: LengthPrefixSize + 8);
        buffer[0] = 82;

        // Act
        var decodedText = DecodedTextOf(buffer);

        // Assert
        decodedText.Should().HaveLength(8);
    }

    [Fact]
    public void ANegativeLenDecodesToAnEmptyString()
    {
        // Arrange
        // .LEN is a signed DINT, and the sign bit is reachable.
        var buffer = StructureHolding("Hi");
        buffer[3] = 0x80;

        // Act
        var decoded = Converter.Decode(Label, buffer);

        // Assert
        decoded.Should().Be(Label.CreateLogixValue(string.Empty));
    }

    [Fact]
    public void AValueEncodesToLenFollowedByItsLatin1Characters()
    {
        // Arrange

        // Act
        var bytes = Converter.Encode(Label.CreateLogixValue("Hi"));

        // Assert
        var expected = new byte[PayloadSize];
        expected[0] = 2;
        expected[LengthPrefixSize] = (byte)'H';
        expected[LengthPrefixSize + 1] = (byte)'i';
        bytes.Should().Equal(expected);
    }

    [Fact]
    public void AnEmptyStringEncodesToAZeroLengthAndNothingElse()
    {
        // Arrange

        // Act
        var bytes = Converter.Encode(Label.CreateLogixValue(string.Empty));

        // Assert
        bytes.Should().Equal(new byte[PayloadSize]);
    }

    [Fact]
    public void AnEncodeCoversTheWholeOfDataSoAShorterValueLeavesNoOldTailBehind()
    {
        // Arrange

        // Act
        var bytes = Converter.Encode(Label.CreateLogixValue("Hi"));

        // Assert
        bytes.Should().HaveCount(PayloadSize);
        bytes.AsSpan(LengthPrefixSize + 2).ToArray().Should().AllSatisfy(b => b.Should().Be(0));
    }

    [Theory]
    [InlineData("Hello World", "Hello World")]
    [InlineData("ÀÉÑÖß", "ÀÉÑÖß")]
    // Characters outside Latin-1 encode to the substitution character, one per character.
    [InlineData("A€BДC中DשE", "A?B?C?D?E")]
    [InlineData("Hello\r\nWorld", "Hello\r\nWorld")]
    [InlineData("Hello\tWorld", "Hello\tWorld")]
    [InlineData("", "")]
    public void AValueRoundTripsThroughTheStructureAsLatin1(string valueToWrite, string expectedText)
    {
        // Arrange
        var bytes = Converter.Encode(Label.CreateLogixValue(valueToWrite));

        // Act
        var decodedText = DecodedTextOf(bytes);

        // Assert
        decodedText.Should().Be(expectedText);
    }

    [Fact]
    public void AFullLengthValueRoundTripsWithDataFilledExactly()
    {
        // Arrange
        var full = new string('X', StringMaxLength.Standard.Value);
        var bytes = Converter.Encode(Label.CreateLogixValue(full));

        // Act
        var decodedText = DecodedTextOf(bytes);

        // Assert
        bytes[0].Should().Be(82);
        decodedText.Should().Be(full);
    }

    [Fact]
    public void AValueBeyondTheCapacitySaysWhichTagAndWhatCapacityRefusedIt()
    {
        // Arrange
        var tooLong = new string('X', StringMaxLength.Standard.Value + 1);

        // Act
        var encoding = Converter.Invoking(c => c.Encode(Label.CreateLogixValue(tooLong)));

        // Assert
        encoding.Should().Throw<InvalidOperationException>()
            .WithMessage("*strValue1*").WithMessage("*82*");
    }

    [Fact]
    public void AValueIsSizedAgainstItsOwnDataPointsCapacity()
    {
        // Arrange
        var full = new string('X', ShortLabel.MaxLength.Value);

        // Act
        var bytes = Converter.Encode(ShortLabel.CreateLogixValue(full));

        // Assert
        bytes.Should().HaveCount(LengthPrefixSize + ShortLabel.MaxLength.Value);
        bytes[0].Should().Be((byte)ShortLabel.MaxLength.Value);
    }

    [Fact]
    public void AValueOneCharacterBeyondItsOwnDataPointsCapacityIsRefused()
    {
        // Arrange
        var oneTooMany = new string('X', ShortLabel.MaxLength.Value + 1);

        // Act
        var encoding = Converter.Invoking(c => c.Encode(ShortLabel.CreateLogixValue(oneTooMany)));

        // Assert
        encoding.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TheConverterExpectsTheStudio5000SpellingOfTheTypeName()
    {
        // Arrange

        // Act
        var typeName = Converter.ExpectedTypeName;

        // Assert
        typeName.Should().Be("STRING");
    }

    [Fact]
    public void DecodingForANonStringDataPointSaysTheRegistryRoutedTheWrongConverter()
    {
        // Arrange
        var dInt = new DIntDataPoint(Label.TagName, DefaultPollFrequency, NoChannels);

        // Act
        var decoding = Converter.Invoking(c => c.Decode(dInt, new byte[StructureSize]));

        // Assert
        decoding.Should().Throw<InvalidOperationException>().WithMessage("*DataPointConverterRegistry*");
    }

    [Fact]
    public void EncodingAValueNoDataPointMadeSaysWhatItExpected()
    {
        // Arrange
        var foreign = new ForeignDataPointValue(Label);

        // Act
        var encoding = Converter.Invoking(c => c.Encode(foreign));

        // Assert
        encoding.Should().Throw<InvalidOperationException>()
            .WithMessage("*strValue1*").WithMessage("*String*");
    }

    private static byte[] StructureHolding(string value, int structureSize = StructureSize)
    {
        var buffer = new byte[structureSize];
        buffer[0] = (byte)value.Length;
        Encoding.Latin1.GetBytes(value).CopyTo(buffer, LengthPrefixSize);

        return buffer;
    }

    private static string DecodedTextOf(byte[] buffer) => (string)Converter.Decode(Label, buffer).Value!;

    // An ILogixDataPointValue that no data point made: the only shape the encode guard can ever reject.
    private sealed record ForeignDataPointValue(ILogixDataPoint DataPoint) : ILogixDataPointValue
    {
        public object? Value => null;

        public bool IsInValueRange() => false;
    }
}
