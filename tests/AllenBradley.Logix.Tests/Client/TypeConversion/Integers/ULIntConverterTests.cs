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
public sealed class ULIntConverterTests
{
    private static readonly IDataPointConverter Converter = new ULIntConverter();

    private static readonly ULIntDataPoint Cycles = new(new TagName("ulintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagAULInt()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // The whole of what tells this converter from the LINT one: the two occupy the same eight bytes
        // and disagree only about what the top bit means.
        expectedDataType.Should().Be(AllenBradleyDataType.Ulint);
    }

    [Fact]
    public void EightStoredBytesDecodeToTheUnsignedLongTheyHoldLeastSignificantFirst()
    {
        // Arrange
        // Every byte differs, so a swapped or rotated read cannot land on the same value. This is the
        // widest type the port has, and the one where a byte-order mistake has the most room to hide.
        byte[] buffer = [0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12];

        // Act
        var decoded = Converter.Decode(Cycles, buffer);

        // Assert
        decoded.Should().Be(Cycles.CreateLogixValue(0x123456789ABCDEF0ul));
    }

    [Theory]
    [InlineData(0x0000000000000000ul, 0ul)]
    [InlineData(0x7FFFFFFFFFFFFFFFul, 9223372036854775807ul)]
    [InlineData(0x8000000000000000ul, 9223372036854775808ul)]
    [InlineData(0xFFFFFFFFFFFFFFFEul, 18446744073709551614ul)]
    [InlineData(0xFFFFFFFFFFFFFFFFul, ulong.MaxValue)]
    public void AStoredBitPatternDecodesToTheULIntTheControllerMeansByIt(ulong storedBits, ulong expectedValue)
    {
        // Arrange
        // The top half of the range is the point: a signed decode reads 0x8000000000000000 and above as
        // long.MinValue upwards to -1.
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Cycles, buffer);

        // Assert
        decoded.Should().Be(Cycles.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADecodeReadsOnlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Cycles, buffer);

        // Assert
        decoded.Should().Be(Cycles.CreateLogixValue(42ul));
    }

    [Fact]
    public void AULIntEncodesToExactlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        var value = Cycles.CreateLogixValue(0x123456789ABCDEF0ul);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // The batch copies these into the tag's buffer, so a longer array would be a wider tag than the
        // type declares.
        bytes.Should().Equal(0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12);
    }

    [Fact]
    public void AULIntAboveTheSignedRangeEncodesAsItStands()
    {
        // Arrange
        var value = Cycles.CreateLogixValue(ulong.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
    }
}
