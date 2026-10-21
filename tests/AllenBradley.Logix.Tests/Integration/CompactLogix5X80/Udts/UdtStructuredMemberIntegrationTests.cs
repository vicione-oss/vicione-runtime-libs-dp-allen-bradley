using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Udts;

/// <summary>
/// The non-atomic members of <see cref="TagAddresses.Udt"/>: arrays, strings, a timer, a timer array and a
/// counter. Each is reached through the tag's template and declared by the member's own template.
/// </summary>
public abstract class UdtStructuredMemberIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const int ElementIndex = 3;
    private const int DIntElementValue = 123456;

    private DIntArrayDataPoint DIntArray => new(
        TagPath.Parse(Tags.Udt.DIntArray), DefaultPollFrequency, NoChannels, TagAddresses.ArrayElementCount);

    private DIntDataPoint DIntElement => new(
        TagPath.Parse(TagAddresses.ArrayElement(Tags.Udt.DIntArray, ElementIndex)), DefaultPollFrequency, NoChannels);

    [Fact]
    public async Task AnArrayMemberReadsBackAsWritten()
    {
        // Arrange
        int[] elements = [int.MinValue, -1, 0, 1, DIntElementValue, 5, 6, 7, 8, int.MaxValue];

        // Act
        var roundTripResult = await RoundTripAsync(DIntArray, elements);

        // Assert
        var expectedResolved = new ResolvedDataPoint(
            DIntArray,
            new DeclaredType(DIntArray.TagAddress, AllenBradleyDataType.Dint, MaxLength: null,
                DimensionCount.OneDimensional, TagAddresses.ArrayElementCount));

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<int[]>().Which.Should().Equal(elements);
    }

    [Fact]
    public async Task AnArrayMemberElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(DIntElement, DIntElementValue);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(DIntElement,
                new DeclaredType(DIntElement.TagAddress, AllenBradleyDataType.Dint, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            DIntElement.CreateLogixValue(DIntElementValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnElementKeepsTheOthers()
    {
        // Arrange

        // Act
        var arrayAfterWrite = await WriteOneElementOfAZeroedArrayAsync(DIntArray, DIntElement, DIntElementValue);

        // Assert
        int[] expected = [0, 0, 0, DIntElementValue, 0, 0, 0, 0, 0, 0];

        arrayAfterWrite.Should().Equal(expected);
    }

    [Fact]
    public async Task ABoolArrayMemberReadsBackAsWritten()
    {
        // Arrange
        var flags = new BoolArrayDataPoint(
            TagPath.Parse(Tags.Udt.BoolArray), DefaultPollFrequency, NoChannels, TagAddresses.BoolArrayElementCount);
        // The values cover both ends of every byte of the word, grouped eight to a line as the controller packs them.
        bool[] thirtyTwoFlags =
        [
            true, true, false, false, false, false, false, true,
            true, false, false, false, false, false, false, true,
            true, false, false, false, false, false, false, false,
            false, false, false, false, false, false, true, true
        ];

        // Act
        var roundTripResult = await RoundTripAsync(flags, thirtyTwoFlags);

        // Assert
        var expectedResolved = new ResolvedDataPoint(
            flags,
            new DeclaredType(flags.TagAddress, AllenBradleyDataType.Bool, MaxLength: null,
                DimensionCount.OneDimensional, TagAddresses.BoolArrayElementCount));

        roundTripResult.Resolved.Should().Be(expectedResolved);
        roundTripResult.ReadValue.Value.Should().BeOfType<bool[]>().Which.Should().Equal(thirtyTwoFlags);
    }

    [Fact]
    public async Task AStringMemberReadsBackAsWritten()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            TagPath.Parse(Tags.Udt.String), DefaultPollFrequency, NoChannels, TagAddresses.StringCapacity);
        const string valueToWrite = "Hello World";

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(dataPoint,
                new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.String, TagAddresses.StringCapacity,
                    DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task AFullString20MemberReadsBackAsWritten()
    {
        // Arrange
        var dataPoint = new StringDataPoint(
            TagPath.Parse(Tags.Udt.String20), DefaultPollFrequency, NoChannels, TagAddresses.String20Capacity);
        var valueToWrite = new string('X', TagAddresses.String20Capacity.Value);

        // Act
        var roundTripResult = await RoundTripAsync(dataPoint, valueToWrite);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(dataPoint,
                new DeclaredType(dataPoint.TagAddress, AllenBradleyDataType.String, TagAddresses.String20Capacity,
                    DimensionCount.Scalar, new ElementCount(1))),
            dataPoint.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task ATimerMemberReadsBackAsWritten()
    {
        // Arrange
        var timer = new TimerDataPoint(TagPath.Parse(Tags.Udt.Timer), DefaultPollFrequency, NoChannels);
        const int accumulatedTime = 1234;

        // Act
        var roundTripResult = await RoundTripAsync(timer, accumulatedTime);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(timer,
                new DeclaredType(timer.TagAddress, AllenBradleyDataType.Timer, MaxLength: null, DimensionCount.Scalar,
                    new ElementCount(1))),
            timer.CreateLogixValue(accumulatedTime));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task ATimerMemberPresetReadsBackAsWritten()
    {
        // Arrange
        var preset = new DIntDataPoint(
            TagPath.Parse(TagAddresses.Member(Tags.Udt.Timer, "PRE")), DefaultPollFrequency, NoChannels);
        const int presetValue = 5000;

        // Act
        var roundTripResult = await RoundTripAsync(preset, presetValue);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(preset,
                new DeclaredType(preset.TagAddress, AllenBradleyDataType.Dint, MaxLength: null, DimensionCount.Scalar,
                    new ElementCount(1))),
            preset.CreateLogixValue(presetValue));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task ATimerArrayMemberElementReadsBackAsWritten()
    {
        // Arrange
        var element = new TimerDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.Udt.TimerArray, ElementIndex)), DefaultPollFrequency,
            NoChannels);
        const int accumulatedTime = 1234;

        // Act
        var roundTripResult = await RoundTripAsync(element, accumulatedTime);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(element,
                new DeclaredType(element.TagAddress, AllenBradleyDataType.Timer, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            element.CreateLogixValue(accumulatedTime));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task ACounterMemberReadsBackAsWritten()
    {
        // Arrange
        var counter = new CounterDataPoint(TagPath.Parse(Tags.Udt.Counter), DefaultPollFrequency, NoChannels);
        const int accumulatedValue = -1234;

        // Act
        var roundTripResult = await RoundTripAsync(counter, accumulatedValue);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(counter,
                new DeclaredType(counter.TagAddress, AllenBradleyDataType.Counter, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            counter.CreateLogixValue(accumulatedValue));

        roundTripResult.Should().Be(expected);
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UdtStructuredMemberIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : UdtStructuredMemberIntegrationTests(fixture, output, TagScope.Program);
}
