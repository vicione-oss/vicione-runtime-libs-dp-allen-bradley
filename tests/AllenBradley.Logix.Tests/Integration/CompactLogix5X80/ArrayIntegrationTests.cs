using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
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
    private static readonly BoolArrayDataPoint Flags = new(
        new TagName(TagAddresses.BoolArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.BoolArrayElementCount);

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

    private static readonly LIntArrayDataPoint Timestamps = new(
        new TagName(TagAddresses.LIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly USIntArrayDataPoint Pressures = new(
        new TagName(TagAddresses.USIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly UIntArrayDataPoint Speeds = new(
        new TagName(TagAddresses.UIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly UDIntArrayDataPoint Runtimes = new(
        new TagName(TagAddresses.UDIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly ULIntArrayDataPoint CycleCounts = new(
        new TagName(TagAddresses.ULIntArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly RealArrayDataPoint Temperatures = new(
        new TagName(TagAddresses.RealArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    private static readonly LRealArrayDataPoint Positions = new(
        new TagName(TagAddresses.LRealArray),
        DefaultPollFrequency,
        NoChannels,
        TagAddresses.ArrayElementCount);

    // Both ends of every byte of the word, so a bit read at the wrong shift or out of the wrong byte
    // cannot agree with the read-back. Grouped eight to a line as the controller packs them.
    private static readonly bool[] ThirtyTwoFlags =
    [
        true, true, false, false, false, false, false, true,
        true, false, false, false, false, false, false, true,
        true, false, false, false, false, false, false, false,
        false, false, false, false, false, false, true, true,
    ];

    // Both ends of the range and one value asymmetric in its bytes, so a swapped or mis-sized element
    // cannot agree with the read-back.
    private static readonly sbyte[] TenSamples =
        [0, 1, -1, 42, sbyte.MinValue, sbyte.MaxValue, 10, 20, 30, 40];

    private static readonly short[] TenReadings =
        [0, 1, -1, 4711, short.MinValue, short.MaxValue, 10, 20, 30, 40];

    private static readonly int[] TenTotals =
        [0, 1, -1, 123456, int.MinValue, int.MaxValue, 10, 20, 30, 40];

    private static readonly long[] TenTimestamps =
        [0, 1, -1, 1234567890123, long.MinValue, long.MaxValue, 10, 20, 30, 40];

    // An unsigned type's ends are zero and its maximum, and the two elements above the signed maximum are
    // what a signed misreading gets wrong.
    private static readonly byte[] TenPressures =
        [0, 1, 42, 128, 200, byte.MaxValue, 10, 20, 30, 40];

    private static readonly ushort[] TenSpeeds =
        [0, 1, 4242, 32768, 50000, ushort.MaxValue, 10, 20, 30, 40];

    private static readonly uint[] TenRuntimes =
        [0, 1, 123456, 2147483648, 4000000000, uint.MaxValue, 10, 20, 30, 40];

    private static readonly ulong[] TenCycleCounts =
    [
        0, 1, 1234567890123, 9223372036854775808, 18446744073709551614, ulong.MaxValue, 10, 20, 30, 40,
    ];

    private static readonly float[] TenTemperatures =
        [0f, 1f, -1f, 3.14159f, float.MinValue, float.MaxValue, 10.5f, -20.25f, 30.75f, 40f];

    private static readonly double[] TenPositions =
    [
        0d, 1d, -1d, 3.141592653589793d, double.MinValue, double.MaxValue, 10.5d, -20.25d, 30.75d, 40d,
    ];

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
    public async Task ABoolArrayIsWrittenWholeAndReadsBackAsTheBitsThatWentIn()
    {
        // Arrange
        // The controller declares the two halves of this differently from every other array: the type
        // as DWORD, which decodes to Bool, and the extent as the one word its 32 bits fill.
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.BoolArray, AllenBradleyDataType.Bool, TagAddresses.BoolArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Flags, ThirtyTwoFlags, expectedDefinition);
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

    [Fact]
    public async Task AnLIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.LIntArray, AllenBradleyDataType.Lint, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Timestamps, TenTimestamps, expectedDefinition);
    }

    [Fact]
    public async Task AUSIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.USIntArray, AllenBradleyDataType.Usint, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Pressures, TenPressures, expectedDefinition);
    }

    [Fact]
    public async Task AUIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.UIntArray, AllenBradleyDataType.Uint, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Speeds, TenSpeeds, expectedDefinition);
    }

    [Fact]
    public async Task AUDIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.UDIntArray, AllenBradleyDataType.Udint, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Runtimes, TenRuntimes, expectedDefinition);
    }

    [Fact]
    public async Task AULIntArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.ULIntArray, AllenBradleyDataType.Ulint, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(CycleCounts, TenCycleCounts, expectedDefinition);
    }

    [Fact]
    public async Task ARealArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.RealArray, AllenBradleyDataType.Real, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Temperatures, TenTemperatures, expectedDefinition);
    }

    [Fact]
    public async Task AnLRealArrayIsWrittenWholeAndReadsBackAsTheElementsThatWentIn()
    {
        // Arrange
        var expectedDefinition = ExpectedTagDefinitions.AtomicArray(
            TagAddresses.LRealArray, AllenBradleyDataType.Lreal, TagAddresses.ArrayElementCount);

        // Act
        // Assert
        await AssertArrayRoundTripAsync(Positions, TenPositions, expectedDefinition);
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
