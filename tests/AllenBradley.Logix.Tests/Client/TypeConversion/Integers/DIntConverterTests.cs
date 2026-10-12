using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class DIntConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new DIntConverter();

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
}
