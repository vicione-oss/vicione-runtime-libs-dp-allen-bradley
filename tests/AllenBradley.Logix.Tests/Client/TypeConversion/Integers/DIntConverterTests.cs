using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>DINT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Four bytes, signed, least significant
/// first.
/// </summary>
public class DIntConverterTests
{
    private static readonly IDataPointConverter Converter = new DIntConverter();

    private static readonly DIntDataPoint Counter = new(new TagName("dintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagADint()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the one thing about this
        // converter a round trip cannot catch: decoding four bytes off another type succeeds and is wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Dint);
    }

    [Fact]
    public void Decode_ReadsASignedIntegerLittleEndian()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0x00, 0x00];

        // Act
        var value = Converter.Decode(Counter, buffer);

        // Assert
        value.Value.Should().Be(4711);
    }

    [Theory]
    [InlineData(0x00000000u, 0)]
    [InlineData(0xFFFFFFFFu, -1)]
    [InlineData(0x7FFFFFFFu, int.MaxValue)]
    [InlineData(0x80000000u, int.MinValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(uint bits, int expected)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(bits);

        // Act
        var value = Converter.Decode(Counter, buffer);

        // Assert
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Counter.CreateLogixValue(4711);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the four bytes a DINT occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0x67, 0x12, 0x00, 0x00);
    }

    [Fact]
    public void Encode_WritesANegativeValueInTwosComplement()
    {
        // Arrange
        var value = Counter.CreateLogixValue(-1);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF);
    }
}
