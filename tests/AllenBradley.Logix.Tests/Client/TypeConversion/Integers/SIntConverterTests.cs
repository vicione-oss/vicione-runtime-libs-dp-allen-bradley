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
public sealed class SIntConverterTests
{
    private static readonly IDataPointConverter Converter = new SIntConverter();

    private static readonly SIntDataPoint Level = new(new TagName("sintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagASInt()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // The one thing a round trip cannot catch: decoding one byte off an INT succeeds and is wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Sint);
    }

    [Theory]
    [InlineData(0x00, (sbyte)0)]
    [InlineData(0x2A, (sbyte)42)]
    [InlineData(0xFF, (sbyte)-1)]
    [InlineData(0x7F, sbyte.MaxValue)]
    [InlineData(0x80, sbyte.MinValue)]
    public void AStoredBitPatternDecodesToTheSIntTheControllerMeansByIt(byte storedBits, sbyte expectedValue)
    {
        // Arrange
        byte[] buffer = [storedBits];

        // Act
        var decoded = Converter.Decode(Level, buffer);

        // Assert
        decoded.Should().Be(Level.CreateLogixValue(expectedValue));
    }

    [Fact]
    public void ADecodeReadsOnlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x2A, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Level, buffer);

        // Assert
        decoded.Should().Be(Level.CreateLogixValue(42));
    }

    [Fact]
    public void ASIntEncodesToExactlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        var value = Level.CreateLogixValue(42);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // The batch copies these into the tag's buffer, so a longer array would be a wider tag than the
        // type declares.
        bytes.Should().Equal(0x2A);
    }

    [Fact]
    public void ANegativeSIntEncodesInTwosComplement()
    {
        // Arrange
        var value = Level.CreateLogixValue(-1);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFF);
    }
}
