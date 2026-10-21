using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Timers;

/// <summary>
/// Each element of <see cref="TagAddresses.TimerArray"/> is addressed with a subscript as a timer of its own.
/// </summary>
public abstract class TimerArrayElementIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const int ElementIndex = 3;
    private const int AccumulatedTime = 1234;

    private TimerDataPoint Element => ElementAt(ElementIndex);

    [Fact]
    public async Task ATimerElementReadsBackAsWritten()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(Element, AccumulatedTime);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(Element,
                new DeclaredType(Element.TagAddress, AllenBradleyDataType.Timer, MaxLength: null,
                    DimensionCount.Scalar, new ElementCount(1))),
            Element.CreateLogixValue(AccumulatedTime));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnElementKeepsTheOthers()
    {
        // Arrange
        // No data point reads a TIMER array whole, so each element is read as a timer.
        var cancellationToken = TestContext.Current.CancellationToken;
        var everyElement = Enumerable.Range(0, (int)TagAddresses.ArrayElementCount.Value).Select(ElementAt).ToArray();
        await Client.WriteAsync([.. everyElement.Select(element => element.CreateLogixValue(0))], cancellationToken);

        // Act
        await Client.WriteAsync([Element.CreateLogixValue(AccumulatedTime)], cancellationToken);
        var everyElementAfterwards = await Client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, everyElement), cancellationToken);

        // Assert
        var expected = everyElement.Select(element =>
            element.CreateLogixValue(element == Element ? AccumulatedTime : 0));

        everyElementAfterwards.Should().Equal(expected);
    }

    [Fact]
    public async Task ANegativeTimeIsRefused()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([Element.CreateLogixValue(AccumulatedTime)], cancellationToken);

        // Act
        var write = await Record.ExceptionAsync(() =>
            Client.WriteAsync([Element.CreateLogixValue(-1)], cancellationToken).AsTask());
        var accumulatedTimeAfterwards = await Client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, [Element]), cancellationToken);

        // Assert
        write.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain(Element.TagAddress.Value);
        accumulatedTimeAfterwards.Should().ContainSingle()
            .Which.Should().Be(Element.CreateLogixValue(AccumulatedTime));
    }

    [Fact]
    public async Task AnElementPastTheEndIsReported()
    {
        // Arrange
        var element = ElementAt((int)TagAddresses.ArrayElementCount.Value);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([element], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{element.TagAddress.Value}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    [Fact]
    public async Task ASubscriptOnAScalarTimerIsReported()
    {
        // Arrange
        var element = new TimerDataPoint(
            TagPath.Parse(TagAddresses.ArrayElement(Tags.Timer, ElementIndex)), DefaultPollFrequency, NoChannels);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([element], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{element.TagAddress.Value}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    private TimerDataPoint ElementAt(int index) =>
        new(TagPath.Parse(TagAddresses.ArrayElement(Tags.TimerArray, index)), DefaultPollFrequency, NoChannels);

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : TimerArrayElementIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : TimerArrayElementIntegrationTests(fixture, output, TagScope.Program);
}
