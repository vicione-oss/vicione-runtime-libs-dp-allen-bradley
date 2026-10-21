using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Timers;

/// <summary>
/// The timer node reaches <c>.ACC</c> alone. A UDT node reaches the other members. Both run against the
/// same <see cref="TagAddresses.Timer"/>.
/// </summary>
public abstract class TimerIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const int Preset = 5000;

    private TimerDataPoint Timer =>
        new(TagPath.Parse(Tags.Timer), DefaultPollFrequency, NoChannels);

    private DIntDataPoint TimerPreset =>
        new(TagPath.Parse(TagAddresses.Member(Tags.Timer, "PRE")), DefaultPollFrequency, NoChannels);

    private DIntDataPoint TimerAccumulatedTime =>
        new(TagPath.Parse(TagAddresses.Member(Tags.Timer, "ACC")), DefaultPollFrequency, NoChannels);

    private BoolDataPoint TimerDone =>
        new(TagPath.Parse(TagAddresses.Member(Tags.Timer, "DN")), DefaultPollFrequency, NoChannels);

    [Theory]
    [InlineData(0)]
    [InlineData(1234)]
    [InlineData(int.MaxValue)]
    public async Task ATimerReadsBackAsWritten(int valueToWrite)
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(Timer, valueToWrite);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(Timer,
                new DeclaredType(Timer.TagAddress, AllenBradleyDataType.Timer, MaxLength: null, DimensionCount.Scalar,
                    new ElementCount(1))),
            Timer.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ANegativeTimeIsRefused()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([Timer.CreateLogixValue(1234)], cancellationToken);

        // Act
        var write = await Record.ExceptionAsync(() =>
            Client.WriteAsync([Timer.CreateLogixValue(-1)], cancellationToken).AsTask());
        var accumulatedTimeAfterwards = await ReadAsync(Timer);

        // Assert
        write.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain(Timer.TagAddress.Value);
        accumulatedTimeAfterwards.Should().Be(Timer.CreateLogixValue(1234));
    }

    [Fact]
    public async Task WritingATimerSetsItsAcc()
    {
        // Arrange
        // The tag is cleared first, so a 1234 left behind by another test cannot pass this one.
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([TimerAccumulatedTime.CreateLogixValue(0)], cancellationToken);

        // Act
        await Client.WriteAsync([Timer.CreateLogixValue(1234)], cancellationToken);
        var accumulatedTime = await ReadAsync(TimerAccumulatedTime);

        // Assert
        accumulatedTime.Should().Be(TimerAccumulatedTime.CreateLogixValue(1234));
    }

    [Fact]
    public async Task AMissingTimerIsReportedByName()
    {
        // Arrange
        var missingTimer =
            new TimerDataPoint(TagPath.Parse(Tags.MissingTimer), DefaultPollFrequency, NoChannels);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([missingTimer], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{Tags.MissingTimer}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    [Fact]
    public async Task WritingATimerKeepsItsPreset()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([TimerPreset.CreateLogixValue(Preset)], cancellationToken);

        // Act
        await Client.WriteAsync([Timer.CreateLogixValue(0)], cancellationToken);
        var presetAfterwards = await ReadAsync(TimerPreset);

        // Assert
        presetAfterwards.Should().Be(TimerPreset.CreateLogixValue(Preset));
    }

    [Fact]
    public async Task ThePresetReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(TimerPreset, Preset);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(
                TimerPreset,
                new DeclaredType(TimerPreset.TagAddress, AllenBradleyDataType.Dint, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            TimerPreset.CreateLogixValue(Preset));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TheDoneBitReadsFalse()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [TimerDone];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);
        var done = await ReadAsync(TimerDone);

        // Assert
        // No instruction runs on the tag, so nothing sets the bit.
        resolved.Should().ContainSingle().Which.Should().Be(new ResolvedDataPoint(
            TimerDone,
            new DeclaredType(TimerDone.TagAddress, AllenBradleyDataType.Bool, MaxLength: null, DimensionCount.Scalar,
                new ElementCount(1))));
        done.Should().Be(TimerDone.CreateLogixValue(false));
    }

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : TimerIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : TimerIntegrationTests(fixture, output, TagScope.Program);
}
