using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Timers;

/// <summary>
/// One element of <see cref="TagAddresses.TimerArray"/> as a timer of its own, addressed with a
/// subscript. The suite writes the <c>.ACC</c> of every element.
/// </summary>
public sealed class TimerArrayElementIntegrationTests(ITestOutputHelper output)
    : CompactLogix5X80IntegrationTestBase(output)
{
    private const int ElementIndex = 3;
    private const int AccumulatedTime = 1234;

    private static readonly TimerDataPoint Element = ElementAt(ElementIndex);

    [Fact]
    public async Task AnElementsAccumulatedTimeRoundTripsAndTheElementIsDeclaredATimer()
    {
        // Arrange

        // Act
        var roundTripResult = await RoundTripAsync(Element, AccumulatedTime);

        // Assert
        var expected = new RoundTripResult(
            new ResolvedDataPoint(Element, ExpectedDeclaredTypes.TimerScalar(Element)),
            Element.CreateLogixValue(AccumulatedTime));

        roundTripResult.Should().Be(expected);
    }

    [Fact]
    public async Task WritingAnElementLeavesTheOtherElementsAsTheyWere()
    {
        // Arrange
        // No data point reads a TIMER array whole, so every element is read as a timer of its own.
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
    public async Task ANegativeAccumulatedTimeForAnElementIsRefusedAndNeverReachesTheController()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await Client.WriteAsync([Element.CreateLogixValue(AccumulatedTime)], cancellationToken);

        // Act
        var write = await Record.ExceptionAsync(
            () => Client.WriteAsync([Element.CreateLogixValue(-1)], cancellationToken).AsTask());
        var accumulatedTimeAfterwards = await Client.ReadAsync(
            new LogixDataPointGroup(DefaultPollFrequency, [Element]), cancellationToken);

        // Assert
        write.Should().BeOfType<LogixTagException>()
            .Which.Message.Should().Contain(Element.TagAddress.Value);
        accumulatedTimeAfterwards.Should().ContainSingle()
            .Which.Should().Be(Element.CreateLogixValue(AccumulatedTime));
    }

    /// <summary>
    /// Each subscript that reaches no element of a TIMER array: one past the end of the array, and one on a
    /// timer that is no array.
    /// </summary>
    public static TheoryData<string> SubscriptsThatReachNoTimerElement =>
    [
        TagAddresses.ArrayElement(TagAddresses.TimerArray, (int)TagAddresses.ArrayElementCount.Value),
        TagAddresses.ArrayElement(TagAddresses.Timer, ElementIndex),
    ];

    [Theory]
    [MemberData(nameof(SubscriptsThatReachNoTimerElement))]
    public async Task ASubscriptThatReachesNoElementIsReportedAtConnectNamingTheElement(string address)
    {
        // Arrange
        var element = new TimerDataPoint(TagPath.Parse(address), DefaultPollFrequency, NoChannels);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var reported = await verifier.Verify([element], TestContext.Current.CancellationToken);

        // Assert
        var expected = $"RootTagName '{address}' was not found on the controller.";
        reported.Should().ContainSingle()
            .Which.MismatchingConfigurations.Should().ContainSingle()
            .Which.Value.Should().Be(expected);
    }

    private static TimerDataPoint ElementAt(int index) =>
        new(TagPath.Parse(TagAddresses.ArrayElement(TagAddresses.TimerArray, index)), DefaultPollFrequency, NoChannels);
}
