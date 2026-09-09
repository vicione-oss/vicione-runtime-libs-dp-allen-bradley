using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

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
        var dataPoint = new BoolDataPoint(new TagName(TagAddresses.Bool), DefaultPollFrequency, NoChannels);

        // Act
        // Assert
        await AssertRoundTripAsync(
            dataPoint,
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.Bool, AllenBradleyDataType.Bool));
    }
}
