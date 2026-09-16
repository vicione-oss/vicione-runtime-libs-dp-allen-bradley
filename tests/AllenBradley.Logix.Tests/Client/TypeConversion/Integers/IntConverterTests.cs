using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class IntConverterTests
{
    private static readonly IDataPointConverter Converter = new IntConverter();

    private static readonly IntDataPoint Counter = new(new TagAddress("intValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagAnInt()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Int);
    }

    [Fact]
    public void TwoStoredBytesDecodeToTheSignedShortTheyHoldLeastSignificantFirst()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12];

        // Act
        var decoded = Converter.Decode(Counter, buffer);

        // Assert
        decoded.Should().Be(Counter.CreateLogixValue(4711));
    }

    [Theory]
    [InlineData(0x0000, (short)0)]
    [InlineData(0xFFFF, (short)-1)]
    [InlineData(0x7FFF, short.MaxValue)]
    [InlineData(0x8000, short.MinValue)]
    public void AStoredBitPatternDecodesToTheIntTheControllerMeansByIt(ushort storedBits, short expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Counter, buffer);

        // Assert
        decoded.Should().Be(Counter.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADecodeReadsOnlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Counter, buffer);

        // Assert
        decoded.Should().Be(Counter.CreateLogixValue(4711));
    }

    [Fact]
    public void AnIntEncodesToExactlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        var value = Counter.CreateLogixValue(4711);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x67, 0x12);
    }

    [Fact]
    public void ANegativeIntEncodesInTwosComplement()
    {
        // Arrange
        var value = Counter.CreateLogixValue(-1);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF);
    }
}
