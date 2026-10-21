using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Integers;

/// <summary>
/// Each element set holds both ends of the range and one value asymmetric in its bytes. A swapped or
/// mis-sized element cannot agree with the read-back.
/// </summary>
public abstract class IntegerArrayIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    [Fact]
    public async Task AnIntArrayIsDeclaredWithItsElementCount()
    {
        // Arrange
        IntArrayDataPoint dataPoint = new(
            TagPath.Parse(Tags.IntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        ILogixDataPoint[] dataPoints = [dataPoint];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.Int, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(dataPoint, expectedDeclaredType);

        resolved.Should().ContainSingle().Which.Should().Be(expectedResolved);
    }

    [Fact]
    public async Task OnePollReadsTheWholeIntArray()
    {
        // Arrange
        var dataPoint = new IntArrayDataPoint(
            TagPath.Parse(Tags.IntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        var group = new LogixDataPointGroup(DefaultPollFrequency, [dataPoint]);

        // Act
        var readResult = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        // Assert
        // The tag value is logged because the test accepts whatever the tag holds.
        var elements = readResult.Should().ContainSingle().Which.Value.Should().BeOfType<short[]>().Subject;
        Output.WriteLine($"{Tags.IntArray} = [{string.Join(", ", elements)}]");
        elements.Should().HaveCount(
            (int)TagAddresses.ArrayElementCount.Value,
            "the handle carries the configured element count, and one without it reads a single element");
    }

    [Fact]
    public async Task AnSIntArrayReadsBackAsWritten()
    {
        // Arrange
        SIntArrayDataPoint samples = new(
            TagPath.Parse(Tags.SIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        sbyte[] tenSamples = [0, 1, -1, 42, sbyte.MinValue, sbyte.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(samples, tenSamples);

        // Assert
        var expectedDeclaredType = new DeclaredType(samples.TagAddress, AllenBradleyDataType.Sint, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(samples, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<sbyte[]>().Which.Should().Equal(tenSamples);
    }

    [Fact]
    public async Task AnIntArrayReadsBackAsWritten()
    {
        // Arrange
        short[] tenReadings = [0, 1, -1, 4711, short.MinValue, short.MaxValue, 10, 20, 30, 40];

        // Act
        IntArrayDataPoint dataPoint1 = new(
            TagPath.Parse(Tags.IntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        var roundTripResult = await RoundTripAsync(dataPoint1, tenReadings);

        // Assert
        var expectedDeclaredType = new DeclaredType(dataPoint1.TagAddress, AllenBradleyDataType.Int, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(dataPoint1, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<short[]>().Which.Should().Equal(tenReadings);
    }

    [Fact]
    public async Task AdIntArrayReadsBackAsWritten()
    {
        // Arrange
        DIntArrayDataPoint totals = new(
            TagPath.Parse(Tags.DIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        int[] tenTotals = [0, 1, -1, 123456, int.MinValue, int.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(totals, tenTotals);

        // Assert
        var expectedDeclaredType = new DeclaredType(totals.TagAddress, AllenBradleyDataType.Dint, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(totals, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<int[]>().Which.Should().Equal(tenTotals);
    }

    [Fact]
    public async Task AnLIntArrayReadsBackAsWritten()
    {
        // Arrange
        LIntArrayDataPoint timestamps = new(
            TagPath.Parse(Tags.LIntArray),
            DefaultPollFrequency,
            NoChannels,
            TagAddresses.ArrayElementCount);
        long[] tenTimestamps = [0, 1, -1, 1234567890123, long.MinValue, long.MaxValue, 10, 20, 30, 40];

        // Act
        var roundTripResult = await RoundTripAsync(timestamps, tenTimestamps);

        // Assert
        var expectedDeclaredType = new DeclaredType(timestamps.TagAddress, AllenBradleyDataType.Lint, MaxLength: null,
            DimensionCount.OneDimensional, TagAddresses.ArrayElementCount);
        var expectedResolved = new ResolvedDataPoint(timestamps, expectedDeclaredType);

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<long[]>().Which.Should().Equal(tenTimestamps);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : IntegerArrayIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : IntegerArrayIntegrationTests(fixture, output, TagScope.Program);
}
