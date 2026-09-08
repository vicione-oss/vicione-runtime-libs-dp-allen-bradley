using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>UDINT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Four bytes, unsigned, least significant
/// first.
/// </summary>
public class UDIntConverterTests
{
    private static readonly IDataPointConverter Converter = new UDIntConverter();

    private static readonly UDIntDataPoint Runtime = new(new TagName("udintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagAUDInt()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the whole of what tells this
        // converter from the DINT one: the two occupy the same four bytes and disagree only about what
        // the top bit means, so a DINT tag read through here hands back 4294967295 where it holds -1.
        expectedDataType.Should().Be(AllenBradleyDataType.Udint);
    }

    [Fact]
    public void Decode_ReadsTheFourBytesLeastSignificantFirst()
    {
        // Arrange
        // Every byte differs, so a swapped or rotated read cannot land on the same value.
        byte[] buffer = [0x78, 0x56, 0x34, 0x12];

        // Act
        var value = Converter.Decode(Runtime, buffer);

        // Assert
        value.Value.Should().Be(0x12345678u);
    }

    [Theory]
    [InlineData(0x00000000u, 0u)]
    [InlineData(0x0001E240u, 123456u)]
    [InlineData(0x7FFFFFFFu, 2147483647u)]
    [InlineData(0x80000000u, 2147483648u)]
    [InlineData(0xFFFFFFFFu, uint.MaxValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(uint bits, uint expected)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(bits);

        // Act
        var value = Converter.Decode(Runtime, buffer);

        // Assert
        // The top half of the range is the point: 0x80000000 and 0xFFFFFFFF are where an unsigned decode
        // parts company with the signed one, which reads them as int.MinValue and -1.
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Decode_ReadsOnlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x40, 0xE2, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var value = Converter.Decode(Runtime, buffer);

        // Assert
        value.Value.Should().Be(123456u);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Runtime.CreateLogixValue(0x12345678u);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the four bytes a UDINT occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0x78, 0x56, 0x34, 0x12);
    }

    [Fact]
    public void Encode_WritesAValueAboveTheSignedRangeAsItStands()
    {
        // Arrange
        var value = Runtime.CreateLogixValue(uint.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF);
    }
}
