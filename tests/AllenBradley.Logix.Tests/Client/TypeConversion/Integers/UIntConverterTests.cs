using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class UIntConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new UIntConverter();

    private static readonly UIntDataPoint Setpoint = new(TagPath.Parse("uintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void TwoStoredBytesDecodeToTheUnsignedShortTheyHoldLeastSignificantFirst()
    {
        // Arrange
        // Asymmetric on purpose: a swapped read of these two bytes is 0x3412, not 0x1234.
        byte[] buffer = [0x34, 0x12];

        // Act
        var decoded = Converter.Decode(Setpoint, buffer);

        // Assert
        decoded.Should().Be(Setpoint.CreateLogixValue(0x1234));
    }

    [Fact]
    public void AUIntEncodesToExactlyTheTwoBytesTheTypeOccupies()
    {
        // Arrange
        var value = Setpoint.CreateLogixValue(0x1234);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x34, 0x12);
    }
}
