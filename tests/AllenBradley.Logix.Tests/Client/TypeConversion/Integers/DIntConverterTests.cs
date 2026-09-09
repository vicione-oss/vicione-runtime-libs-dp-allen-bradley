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
public sealed class DIntConverterTests
{
    private static readonly IDataPointConverter Converter = new DIntConverter();

    private static readonly DIntDataPoint Counter = new(new TagName("dintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagADint()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // The one thing a round trip cannot catch: decoding four bytes off another type succeeds and is
        // wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Dint);
    }

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
        // The batch copies these into the tag's buffer, so a longer array would be a wider tag than the
        // type declares.
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
