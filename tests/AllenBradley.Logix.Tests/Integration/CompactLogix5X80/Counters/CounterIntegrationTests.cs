using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Counters;

/// <summary>
/// The counter node reaches <c>.ACC</c> alone, and a UDT node reaches the other members. Both are driven
/// against the same <see cref="TagAddresses.Counter"/>. The suite writes its <c>.ACC</c> and <c>.PRE</c>.
/// </summary>
public abstract class CounterIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const int Preset = 500;

    private CounterDataPoint Counter =>
        new(TagPath.Parse(Tags.Counter), DefaultPollFrequency, NoChannels);

    private DIntDataPoint CounterPreset =>
        new(TagPath.Parse(TagAddresses.Member(Tags.Counter, "PRE")), DefaultPollFrequency, NoChannels);

    private DIntDataPoint CounterAccumulatedValue =>
        new(TagPath.Parse(TagAddresses.Member(Tags.Counter, "ACC")), DefaultPollFrequency, NoChannels);

    private BoolDataPoint CounterDone =>
        new(TagPath.Parse(TagAddresses.Member(Tags.Counter, "DN")), DefaultPollFrequency, NoChannels);

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1234)]
    [InlineData(int.MaxValue)]
    public async Task ACounterReadsBackAsWritten(int valueToWrite)
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(Counter, valueToWrite);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(Counter,
                new DeclaredType(Counter.TagAddress, AllenBradleyDataType.Counter, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            Counter.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task WritingACounterSetsItsAcc()
    {
        // Arrange
        // Cleared first, so a 1234 another test left behind cannot pass this one.
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([CounterAccumulatedValue.CreateLogixValue(0)], cancellationToken);

        // Act
        await Client.WriteAsync([Counter.CreateLogixValue(1234)], cancellationToken);
        var accumulatedValue = await ReadAsync(CounterAccumulatedValue);

        // Assert
        accumulatedValue.Should().Be(CounterAccumulatedValue.CreateLogixValue(1234));
    }

    [Fact]
    public async Task AMissingCounterIsReportedByName()
    {
        // Arrange
        var missingCounter =
            new CounterDataPoint(TagPath.Parse(Tags.MissingCounter), DefaultPollFrequency, NoChannels);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([missingCounter], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{Tags.MissingCounter}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    [Fact]
    public async Task WritingACounterKeepsItsPreset()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([CounterPreset.CreateLogixValue(Preset)], cancellationToken);

        // Act
        await Client.WriteAsync([Counter.CreateLogixValue(0)], cancellationToken);
        var presetAfterwards = await ReadAsync(CounterPreset);

        // Assert
        presetAfterwards.Should().Be(CounterPreset.CreateLogixValue(Preset));
    }

    [Fact]
    public async Task ThePresetReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(CounterPreset, Preset);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(
                CounterPreset,
                new DeclaredType(CounterPreset.TagAddress, AllenBradleyDataType.Dint, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            CounterPreset.CreateLogixValue(Preset));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TheDoneBitReadsFalse()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [CounterDone];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);
        var done = await ReadAsync(CounterDone);

        // Assert
        // No instruction runs on the tag, so nothing ever sets the bit.
        resolved.Should().ContainSingle().Which.Should().Be(new ResolvedDataPoint(
            CounterDone,
            new DeclaredType(CounterDone.TagAddress, AllenBradleyDataType.Bool, MaxLength: null,
                DimensionCount.Scalar, new ElementCount(1))));
        done.Should().Be(CounterDone.CreateLogixValue(false));
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : CounterIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : CounterIntegrationTests(fixture, output, TagScope.Program);
}
