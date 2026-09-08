using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>ULINT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Eight bytes, unsigned, least significant
/// first.
/// </summary>
public class ULIntConverterTests
{
    private static readonly IDataPointConverter Converter = new ULIntConverter();

    private static readonly ULIntDataPoint Cycles = new(new TagName("ulintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagAULInt()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the whole of what tells this
        // converter from the LINT one: the two occupy the same eight bytes and disagree only about what
        // the top bit means, so a LINT tag read through here hands back ulong.MaxValue where it holds -1.
        expectedDataType.Should().Be(AllenBradleyDataType.Ulint);
    }

    [Fact]
    public void Decode_ReadsTheEightBytesLeastSignificantFirst()
    {
        // Arrange
        // Every byte differs, so a swapped or rotated read cannot land on the same value. This is the
        // widest type the port has, and the one where a byte-order mistake has the most room to hide.
        byte[] buffer = [0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12];

        // Act
        var value = Converter.Decode(Cycles, buffer);

        // Assert
        value.Value.Should().Be(0x123456789ABCDEF0ul);
    }

    [Theory]
    [InlineData(0x0000000000000000ul, 0ul)]
    [InlineData(0x7FFFFFFFFFFFFFFFul, 9223372036854775807ul)]
    [InlineData(0x8000000000000000ul, 9223372036854775808ul)]
    [InlineData(0xFFFFFFFFFFFFFFFEul, 18446744073709551614ul)]
    [InlineData(0xFFFFFFFFFFFFFFFFul, ulong.MaxValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(ulong bits, ulong expected)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(bits);

        // Act
        var value = Converter.Decode(Cycles, buffer);

        // Assert
        // The top half of the range is the point: 0x8000000000000000 and above are where an unsigned
        // decode parts company with the signed one, which reads them as long.MinValue and upwards to -1.
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Decode_ReadsOnlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var value = Converter.Decode(Cycles, buffer);

        // Assert
        value.Value.Should().Be(42ul);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Cycles.CreateLogixValue(0x123456789ABCDEF0ul);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the eight bytes a ULINT occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12);
    }

    [Fact]
    public void Encode_WritesAValueAboveTheSignedRangeAsItStands()
    {
        // Arrange
        var value = Cycles.CreateLogixValue(ulong.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
    }
}
