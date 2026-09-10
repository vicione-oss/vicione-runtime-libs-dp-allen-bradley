using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

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
        expectedDataType.Should().Be(AllenBradleyDataType.Ulint);
    }

    [Fact]
    public void EightStoredBytesDecodeToTheUnsignedLongTheyHoldLeastSignificantFirst()
    {
        // Arrange
        // Every byte differs, so a swapped or rotated read cannot land on the same value.
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
