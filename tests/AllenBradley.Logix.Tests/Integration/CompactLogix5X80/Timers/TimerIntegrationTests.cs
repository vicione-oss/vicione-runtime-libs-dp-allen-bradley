using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Timers;

/// <summary>
/// The timer node reaches <c>.ACC</c> alone, and a UDT node reaches the other members; both are driven
/// against the same <see cref="TagAddresses.Timer"/>. The suite writes its <c>.ACC</c> and <c>.PRE</c>.
/// </summary>
public sealed class TimerIntegrationTests(ITestOutputHelper output) : CompactLogix5X80IntegrationTestBase(output)
{
    private const int Preset = 5000;

    private static readonly TimerDataPoint Timer =
        new(TagPath.Parse(TagAddresses.Timer), DefaultPollFrequency, NoChannels);

    private static readonly DIntDataPoint TimerPreset =
        new(TagPath.Parse(TagAddresses.TimerMember("PRE")), DefaultPollFrequency, NoChannels);

    private static readonly BoolDataPoint TimerDone =
        new(TagPath.Parse(TagAddresses.TimerMember("DN")), DefaultPollFrequency, NoChannels);

    [Theory]
    [InlineData(0)]
    [InlineData(1234)]
    [InlineData(int.MaxValue)]
    public async Task AnAccumulatedTimeRoundTripsAndItsTagIsDeclaredATimer(int valueToWrite)
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(Timer, valueToWrite);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(Timer, ExpectedDeclaredTypes.TimerScalar(Timer)),
            Timer.CreateLogixValue(valueToWrite));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ANegativeAccumulatedTimeIsRefusedAndNeverReachesTheController()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([Timer.CreateLogixValue(1234)], cancellationToken);

        // Act
        var write = await Record.ExceptionAsync(
            () => Client.WriteAsync([Timer.CreateLogixValue(-1)], cancellationToken).AsTask());
        var accumulatedTimeAfterwards = await ReadAsync(Timer);

        // Assert
        write.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain(Timer.TagAddress.Value);
        accumulatedTimeAfterwards.Should().Be(Timer.CreateLogixValue(1234));
    }

    [Fact]
    public async Task WritingTheAccumulatedTimeLeavesThePresetAlone()
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
    public async Task ThePresetRoundTripsThroughAUdtNodeAsADInt()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(TimerPreset, Preset);

        // Assert
        var expectedResult = new RoundTripResult(
            new ResolvedDataPoint(
                TimerPreset, ExpectedDeclaredTypes.AtomicScalar(TimerPreset, AllenBradleyDataType.Dint)),
            TimerPreset.CreateLogixValue(Preset));

        roundTripResult.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TheDoneBitReadsThroughAUdtNodeAsABool()
    {
        // Arrange
        ILogixDataPoint[] dataPoints = [TimerDone];

        // Act
        var resolved = await Client.ResolveDataPoints(dataPoints, TestContext.Current.CancellationToken);
        var done = await ReadAsync(TimerDone);

        // Assert
        // No instruction runs on the tag, so nothing ever sets the bit.
        resolved.Should().ContainSingle().Which.Should().Be(new ResolvedDataPoint(
            TimerDone, ExpectedDeclaredTypes.AtomicScalar(TimerDone, AllenBradleyDataType.Bool)));
        done.Should().Be(TimerDone.CreateLogixValue(false));
    }

    private async Task<ILogixDataPointValue> ReadAsync(ILogixDataPoint dataPoint)
    {
        var group = new LogixDataPointGroup(DefaultPollFrequency, [dataPoint]);
        var values = await Client.ReadAsync(group, TestContext.Current.CancellationToken);

        return values.Single();
    }
}
