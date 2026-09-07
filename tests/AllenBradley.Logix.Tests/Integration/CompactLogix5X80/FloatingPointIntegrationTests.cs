using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Write and read back the floating-point types on the CompactLogix 5X80, through the production client
/// stack: <c>REAL</c> and <c>LREAL</c>.
/// </summary>
/// <remarks>
/// Compared for exact equality, deliberately. A controller stores an IEEE-754 value as the bytes it was
/// given and hands the same bytes back, so a round trip that loses a bit is a defect in the codec and
/// not the rounding a tolerance would be there to absorb. The values include both ends of each range
/// and a fraction that is not representable in the narrower type, which is what catches a
/// <c>LREAL</c> that went out through the <c>REAL</c> codec.
/// <para>
/// <c>LREAL</c> is the reason this folder is pinned to a 5X80: the 5X70 controllers have no such type,
/// so on one of those this suite addresses a type the controller cannot resolve. See
/// <c>docs/AllenBradley.Logix.Documentation/reference/datatype-support.md</c>.
/// </para>
/// </remarks>
public sealed class FloatingPointIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData(0f)]
    [InlineData(3.14159f)]
    [InlineData(-1.5f)]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    public async Task WriteAndReadBack_RealValue_RoundTripsAndTheTagIsDeclaredReal(float valueToWrite) =>
        await AssertRoundTripAsync(
            new RealDataPoint(new TagName(TagAddresses.Real), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.Real, AllenBradleyDataType.Real));

    [Theory]
    [InlineData(0d)]
    [InlineData(Math.PI)]
    [InlineData(-2.5d)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public async Task WriteAndReadBack_LRealValue_RoundTripsAndTheTagIsDeclaredLReal(double valueToWrite) =>
        await AssertRoundTripAsync(
            new LRealDataPoint(new TagName(TagAddresses.LReal), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.LReal, AllenBradleyDataType.Lreal));
}
