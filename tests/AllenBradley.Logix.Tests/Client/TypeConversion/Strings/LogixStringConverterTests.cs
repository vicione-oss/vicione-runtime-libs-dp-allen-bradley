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
/// The Logix <c>STRING</c> codec, against buffers built by hand from the documented structure —
/// <c>.LEN : DINT</c> at offset 0, then <c>.DATA : SINT[82]</c>, the whole padded to 88 bytes. This is
/// where the wire layout is pinned; the device-tier round trip proves the same bytes survive a real
/// controller, but it cannot run without one and this can.
/// </summary>
public class LogixStringConverterTests
{
    private const int StructureSize = 88;
    private const int LengthPrefixSize = 4;

    private static readonly IDataPointConverter Converter = new LogixStringConverter();

    private static readonly StringDataPoint Label = new(new TagName("Program:MainProgram.strValue1"), DefaultPollFrequency, NoChannels, new StringMaxLength(StringMaxLength.Standard.Value));

    // A STRING structure as it sits in the tag buffer, spelled out rather than produced by the encoder,
    // which would make the decode test agree with itself by construction.
    private static byte[] StructureHolding(string value, int structureSize = StructureSize)
    {
        var buffer = new byte[structureSize];
        buffer[0] = (byte)value.Length;
        Encoding.Latin1.GetBytes(value).CopyTo(buffer, LengthPrefixSize);
        return buffer;
    }

    private static string Decode(byte[] buffer) =>
        Converter.Decode(Label, buffer).Value.Should().BeOfType<string>().Subject;

    // The buffer the tag hands the write batch: .LEN plus .DATA[n], padded up to Logix's 32-bit
    // structure boundary. The converter no longer sizes it — the controller's own declaration does —
    // so the test builds it the way the layout says it is built.
    private static int StructureSizeFor(int maxLength) =>
        (LengthPrefixSize + maxLength + 3) / 4 * 4;

    private static byte[] Encode(string value, StringDataPoint? dataPoint = null)
    {
        var point = dataPoint ?? Label;
        var buffer = new byte[StructureSizeFor(point.MaxLength.Value)];
        Converter.Encode(point.CreateLogixValue(value), buffer);
        return buffer;
    }

    [Fact]
    public void Decode_ReadsLenAtOffsetZeroAndDataAfterIt()
    {
        // Arrange
        // The offsets that hang off libplctag stripping the A0 02 abbreviated-structure prefix. If it
        // ever stopped doing that, this is the test that would say so.
        var buffer = StructureHolding("Hi");

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().Be("Hi");
    }

    [Fact]
    public void Decode_AnEmptyString_ReadsBackEmpty()
    {
        // Arrange
        var buffer = new byte[StructureSize];

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().BeEmpty();
    }

    [Fact]
    public void Decode_StopsAtLenAndIgnoresWhateverFollowsIt()
    {
        // Arrange
        // A shorter value written over a longer one leaves the old tail in .DATA on the controller. Only
        // .LEN says where the value ends.
        var buffer = StructureHolding("Hi");
        "stale"u8.CopyTo(buffer.AsSpan(LengthPrefixSize + 2));

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().Be("Hi");
    }

    [Fact]
    public void Decode_KeepsLatin1CharactersAboveAscii()
    {
        // Arrange
        var buffer = StructureHolding("ÀÉÑÖß");

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().Be("ÀÉÑÖß");
    }

    [Fact]
    public void Decode_AFullLengthValue_ReadsEveryCharacter()
    {
        // Arrange
        var full = new string('X', StringMaxLength.Standard.Value);
        var buffer = StructureHolding(full);

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().Be(full);
    }

    [Fact]
    public void Decode_WhenLenExceedsTheCapacity_ClampsToTheCapacity()
    {
        // Arrange
        // The controller claiming more characters than .DATA can hold is either corruption or a tag that
        // is not the type we think it is. Honouring the claim would read the padding as text.
        var buffer = StructureHolding("Hi");
        buffer[0] = 200;

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().HaveLength(StringMaxLength.Standard.Value);
    }

    [Fact]
    public void Decode_WhenLenExceedsTheBuffer_ClampsToWhatArrived()
    {
        // Arrange
        var buffer = StructureHolding("Hi", structureSize: LengthPrefixSize + 8);
        buffer[0] = 82;

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().HaveLength(8);
    }

    [Fact]
    public void Decode_WhenLenIsNegative_ReadsNothing()
    {
        // Arrange
        var buffer = StructureHolding("Hi");
        buffer[3] = 0x80; // .LEN is a signed DINT, and the sign bit is reachable.

        // Act
        var value = Decode(buffer);

        // Assert
        value.Should().BeEmpty();
    }

