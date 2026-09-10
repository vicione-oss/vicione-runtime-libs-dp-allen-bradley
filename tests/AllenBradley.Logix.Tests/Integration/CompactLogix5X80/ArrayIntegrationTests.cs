using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The whole-array round trip, which the scalar helper on the base class cannot do: a data point value
/// carrying a <c>short[]</c> compares by reference, so the elements are asserted rather than the value.
/// </summary>
public sealed class ArrayIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private static readonly IntArrayDataPoint Readings = new(
        new TagName(TagAddresses.IntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.IntArrayElementCount);

    // Both ends of the range and one value asymmetric in its bytes, so a swapped or mis-sized element
    // cannot agree with the read-back.
    private static readonly short[] TenValues =
        [0, 1, -1, 4711, short.MinValue, short.MaxValue, 10, 20, 30, 40];

    [Fact]
    public async Task TheIntArrayTagIsDeclaredWithTheRankAndCountItIsConfiguredWith()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [Readings];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        var expected = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.IntArray, AllenBradleyDataType.Int, TagAddresses.IntArrayElementCount);
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
            TagAddresses.IntArrayElementCount.Value,
            "the handle carries the configured element count, and one without it reads a single element");
    }

    [Fact]
    public async Task AnIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        // Resolving as well as reading is what makes this say something: elements that survive a write
        // and a read are consistent with the tag being almost anything of the right width.
        var cancellationToken = TestContext.Current.CancellationToken;
        ILogixDataPoint[] dataPoints = [Readings];
        var group = new LogixDataPointGroup(DefaultPollFrequency, dataPoints);

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, cancellationToken);
        await Client.WriteAsync([Readings.CreateLogixValue(TenValues)], cancellationToken);
        var readResult = await Client.ReadAsync(group, cancellationToken);

        // Assert
        var expected = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.IntArray, AllenBradleyDataType.Int, TagAddresses.IntArrayElementCount);
        resolved.Should().ContainSingle().Which.Should().Be(new ResolvedDataPoint(Readings, expected));
        readResult.Should().ContainSingle()
            .Which.Value.Should().BeOfType<short[]>().Which.Should().Equal(TenValues);
    }
}
