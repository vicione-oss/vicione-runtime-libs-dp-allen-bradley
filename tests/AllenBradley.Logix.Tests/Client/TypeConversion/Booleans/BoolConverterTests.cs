using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Booleans;

/// <summary>
/// The byte patterns are spelled out rather than produced by the encoder, which would make a decode test
/// agree with itself by construction.
/// </summary>
public sealed class BoolConverterTests
{
    private static readonly IDataPointConverter Converter = new BoolConverter();

    private static readonly BoolDataPoint Flag = new(new TagName("boolValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagABool()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // The one thing a round trip cannot catch: reading a byte off a SINT succeeds and is wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Bool);
    }

    [Theory]
    [InlineData(0xFF)]
    [InlineData(0x01)]
    [InlineData(0x80)]
    public void AnyNonzeroStoredByteDecodesToTrue(byte storedBits)
    {
        // Arrange
        // 0xFF is what the controller stores for a set BOOL, but a member set through a mask can leave any
        // nonzero pattern behind.
        byte[] buffer = [storedBits];

        // Act
        var decoded = Converter.Decode(Flag, buffer);

        // Assert
        decoded.Should().Be(Flag.CreateLogixValue(true));
    }

    [Fact]
    public void AZeroStoredByteDecodesToFalse()
    {
        // Arrange
        byte[] buffer = [0x00];

        // Act
        var decoded = Converter.Decode(Flag, buffer);

        // Assert
        decoded.Should().Be(Flag.CreateLogixValue(false));
    }

    [Fact]
    public void ADecodeReadsOnlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x00, 0xFF, 0xFF, 0xFF];

        // Act
        var decoded = Converter.Decode(Flag, buffer);

        // Assert
        decoded.Should().Be(Flag.CreateLogixValue(false));
    }

    [Fact]
    public void TrueEncodesToTheByteStudio5000Shows()
    {
        // Arrange
        var value = Flag.CreateLogixValue(true);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // The batch copies these into the tag's buffer, so a longer array would be a wider tag than the
        // type declares.
        bytes.Should().Equal(0xFF);
    }

    [Fact]
    public void FalseEncodesToZero()
    {
        // Arrange
        var value = Flag.CreateLogixValue(false);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x00);
    }
}
