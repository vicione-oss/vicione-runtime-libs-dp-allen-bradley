using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Booleans;

public sealed class BoolConverterTests
{
    private static readonly IDataPointConverter Converter = new BoolConverter();

    private static readonly BoolDataPoint Flag = new(new TagAddress("boolValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TheConverterExpectsTheControllerToDeclareTheTagABool()
    {
        // Arrange

        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        expectedDataType.Should().Be(AllenBradleyDataType.Bool);
    }

    [Theory]
    [InlineData(0xFF)]
    [InlineData(0x01)]
    [InlineData(0x80)]
    public void AnyNonzeroStoredByteDecodesToTrue(byte storedBits)
    {
        // Arrange
        // The controller stores 0xFF for a set BOOL, but a masked member can leave any nonzero pattern.
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
