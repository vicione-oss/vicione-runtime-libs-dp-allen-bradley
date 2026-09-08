using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The <c>USINT</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. One byte, unsigned, and no byte order to
/// get wrong.
/// </summary>
public class USIntConverterTests
{
    private static readonly IDataPointConverter Converter = new USIntConverter();

    private static readonly USIntDataPoint Level = new(new TagName("usintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagAUSInt()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the whole of what tells this
        // converter from the SINT one: the two occupy the same byte and disagree only about what the
        // top bit means, so a SINT tag read through here hands back 255 where the controller holds -1.
        expectedDataType.Should().Be(AllenBradleyDataType.Usint);
    }

    [Theory]
    [InlineData(0x00, (byte)0)]
    [InlineData(0x2A, (byte)42)]
    [InlineData(0x7F, (byte)127)]
    [InlineData(0x80, (byte)128)]
    [InlineData(0xFF, byte.MaxValue)]
    public void Decode_ReadsTheBitPatternTheControllerStored(byte bits, byte expected)
    {
        // Arrange
        byte[] buffer = [bits];

        // Act
        var value = Converter.Decode(Level, buffer);

        // Assert
        // The top half of the range is the point: 0x80 and 0xFF are where an unsigned decode parts
        // company with the signed one, which reads them as -128 and -1.
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
        value.Value.Should().Be((byte)42);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Level.CreateLogixValue(42);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the one byte a USINT occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0x2A);
    }

    [Fact]
    public void Encode_WritesAValueAboveTheSignedRangeAsItStands()
    {
        // Arrange
        var value = Level.CreateLogixValue(byte.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF);
    }
}
