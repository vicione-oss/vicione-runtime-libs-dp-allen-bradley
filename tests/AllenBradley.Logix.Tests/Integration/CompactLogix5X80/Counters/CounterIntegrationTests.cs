using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Counters;

/// <summary>
/// The counter node reaches <c>.ACC</c> alone, and a UDT node reaches the other members; both are driven
/// against the same <see cref="TagAddresses.Counter"/>. The suite writes its <c>.ACC</c> and <c>.PRE</c>.
/// </summary>
public sealed class CounterIntegrationTests(ITestOutputHelper output) : CompactLogix5X80IntegrationTestBase(output)
{
    private const int Preset = 500;

    private static readonly CounterDataPoint Counter =
        new(TagPath.Parse(TagAddresses.Counter), DefaultPollFrequency, NoChannels);

    private static readonly DIntDataPoint CounterPreset =
        new(TagPath.Parse(TagAddresses.CounterMember("PRE")), DefaultPollFrequency, NoChannels);

    private static readonly DIntDataPoint CounterAccumulatedValue =
        new(TagPath.Parse(TagAddresses.CounterMember("ACC")), DefaultPollFrequency, NoChannels);

    private static readonly BoolDataPoint CounterDone =
        new(TagPath.Parse(TagAddresses.CounterMember("DN")), DefaultPollFrequency, NoChannels);

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1234)]
    [InlineData(int.MaxValue)]
    public async Task AnAccumulatedValueRoundTripsAndItsTagIsDeclaredACounter(int valueToWrite)
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(Counter, valueToWrite);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(Counter, ExpectedDeclaredTypes.CounterScalar(Counter)),
            Counter.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TheAccumulatedValueWrittenThroughTheCounterIsWhatItsAccMemberHolds()
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
    public async Task AMissingCounterIsReportedAtConnectNamingTheCounterRatherThanItsAcc()
    {
        // Arrange
        var missingCounter =
            new CounterDataPoint(TagPath.Parse(TagAddresses.MissingCounter), DefaultPollFrequency, NoChannels);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([missingCounter], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{TagAddresses.MissingCounter}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    [Fact]
    public async Task WritingTheAccumulatedValueLeavesThePresetAlone()
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
    public async Task ThePresetRoundTripsThroughAUdtNodeAsADInt()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(CounterPreset, Preset);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(
                CounterPreset, ExpectedDeclaredTypes.AtomicScalar(CounterPreset, AllenBradleyDataType.Dint)),
            CounterPreset.CreateLogixValue(Preset));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TheDoneBitReadsThroughAUdtNodeAsABool()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [CounterDone];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);
        var done = await ReadAsync(CounterDone);

        // Assert
        // No instruction runs on the tag, so nothing ever sets the bit.
        resolved.Should().ContainSingle().Which.Should().Be(new ResolvedDataPoint(
            CounterDone, ExpectedDeclaredTypes.AtomicScalar(CounterDone, AllenBradleyDataType.Bool)));
        done.Should().Be(CounterDone.CreateLogixValue(false));
    }

    private async Task<ILogixDataPointValue> ReadAsync(ILogixDataPoint dataPoint)
    {
        var group = new LogixDataPointGroup(DefaultPollFrequency, [dataPoint]);
        var values = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        return values.Single();
    }
}
