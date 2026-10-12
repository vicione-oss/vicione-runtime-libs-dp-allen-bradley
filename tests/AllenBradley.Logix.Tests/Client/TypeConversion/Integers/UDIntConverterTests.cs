using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class UDIntConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new UDIntConverter();

    private static readonly UDIntDataPoint Runtime = new(TagPath.Parse("udintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void FourStoredBytesDecodeToTheUnsignedIntegerTheyHoldLeastSignificantFirst()
    {
        // Arrange
        // Every byte differs, so a swapped or rotated read cannot land on the same value.
        byte[] buffer = [0x78, 0x56, 0x34, 0x12];

        // Act
        var decoded = Converter.Decode(Runtime, buffer);

        // Assert
        decoded.Should().Be(Runtime.CreateLogixValue(0x12345678u));
    }

    [Fact]
    public void AUDIntEncodesToExactlyTheFourBytesTheTypeOccupies()
    {
        // Arrange
        var value = Runtime.CreateLogixValue(0x12345678u);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x78, 0x56, 0x34, 0x12);
    }
}
