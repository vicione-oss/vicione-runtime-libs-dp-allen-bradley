using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.FloatingPoints;

public sealed class RealConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new RealConverter();

    private static readonly RealDataPoint Measurement =
        new(TagPath.Parse("realValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void FourStoredBytesDecodeToTheIeee754SingleTheyHoldLeastSignificantFirst()
    {
        // Arrange
        byte[] buffer = [0x00, 0x00, 0x80, 0x3F];

        // Act
        var decoded = Converter.Decode(Measurement, buffer);

        // Assert
        decoded.Should().Be(Measurement.CreateLogixValue(1.0f));
    }

    [Fact]
    public void ARealEncodesToExactlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        var value = Measurement.CreateLogixValue(MathF.PI);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xDB, 0x0F, 0x49, 0x40);
    }
}
