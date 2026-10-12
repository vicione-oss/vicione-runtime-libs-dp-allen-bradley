using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class IntConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new IntConverter();

    private static readonly IntDataPoint Counter = new(TagPath.Parse("intValue1"), DefaultPollFrequency, NoChannels);

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
}
