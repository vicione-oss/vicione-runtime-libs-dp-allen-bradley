using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class DIntConverterTests
{
    private static readonly IDataPointConverter Converter = new DIntConverter();

    private static readonly DIntDataPoint Counter = new(TagPath.Parse("dintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void FourStoredBytesDecodeToTheSignedIntegerTheyHoldLeastSignificantFirst()
    {
        // Arrange
        byte[] buffer = [0x67, 0x12, 0x00, 0x00];

        // Act
        var decoded = Converter.Decode(Counter, buffer);

        // Assert
        decoded.Should().Be(Counter.CreateLogixValue(4711));
    }

    [Theory]
    [InlineData(0x00000000u, 0)]
    [InlineData(0xFFFFFFFFu, -1)]
    [InlineData(0x7FFFFFFFu, int.MaxValue)]
    [InlineData(0x80000000u, int.MinValue)]
    public void AStoredBitPatternDecodesToTheDintTheControllerMeansByIt(uint storedBits, int expectedValue)
    {
        // Arrange
        var buffer = BitConverter.GetBytes(storedBits);

        // Act
        var decoded = Converter.Decode(Counter, buffer);

        // Assert
        decoded.Should().Be(Counter.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADintEncodesToExactlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        var value = Counter.CreateLogixValue(4711);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x67, 0x12, 0x00, 0x00);
    }

    [Fact]
    public void ANegativeDintEncodesInTwosComplement()
    {
        // Arrange
        var value = Counter.CreateLogixValue(-1);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF, 0xFF, 0xFF, 0xFF);
    }
}
