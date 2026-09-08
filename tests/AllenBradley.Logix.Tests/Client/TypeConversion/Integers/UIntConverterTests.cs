using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>UINT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Two bytes, unsigned, least significant
/// first.
/// </summary>
public class UIntConverterTests
{
    private static readonly IDataPointConverter Converter = new UIntConverter();

    private static readonly UIntDataPoint Setpoint = new(new TagName("uintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagAUInt()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the whole of what tells this
        // converter from the INT one: the two occupy the same two bytes and disagree only about what
        // the top bit means, so an INT tag read through here hands back 65535 where it holds -1.
        expectedDataType.Should().Be(AllenBradleyDataType.Uint);
    }

    [Fact]
    public void Decode_ReadsTheTwoBytesLeastSignificantFirst()
    {
        // Arrange
        // Asymmetric on purpose: a swapped read of these two bytes is 0x3412, not 0x1234.
        byte[] buffer = [0x34, 0x12];

        // Act
        var value = Converter.Decode(Setpoint, buffer);

        // Assert
        value.Value.Should().Be((ushort)0x1234);
    }

    [Theory]
    [InlineData(0x00, 0x00, (ushort)0)]
    [InlineData(0x92, 0x10, (ushort)4242)]
    [InlineData(0xFF, 0x7F, (ushort)32767)]
    [InlineData(0x00, 0x80, (ushort)32768)]
    [InlineData(0xFF, 0xFF, ushort.MaxValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(byte low, byte high, ushort expected)
    {
        // Arrange
        byte[] buffer = [low, high];

        // Act
        var value = Converter.Decode(Setpoint, buffer);

        // Assert
        // The top half of the range is the point: 0x8000 and 0xFFFF are where an unsigned decode parts
        // company with the signed one, which reads them as -32768 and -1.
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Decode_ReadsOnlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x92, 0x10, 0xFF, 0xFF];

        // Act
        var value = Converter.Decode(Setpoint, buffer);

        // Assert
        value.Value.Should().Be((ushort)4242);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Setpoint.CreateLogixValue(0x1234);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the two bytes a UINT occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0x34, 0x12);
    }

    [Fact]
    public void Encode_WritesAValueAboveTheSignedRangeAsItStands()
    {
        // Arrange
        var value = Setpoint.CreateLogixValue(ushort.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF);
    }
}
