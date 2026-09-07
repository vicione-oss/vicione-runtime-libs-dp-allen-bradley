using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>INT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Two bytes, signed, least significant
/// first.
/// </summary>
public class IntConverterTests
{
    private static readonly IDataPointConverter Converter = new IntConverter();

    private static readonly IntDataPoint Counter = new(new TagName("intValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagAnInt()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the one thing about this
        // converter a round trip cannot catch: decoding two bytes off a DINT succeeds and is wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Int);
    }

    [Fact]
    public void Decode_ReadsASignedShortLittleEndian()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12];

        // Act
        var value = Converter.Decode(Counter, buffer);

        // Assert
        value.Value.Should().Be((short)4711);
    }

    [Theory]
    [InlineData(0x0000, (short)0)]
    [InlineData(0xFFFF, (short)-1)]
    [InlineData(0x7FFF, short.MaxValue)]
    [InlineData(0x8000, short.MinValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(ushort bits, short expected)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(bits);

        // Act
        var value = Converter.Decode(Counter, buffer);

        // Assert
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Decode_ReadsOnlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0xFF, 0xFF];

        // Act
        var value = Converter.Decode(Counter, buffer);

        // Assert
        value.Value.Should().Be((short)4711);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var buffer = new byte[sizeof(short)];
        var value = Counter.CreateLogixValue(4711);

        // Act
        Converter.Encode(value, buffer);

        // Assert
        buffer.Should().Equal(0x67, 0x12);
    }

    [Fact]
    public void Encode_WritesANegativeValueInTwosComplement()
    {
        // Arrange
        var buffer = new byte[sizeof(short)];
        var value = Counter.CreateLogixValue(-1);

        // Act
        Converter.Encode(value, buffer);

        // Assert
        buffer.Should().Equal(0xFF, 0xFF);
    }
}
