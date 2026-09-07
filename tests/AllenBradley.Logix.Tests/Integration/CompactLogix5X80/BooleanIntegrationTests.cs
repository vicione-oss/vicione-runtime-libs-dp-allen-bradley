using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Write and read back the <c>BOOL</c> tag on the CompactLogix 5X80, through the production client
/// stack. Both states, because a <c>BOOL</c> that reads <c>true</c> whatever was written passes a
/// one-value test.
/// </summary>
public sealed class BooleanIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WriteAndReadBack_BoolValue_RoundTripsAndTheTagIsDeclaredBool(bool valueToWrite) =>
        await AssertRoundTripAsync(
            new BoolDataPoint(new TagName(TagAddresses.Bool), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.Bool, AllenBradleyDataType.Bool));
}
