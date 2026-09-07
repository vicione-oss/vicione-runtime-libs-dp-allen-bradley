using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>SINT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. One byte, signed, and no byte order to
/// get wrong.
/// </summary>
public class SIntConverterTests
{
    private static readonly IDataPointConverter Converter = new SIntConverter();

    private static readonly SIntDataPoint Level = new(new TagName("sintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagASInt()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the one thing about this
        // converter a round trip cannot catch: decoding one byte off an INT succeeds and is wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Sint);
    }

    [Theory]
    [InlineData(0x00, (sbyte)0)]
    [InlineData(0x2A, (sbyte)42)]
    [InlineData(0xFF, (sbyte)-1)]
    [InlineData(0x7F, sbyte.MaxValue)]
    [InlineData(0x80, sbyte.MinValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(byte bits, sbyte expected)
    {
        // Arrange
        byte[] buffer = [bits];

        // Act
        var value = Converter.Decode(Level, buffer);

        // Assert
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Decode_ReadsOnlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x2A, 0xFF, 0xFF, 0xFF];

        // Act
        var value = Converter.Decode(Level, buffer);

        // Assert
        value.Value.Should().Be((sbyte)42);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Level.CreateLogixValue(42);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the one byte a SINT occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0x2A);
    }

    [Fact]
    public void Encode_WritesANegativeValueInTwosComplement()
    {
        // Arrange
        var value = Level.CreateLogixValue(-1);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF);
    }
}
