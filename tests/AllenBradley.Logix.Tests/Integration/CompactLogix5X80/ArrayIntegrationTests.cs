using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The whole-array round trip, which the scalar helper on the base class cannot do: a data point value
/// carrying an array compares by reference, so the elements are asserted rather than the value.
/// </summary>
public sealed class ArrayIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private static readonly SIntArrayDataPoint Samples = new(
        new TagName(TagAddresses.SIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly IntArrayDataPoint Readings = new(
        new TagName(TagAddresses.IntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly DIntArrayDataPoint Totals = new(
        new TagName(TagAddresses.DIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    // Both ends of the range and one value asymmetric in its bytes, so a swapped or mis-sized element
    // cannot agree with the read-back.
    private static readonly sbyte[] TenSamples =
        [0, 1, -1, 42, sbyte.MinValue, sbyte.MaxValue, 10, 20, 30, 40];

    private static readonly short[] TenReadings =
        [0, 1, -1, 4711, short.MinValue, short.MaxValue, 10, 20, 30, 40];

    private static readonly int[] TenTotals =
        [0, 1, -1, 123456, int.MinValue, int.MaxValue, 10, 20, 30, 40];

    [Fact]
    public async Task TheIntArrayTagIsDeclaredWithTheRankAndCountItIsConfiguredWith()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [Readings];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        var expected = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.IntArray, AllenBradleyDataType.Int, TagAddresses.ArrayElementCount);
        resolved.Should().ContainSingle().Which.Should().Be(new ResolvedDataPoint(Readings, expected));
    }

    [Fact]
    public async Task AnIntArrayIsPolledWholeAndArrivesAsOneValueOfItsDeclaredLength()
    {
        // Arrange
        var group = new LogixDataPointGroup(DefaultPollFrequency, [Readings]);

        // Act
        var readResult = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        // Whatever the tag holds when this runs, reported for the run that passes.
        var elements = readResult.Should().ContainSingle().Which.Value.Should().BeOfType<short[]>().Subject;
        Output.WriteLine($"{TagAddresses.IntArray} = [{string.Join(", ", elements)}]");
        elements.Should().HaveCount(
            TagAddresses.ArrayElementCount.Value,
            "the handle carries the configured element count, and one without it reads a single element");
    }

    [Fact]
    public async Task AnSIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.SIntArray, AllenBradleyDataType.Sint, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Samples, TenSamples, expectedDefinition);
    }

    [Fact]
    public async Task AnIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.IntArray, AllenBradleyDataType.Int, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Readings, TenReadings, expectedDefinition);
    }

    [Fact]
    public async Task ADIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.DIntArray, AllenBradleyDataType.Dint, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Totals, TenTotals, expectedDefinition);
    }

    private async Task AssertArrayRoundTripAsync<TElement>(
        LogixDataPoint<TElement[]> dataPoint, TElement[] valuesToWrite, TagDefinition expectedDefinition)
    {
        // Arrange
        // Resolving as well as reading is what makes this say something: elements that survive a write
        // and a read are consistent with the tag being almost anything of the right width.
        var cancellationToken = TestContext.Current.CancellationToken;
        ILogixDataPoint[] dataPoints = [dataPoint];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, cancellationToken);
        await Client.WriteAsync([dataPoint.CreateLogixValue(valuesToWrite)], cancellationToken);
        var readResult = await Client.ReadAsync(group, cancellationToken);

        // Assert
        resolved.Should().ContainSingle()
            .Which.Should().Be(new ResolvedDataPoint(dataPoint, expectedDefinition));
        readResult.Should().ContainSingle()
            .Which.Value.Should().BeOfType<TElement[]>().Which.Should().Equal(valuesToWrite);
    }
}
