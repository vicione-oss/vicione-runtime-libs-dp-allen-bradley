using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// The values above the signed maximum carry this suite: an unsigned type shares its width and its bytes
/// with the signed twin beside it, so every value up to that maximum round-trips identically through the
/// wrong codec.
/// </summary>
public sealed class UnsignedIntegerIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)42)]
    [InlineData((byte)128)]
    [InlineData((byte)200)]
    [InlineData(byte.MaxValue)]
    public async Task AUSIntValueRoundTripsAndItsTagIsDeclaredUSInt(byte valueToWrite)
    {
        // Arrange
        var dataPoint = new USIntDataPoint(TagPath.Parse(TagAddresses.USInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicScalar(TagAddresses.USInt, AllenBradleyDataType.Usint);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData((ushort)0)]
    [InlineData((ushort)4242)]
    [InlineData((ushort)32768)]
    [InlineData((ushort)50000)]
    [InlineData(ushort.MaxValue)]
    public async Task AUIntValueRoundTripsAndItsTagIsDeclaredUInt(ushort valueToWrite)
    {
        // Arrange
        var dataPoint = new UIntDataPoint(TagPath.Parse(TagAddresses.UInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicScalar(TagAddresses.UInt, AllenBradleyDataType.Uint);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(123456u)]
    [InlineData(2147483648u)]
    [InlineData(4000000000u)]
    [InlineData(uint.MaxValue)]
    public async Task AUDIntValueRoundTripsAndItsTagIsDeclaredUDInt(uint valueToWrite)
    {
        // Arrange
        var dataPoint = new UDIntDataPoint(TagPath.Parse(TagAddresses.UDInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicScalar(TagAddresses.UDInt, AllenBradleyDataType.Udint);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0ul)]
    [InlineData(1234567890123ul)]
    [InlineData(9223372036854775808ul)]
    [InlineData(18446744073709551614ul)]
    [InlineData(ulong.MaxValue)]
    public async Task AULIntValueRoundTripsAndItsTagIsDeclaredULInt(ulong valueToWrite)
    {
        // Arrange
        var dataPoint = new ULIntDataPoint(TagPath.Parse(TagAddresses.ULInt), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicScalar(TagAddresses.ULInt, AllenBradleyDataType.Ulint);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }
}
