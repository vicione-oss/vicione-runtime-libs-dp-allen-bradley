using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// The one suite in this folder that only reads: writing an array is a later slice, so what
/// <see cref="TagAddresses.IntArray"/> holds is the controller's and these tests leave it alone.
/// That is also why the read is checked against the declaration rather than against a value — the
/// elements are whatever the program put there, and how many of them arrive is the question.
/// </summary>
public sealed class ArrayIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private static readonly IntArrayDataPoint Readings = new(
        new TagName(TagAddresses.IntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.IntArrayElementCount);

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
        // Written out before the assertion, because the elements are the controller's and the run is the
        // only place anyone sees them.
        var elements = readResult.Should().ContainSingle().Which.Value.Should().BeOfType<short[]>().Subject;
        Output.WriteLine($"{TagAddresses.IntArray} = [{string.Join(", ", elements)}]");
        elements.Should().HaveCount(
            TagAddresses.IntArrayElementCount.Value,
            "the handle carries the configured element count, and one without it reads a single element");
    }
}
