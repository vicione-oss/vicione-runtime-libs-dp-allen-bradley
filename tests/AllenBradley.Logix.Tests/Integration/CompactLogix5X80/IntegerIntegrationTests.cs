using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// Write and read back each signed integer type on the CompactLogix 5X80, through the production client
/// stack: <c>SINT</c>, <c>INT</c>, <c>DINT</c>, <c>LINT</c>.
/// </summary>
/// <remarks>
/// Each type is driven to both ends of its range as well as through the middle. The width is the whole
/// of what distinguishes these four, and a decode that reads the right bytes as the wrong width — or
/// swaps them — agrees with a narrow test value and disagrees with a full-width one. Zero and a
/// negative are in every set because a sign bit is exactly what a width error moves.
/// </remarks>
public sealed class IntegerIntegrationTests(ITestOutputHelper output) : CompactLogix5X80IntegrationTestBase(output)
{
    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)42)]
    [InlineData((sbyte)-1)]
    [InlineData(sbyte.MinValue)]
    [InlineData(sbyte.MaxValue)]
    public async Task WriteAndReadBack_SIntValue_RoundTripsAndTheTagIsDeclaredSInt(sbyte valueToWrite) =>
        await AssertRoundTripAsync(
            new SIntDataPoint(new TagName(TagAddresses.SInt), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.SInt, AllenBradleyDataType.Sint));

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)4242)]
    [InlineData((short)-1)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    public async Task WriteAndReadBack_IntValue_RoundTripsAndTheTagIsDeclaredInt(short valueToWrite) =>
        await AssertRoundTripAsync(
            new IntDataPoint(new TagName(TagAddresses.Int), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.Int, AllenBradleyDataType.Int));

    [Theory]
    [InlineData(0)]
    [InlineData(123456)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task WriteAndReadBack_DIntValue_RoundTripsAndTheTagIsDeclaredDInt(int valueToWrite) =>
        await AssertRoundTripAsync(
            new DIntDataPoint(new TagName(TagAddresses.DInt), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.DInt, AllenBradleyDataType.Dint));

    [Theory]
    [InlineData(0L)]
    [InlineData(1234567890123L)]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public async Task WriteAndReadBack_LIntValue_RoundTripsAndTheTagIsDeclaredLInt(long valueToWrite) =>
        await AssertRoundTripAsync(
            new LIntDataPoint(new TagName(TagAddresses.LInt), DefaultPollFrequency, NoChannels),
            valueToWrite,
            ExpectedTagDefinitions.AtomicScalar(TagAddresses.LInt, AllenBradleyDataType.Lint));
}
