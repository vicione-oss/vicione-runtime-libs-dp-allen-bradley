using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.FloatingPoints;

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
        var dataPoint = new RealDataPoint(TagPath.Parse(TagAddresses.Real), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicScalar(TagAddresses.Real, AllenBradleyDataType.Real);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
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
        // Math.PI is not representable in a REAL, so it catches an LREAL encoded through the narrower codec.
        var dataPoint = new LRealDataPoint(TagPath.Parse(TagAddresses.LReal), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicScalar(TagAddresses.LReal, AllenBradleyDataType.Lreal);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }
}
