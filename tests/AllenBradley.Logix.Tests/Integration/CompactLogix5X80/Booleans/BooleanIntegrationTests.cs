using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Booleans;

/// <summary>
/// Both states, because a <c>BOOL</c> that reads <c>true</c> whatever was written passes a one-value
/// test.
/// </summary>
public sealed class BooleanIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ABoolValueRoundTripsAndItsTagIsDeclaredBool(bool valueToWrite)
    {
        // Arrange
        var dataPoint = new BoolDataPoint(TagPath.Parse(TagAddresses.Bool), DefaultPollFrequency, NoChannels);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expectedDefinition = ExpectedTagDefinitions.AtomicScalar(TagAddresses.Bool, AllenBradleyDataType.Bool);
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(dataPoint, expectedDefinition),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }
}
