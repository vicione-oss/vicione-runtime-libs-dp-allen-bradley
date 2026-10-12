using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Integers;

public sealed class ULIntConverterTests
{
    private static readonly IDataPointConverter<ILogixDataPoint, ILogixDataPointValue> Converter = new ULIntConverter();

    private static readonly ULIntDataPoint Cycles = new(TagPath.Parse("ulintValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void EightStoredBytesDecodeToTheUnsignedLongTheyHoldLeastSignificantFirst()
    {
        // Arrange
        // Every byte differs, so a swapped or rotated read cannot land on the same value.
        byte[] buffer = [0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12];

        // Act
        var decoded = Converter.Decode(Cycles, buffer);

        // Assert
        decoded.Should().Be(Cycles.CreateLogixValue(0x123456789ABCDEF0ul));
    }

    [Fact]
    public void AULIntEncodesToExactlyTheEightBytesTheTypeOccupies()
    {
        // Arrange
        var value = Cycles.CreateLogixValue(0x123456789ABCDEF0ul);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12);
    }
}
