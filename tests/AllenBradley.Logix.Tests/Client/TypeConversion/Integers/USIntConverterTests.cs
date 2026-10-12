using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class USIntConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new USIntConverter();

    private static readonly USIntDataPoint Level = new(TagPath.Parse("usintValue1"), DefaultPollFrequency, NoChannels);

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
    public void AUSIntEncodesToExactlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        var value = Level.CreateLogixValue(42);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x2A);
    }
}
