using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

/// <summary>
/// The <c>REAL</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Four bytes, IEEE-754 single, least
/// significant first.
/// </summary>
public class RealConverterTests
{
    private static readonly IDataPointConverter Converter = new RealConverter();

    private static readonly RealDataPoint Measurement = new(new TagName("realValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagAReal()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the one thing about this
        // converter a round trip cannot catch: decoding four bytes off a DINT succeeds and is wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Real);
    }

    [Fact]
    public void Decode_ReadsAnIeee754SingleLittleEndian()
    {
        // Arrange
        byte[] buffer = [0x00, 0x00, 0x80, 0x3F];

        // Act
        var value = Converter.Decode(Measurement, buffer);

        // Assert
        value.Value.Should().Be(1.0f);
    }

    [Theory]
    [InlineData(0x3F800000u, 1.0f)]
    [InlineData(0xBF800000u, -1.0f)]
    [InlineData(0x00000000u, 0.0f)]
    [InlineData(0x40490FDBu, MathF.PI)]
    public void Decode_ReadsTheBitPatternTheControllerStored(uint bits, float expected)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(bits);

        // Act
        var value = Converter.Decode(Measurement, buffer);

        // Assert
        value.Value.Should().Be(expected);
    }

    [Fact]
    public void Encode_WritesBackWhatDecodeReads()
    {
        // Arrange
        var value = Measurement.CreateLogixValue(MathF.PI);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xDB, 0x0F, 0x49, 0x40);
    }
}
