using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class LIntConverterTests
{
    // One below long.MaxValue, so a swapped byte order cannot pass by symmetry.
    private const long AlmostMaxValue = 9223372036854775806L;

    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new LIntConverter();

    private static readonly LIntDataPoint Ticks = new(TagPath.Parse("lintValue1"), DefaultPollFrequency, NoChannels);

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

    [Fact]
    public void ALIntEncodesToExactlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        var value = Ticks.CreateLogixValue(AlmostMaxValue);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F);
    }
}
