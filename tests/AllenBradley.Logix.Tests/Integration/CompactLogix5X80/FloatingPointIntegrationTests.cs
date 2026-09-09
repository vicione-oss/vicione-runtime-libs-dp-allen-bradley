using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Compared for exact equality, deliberately: a controller hands back the bytes it was given, so a round
/// trip that loses a bit is a defect in the codec rather than the rounding a tolerance would absorb.
/// </summary>
public sealed class FloatingPointIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData(0f)]
    [InlineData(3.14159f)]
    [InlineData(-1.5f)]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    public async Task ARealValueRoundTripsAndItsTagIsDeclaredReal(float valueToWrite)
    {
        // Arrange
        var dataPoint = new RealDataPoint(new TagName(TagAddresses.Real), DefaultPollFrequency, NoChannels);

        // Act
        // Assert
        await AssertRoundTripAsync(
            dataPoint,
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.Real, AllenBradleyDataType.Real));
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(Math.PI)]
    [InlineData(-2.5d)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public async Task ALRealValueRoundTripsAndItsTagIsDeclaredLReal(double valueToWrite)
    {
        // Arrange
        // A fraction not representable in the narrower type is what catches an LREAL that went out
        // through the REAL codec.
        var dataPoint = new LRealDataPoint(new TagName(TagAddresses.LReal), DefaultPollFrequency, NoChannels);

        // Act
        // Assert
        await AssertRoundTripAsync(
            dataPoint,
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.LReal, AllenBradleyDataType.Lreal));
    }
}
