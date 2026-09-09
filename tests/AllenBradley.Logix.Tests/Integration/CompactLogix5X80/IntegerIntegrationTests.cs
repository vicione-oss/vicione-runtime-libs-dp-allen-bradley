using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Each type is driven to both ends of its range as well as through the middle, because the width is
/// the whole of what distinguishes these four and a decode that reads the right bytes as the wrong
/// width agrees with a narrow test value.
/// </summary>
public sealed class IntegerIntegrationTests(ITestOutputHelper output) : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)42)]
    [InlineData((sbyte)-1)]
    [InlineData(sbyte.MinValue)]
    [InlineData(sbyte.MaxValue)]
    public async Task ASIntValueRoundTripsAndItsTagIsDeclaredSInt(sbyte valueToWrite)
    {
        // Arrange
        var dataPoint = new SIntDataPoint(new TagName(TagAddresses.SInt), DefaultPollFrequency, NoChannels);

        // Act
        // Assert
        await AssertRoundTripAsync(
            dataPoint,
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.SInt, AllenBradleyDataType.Sint));
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)4242)]
    [InlineData((short)-1)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    public async Task AnIntValueRoundTripsAndItsTagIsDeclaredInt(short valueToWrite)
    {
        // Arrange
        var dataPoint = new IntDataPoint(new TagName(TagAddresses.Int), DefaultPollFrequency, NoChannels);

        // Act
        // Assert
        await AssertRoundTripAsync(
            dataPoint,
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.Int, AllenBradleyDataType.Int));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(123456)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task ADIntValueRoundTripsAndItsTagIsDeclaredDInt(int valueToWrite)
    {
        // Arrange
        var dataPoint = new DIntDataPoint(new TagName(TagAddresses.DInt), DefaultPollFrequency, NoChannels);

        // Act
        // Assert
        await AssertRoundTripAsync(
            dataPoint,
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.DInt, AllenBradleyDataType.Dint));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1234567890123L)]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public async Task ALIntValueRoundTripsAndItsTagIsDeclaredLInt(long valueToWrite)
    {
        // Arrange
        var dataPoint = new LIntDataPoint(new TagName(TagAddresses.LInt), DefaultPollFrequency, NoChannels);

        // Act
        // Assert
        await AssertRoundTripAsync(
            dataPoint,
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.LInt, AllenBradleyDataType.Lint));
    }
}
