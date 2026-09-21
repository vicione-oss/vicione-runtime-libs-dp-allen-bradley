using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// Every element set holds both ends of the range and one value asymmetric in its bytes, so a swapped or
/// mis-sized element cannot agree with the read-back.
/// </summary>
public sealed class IntegerArrayIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    [Fact]
    public async Task TheIntArrayTagIsDeclaredWithTheRankAndCountItIsConfiguredWith()
    {
        // Arrange
        IntArrayDataPoint dataPoint = new(
            TagPath.Parse(TagAddresses.IntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        ILogixDataPoint[] dataPoints = [dataPoint];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        var expectedDeclaredType = ExpectedDeclaredTypes.AtomicArray(dataPoint, AllenBradleyDataType.Int, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(dataPoint, expectedDeclaredType);

        resolved.Should().ContainSingle().Which.Should().Be(expectedResolved);
    }

    [Fact]
    public async Task AnIntArrayIsPolledWholeAndArrivesAsOneValueOfItsDeclaredLength()
    {
        // Arrange
        var dataPoint = new IntArrayDataPoint(
            TagPath.Parse(TagAddresses.IntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        var group = new LogixDataPointGroup(DefaultPollFrequency, [dataPoint]);

        // Act
        var readResult = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        // Whatever the tag holds when this runs, reported for the run that passes.
        var elements = readResult.Should().ContainSingle().Which.Value.Should().BeOfType<short[]>().Subject;
        Output.WriteLine($"{TagAddresses.IntArray} = [{string.Join(", ", elements)}]");
        elements.Should().HaveCount(
            (int)TagAddresses.ArrayElementCount.Value,
            "the handle carries the configured element count, and one without it reads a single element");
    }

    [Fact]
    public async Task AnSIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        SIntArrayDataPoint samples = new(
            TagPath.Parse(TagAddresses.SIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        sbyte[] tenSamples = [0, 1, -1, 42, sbyte.MinValue, sbyte.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(samples, tenSamples);

        // Assert
        var expectedDeclaredType = ExpectedDeclaredTypes.AtomicArray(samples, AllenBradleyDataType.Sint, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(samples, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<sbyte[]>().Which.Should().Equal(tenSamples);
    }

    [Fact]
    public async Task AnIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        short[] tenReadings = [0, 1, -1, 4711, short.MinValue, short.MaxValue, 10, 20, 30, 40];

        // Act
        IntArrayDataPoint dataPoint1 = new(
            TagPath.Parse(TagAddresses.IntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        var roundTripResult = await RoundTripAsync(dataPoint1, tenReadings);

        // Assert
        var expectedDeclaredType = ExpectedDeclaredTypes.AtomicArray(dataPoint1, AllenBradleyDataType.Int, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(dataPoint1, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<short[]>().Which.Should().Equal(tenReadings);
    }

    [Fact]
    public async Task ADIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        DIntArrayDataPoint totals = new(
            TagPath.Parse(TagAddresses.DIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        int[] tenTotals = [0, 1, -1, 123456, int.MinValue, int.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(totals, tenTotals);

        // Assert
        var expectedDeclaredType = ExpectedDeclaredTypes.AtomicArray(totals, AllenBradleyDataType.Dint, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(totals, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<int[]>().Which.Should().Equal(tenTotals);
    }

    [Fact]
    public async Task AnLIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        LIntArrayDataPoint timestamps = new(
            TagPath.Parse(TagAddresses.LIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        long[] tenTimestamps = [0, 1, -1, 1234567890123, long.MinValue, long.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(timestamps, tenTimestamps);

        // Assert
        var expectedDeclaredType = ExpectedDeclaredTypes.AtomicArray(timestamps, AllenBradleyDataType.Lint, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(timestamps, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<long[]>().Which.Should().Equal(tenTimestamps);
    }
}
