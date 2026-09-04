using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

/// <summary>
/// The <c>LREAL</c> codec, against byte patterns spelled out rather than produced by the encoder — which
/// would make a decode test agree with itself by construction. Eight bytes, IEEE-754 double, least
/// significant first.
/// </summary>
public class LRealConverterTests
{
    private static readonly IDataPointConverter Converter = new LRealConverter();

    private static readonly LRealDataPoint Measurement = new(new TagName("PrecisionValue"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void Decode_ReadsAnIeee754DoubleLittleEndian()
    {
        // Arrange
        byte[] buffer = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F];

        // Act
        var value = Converter.Decode(Measurement, buffer);

        // Assert
        value.Value.Should().Be(1.0d);
    }

    [Theory]
    [InlineData(0x3FF0000000000000, 1.0d)]
    [InlineData(0xBFF0000000000000, -1.0d)]
    [InlineData(0x0000000000000000, 0.0d)]
    [InlineData(0x400921FB54442D18, Math.PI)]
    public void Decode_ReadsTheBitPatternTheControllerStored(ulong bits, double expected)
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
        var buffer = new byte[sizeof(double)];
        var value = Measurement.CreateLogixValue(Math.PI);

        // Act
        Converter.Encode(value, buffer);

        // Assert
        buffer.Should().Equal(0x18, 0x2D, 0x44, 0x54, 0xFB, 0x21, 0x09, 0x40);
    }
}
