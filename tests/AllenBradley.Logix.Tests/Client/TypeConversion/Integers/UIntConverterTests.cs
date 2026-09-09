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
public sealed class UIntConverterTests
{
    private static readonly IDataPointConverter Converter = new UIntConverter();

    private static readonly UIntDataPoint Setpoint = new(new TagName("uintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagAUInt()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // The whole of what tells this converter from the INT one: the two occupy the same two bytes and
        // disagree only about what the top bit means.
        expectedDataType.Should().Be(AllenBradleyDataType.Uint);
    }

    [Fact]
    public void TwoStoredBytesDecodeToTheUnsignedShortTheyHoldLeastSignificantFirst()
    {
        // Arrange
        // Asymmetric on purpose: a swapped read of these two bytes is 0x3412, not 0x1234.
        byte[] buffer = [0x34, 0x12];

        // Act
        var decoded = Converter.Decode(Setpoint, buffer);

        // Assert
        decoded.Should().Be(Setpoint.CreateLogixValue(0x1234));
    }

    [Theory]
    [InlineData(0x0000, (ushort)0)]
    [InlineData(0x1092, (ushort)4242)]
    [InlineData(0x7FFF, (ushort)32767)]
    [InlineData(0x8000, (ushort)32768)]
    [InlineData(0xFFFF, ushort.MaxValue)]
    public void AStoredBitPatternDecodesToTheUIntTheControllerMeansByIt(ushort storedBits, ushort expectedValue)
    {
        // Arrange
        // The top half of the range is the point: a signed decode reads 0x8000 and 0xFFFF as -32768 and -1.
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Setpoint, buffer);

        // Assert
        decoded.Should().Be(Setpoint.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADecodeReadsOnlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x92, 0x10, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Setpoint, buffer);

        // Assert
        decoded.Should().Be(Setpoint.CreateLogixValue(4242));
    }

    [Fact]
    public void AUIntEncodesToExactlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        var value = Setpoint.CreateLogixValue(0x1234);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // The batch copies these into the tag's buffer, so a longer array would be a wider tag than the
        // type declares.
        bytes.Should().Equal(0x34, 0x12);
    }

    [Fact]
    public void AUIntAboveTheSignedRangeEncodesAsItStands()
    {
        // Arrange
        var value = Setpoint.CreateLogixValue(ushort.MaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF);
    }
}
