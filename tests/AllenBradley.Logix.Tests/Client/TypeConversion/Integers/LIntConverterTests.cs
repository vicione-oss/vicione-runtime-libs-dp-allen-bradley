using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>LINT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Eight bytes, signed, least significant
/// first: the first type wide enough that the shared little-endian assumption is worth asserting rather
/// than assuming.
/// </summary>
public class LIntConverterTests
{
    private static readonly IDataPointConverter Converter = new LIntConverter();

    private static readonly LIntDataPoint Ticks = new(new TagName("lintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagALInt()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the one thing about this
        // converter a round trip cannot catch: decoding eight bytes off a DINT reads past the tag.
        expectedDataType.Should().Be(AllenBradleyDataType.Lint);
    }

    [Fact]
    public void Decode_ReadsASignedLongLeastSignificantByteFirst()
    {
        // Arrange
        // 9223372036854775806 — one below long.MaxValue, so every byte but the last is 0xFF and a
        // swapped byte order cannot pass by symmetry.
        byte[] buffer = [0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F];

        // Act
        var value = Converter.Decode(Ticks, buffer);

        // Assert
        value.Value.Should().Be(9223372036854775806L);
    }

    [Theory]
    [InlineData(0UL, 0L)]
    [InlineData(ulong.MaxValue, -1L)]
    [InlineData(0x7FFFFFFFFFFFFFFFUL, long.MaxValue)]
    [InlineData(0x8000000000000000UL, long.MinValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(ulong bits, long expected)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(bits);

        // Act
        var value = Converter.Decode(Ticks, buffer);

        // Assert
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Decode_ReadsOnlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF];

        // Act
        var value = Converter.Decode(Ticks, buffer);

        // Assert
        value.Value.Should().Be(1L);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Ticks.CreateLogixValue(9223372036854775806L);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the eight bytes a LINT occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F);
    }

    [Fact]
    public void Encode_WritesANegativeValueInTwosComplement()
    {
        // Arrange
        var value = Ticks.CreateLogixValue(-1L);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
    }
}
