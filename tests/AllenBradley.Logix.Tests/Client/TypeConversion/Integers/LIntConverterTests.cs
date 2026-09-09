using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

/// <summary>
/// The byte patterns are spelled out rather than produced by the encoder, which would make a decode test
/// agree with itself by construction.
/// </summary>
public sealed class LIntConverterTests
{
    // One below long.MaxValue, so every byte but the last is 0xFF and a swapped byte order cannot pass by
    // symmetry.
    private const long AlmostMaxValue = 9223372036854775806L;

    private static readonly IDataPointConverter Converter = new LIntConverter();

    private static readonly LIntDataPoint Ticks = new(new TagName("lintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagALInt()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // The one thing a round trip cannot catch: decoding eight bytes off a DINT reads past the tag.
        expectedDataType.Should().Be(AllenBradleyDataType.Lint);
    }

    [Fact]
    public void EightStoredBytesDecodeToTheSignedLongTheyHoldLeastSignificantFirst()
    {
        // Arrange
        byte[] buffer = [0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F];

        // Act
        var decoded = Converter.Decode(Ticks, buffer);

        // Assert
        decoded.Should().Be(Ticks.CreateLogixValue(AlmostMaxValue));
    }

    [Theory]
    [InlineData(0UL, 0L)]
    [InlineData(ulong.MaxValue, -1L)]
    [InlineData(0x7FFFFFFFFFFFFFFFUL, long.MaxValue)]
    [InlineData(0x8000000000000000UL, long.MinValue)]
    public void AStoredBitPatternDecodesToTheLIntTheControllerMeansByIt(ulong storedBits, long expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Ticks, buffer);

        // Assert
        decoded.Should().Be(Ticks.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADecodeReadsOnlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Ticks, buffer);

        // Assert
        decoded.Should().Be(Ticks.CreateLogixValue(1L));
    }

    [Fact]
    public void ALIntEncodesToExactlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        var value = Ticks.CreateLogixValue(AlmostMaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // The batch copies these into the tag's buffer, so a longer array would be a wider tag than the
        // type declares.
        bytes.Should().Equal(0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F);
    }

    [Fact]
    public void ANegativeLIntEncodesInTwosComplement()
    {
        // Arrange
        var value = Ticks.CreateLogixValue(-1L);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
    }
}