    [Fact]
    public void Encode_WritesLenThenTheLatin1Characters()
    {
        // Arrange

        // Act
        var buffer = Encode("Hi");

        // Assert
        buffer.Should().HaveCount(StructureSize);
        buffer.AsSpan(0, LengthPrefixSize).ToArray().Should().Equal(2, 0, 0, 0);
        buffer.AsSpan(LengthPrefixSize, 2).ToArray().Should().Equal((byte)'H', (byte)'i');
        buffer.AsSpan(LengthPrefixSize + 2).ToArray().Should().AllSatisfy(b => b.Should().Be(0));
    }

    [Fact]
    public void Encode_AnEmptyString_WritesAZeroLengthAndNothingElse()
    {
        // Arrange

        // Act
        var buffer = Encode(string.Empty);

        // Assert
        buffer.Should().AllSatisfy(b => b.Should().Be(0));
    }

    [Fact]
    public void Encode_ZeroesTheTailSoAShorterValueDoesNotLeaveTheOldOneBehind()
    {
        // Arrange
        var buffer = Encode("Hi");
        Array.Fill(buffer, (byte)0xEE, LengthPrefixSize + 2, 4);

        // Act
        Converter.Encode(Label.CreateLogixValue("Hi"), buffer);

        // Assert
        buffer.AsSpan(LengthPrefixSize + 2).ToArray().Should().AllSatisfy(b => b.Should().Be(0));
    }

    [Theory]
    [InlineData("Hello World", "Hello World")] // plain ASCII
    [InlineData("ÀÉÑÖß", "ÀÉÑÖß")] // Latin-1 extended, preserved
    [InlineData("A€BДC中DשE", "A?B?C?D?E")] // outside Latin-1 — one '?' per character
    [InlineData("Hello\r\nWorld", "Hello\r\nWorld")] // CR+LF
    [InlineData("Hello\tWorld", "Hello\tWorld")] // tab
    [InlineData("", "")]
    public void EncodeThenDecode_RoundTripsTheValueThroughLatin1(string valueToWrite, string expected)
    {
        // Arrange

        // Act
        var roundTripped = Decode(Encode(valueToWrite));

        // Assert
        roundTripped.Should().Be(expected);
    }

    [Fact]
    public void EncodeThenDecode_AFullLengthValue_FillsDataExactly()
    {
        // Arrange
        var full = new string('X', StringMaxLength.Standard.Value);

        // Act
        var buffer = Encode(full);
        var roundTripped = Decode(buffer);

        // Assert
        buffer[0].Should().Be(82);
        roundTripped.Should().Be(full);
    }

    [Fact]
    public void Encode_WhenTheValueExceedsTheCapacity_ThrowsRatherThanTruncating()
    {
        // Arrange
        // Truncating would write a value the caller never asked for and report success for it.
        var tooLong = new string('X', StringMaxLength.Standard.Value + 1);
        var buffer = new byte[StructureSize];

        // Act
        var encode = Converter.Invoking(c => c.Encode(Label.CreateLogixValue(tooLong), buffer));

        // Assert
        encode.Should().Throw<InvalidOperationException>()
            .WithMessage("*strValue1*").WithMessage("*82*");
    }

    [Fact]
    public void Encode_SizesTheValueAgainstTheDataPointsOwnCapacity()
    {
        // Arrange
        // Twenty characters fit a STRING and overflow a STRING_20 by one. The converter is the same
        // object either way; the capacity is the data point's.
        var short20 = new StringDataPoint(Label.TagName, DefaultPollFrequency, NoChannels, new StringMaxLength(20));

        // Act
        var buffer = Encode(new string('X', 20), short20);
        var oneTooMany = Record.Exception(() => Encode(new string('X', 21), short20));

        // Assert
        buffer[0].Should().Be(20);
        oneTooMany.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void ExpectedTypeName_IsTheStudio5000Spelling()
    {
        // Arrange

        // Act
        var typeName = Converter.ExpectedTypeName;

        // Assert
        typeName.Should().Be(new LogixDataTypeName("STRING"));
    }


    [Fact]
    public void Decode_WhenTheDataPointIsNotAString_SaysTheRegistryRoutedTheWrongConverter()
    {
        // Arrange
        var dInt = new DIntDataPoint(Label.TagName, DefaultPollFrequency, NoChannels);

        // Act
        var decode = Converter.Invoking(c => c.Decode(dInt, new byte[StructureSize]));

        // Assert
        decode.Should().Throw<InvalidOperationException>().WithMessage("*DataPointConverterRegistry*");
    }

    [Fact]
    public void Encode_WhenTheValueIsNotTheDataPointsOwn_SaysWhatItExpected()
    {
        // Arrange
        // The write-side mirror of the wrong-converter case, and the one shape a Bad value arrives in:
        // it carries no payload, and there is nothing to write.
        var bad = new BadLogixDataPointValue(Label);

        // Act
        var encode = Converter.Invoking(c => c.Encode(bad, new byte[StructureSize]));

        // Assert
        encode.Should().Throw<InvalidOperationException>()
            .WithMessage("*strValue1*").WithMessage("*String*");
    }
}
