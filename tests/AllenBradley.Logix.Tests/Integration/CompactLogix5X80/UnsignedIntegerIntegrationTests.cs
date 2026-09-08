using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Write and read back each unsigned integer type on the CompactLogix 5X80, through the production
/// client stack. <c>USINT</c> so far; <c>UINT</c>, <c>UDINT</c> and <c>ULINT</c> follow with their own
/// slices.
/// </summary>
/// <remarks>
/// A sibling of <see cref="IntegerIntegrationTests"/> rather than a section of it, because these types
/// are not merely four more widths: a 5X70 controller has no unsigned type at all, so this suite could
/// not run against the bench L32E even if the folder were not already pinned to a 5X80 by
/// <c>LREAL</c>. See <c>docs/AllenBradley.Logix.Documentation/reference/datatype-support.md</c>.
/// <para>
/// Each type is driven to both ends of its range and through the middle, and the values above the
/// signed maximum carry the suite: an unsigned type shares its width and its bytes with the signed twin
/// beside it, so every value up to that maximum round-trips identically through the wrong codec. Only
/// the top half of the range tells them apart.
/// </para>
/// </remarks>
public sealed class UnsignedIntegerIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)42)]
    [InlineData((byte)128)]
    [InlineData((byte)200)]
    [InlineData(byte.MaxValue)]
    public async Task WriteAndReadBack_USIntValue_RoundTripsAndTheTagIsDeclaredUSInt(byte valueToWrite) =>
        await AssertRoundTripAsync(
            new USIntDataPoint(new TagName(TagAddresses.USInt), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.USInt, AllenBradleyDataType.Usint));
}
