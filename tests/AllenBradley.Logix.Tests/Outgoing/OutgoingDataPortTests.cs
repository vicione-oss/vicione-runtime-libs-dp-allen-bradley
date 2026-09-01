using Microsoft.Extensions.Logging;
using NSubstitute;
using ViciOne.ManagedEngine.ExternalCommunication;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Outgoing;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using ViciOne.Suite.DataPort.Extensions.Testing.Assertions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.Extensions.Testing.Assertions.EventualAssertions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Outgoing;

/// <summary>
/// What the outgoing port adds on top of <c>OutgoingDataPortBase</c>: the client it acquires, the
/// conversion gate a value crosses on its way to the queue, and the verifier a connect runs. The queue,
/// the retry loop and the backpressure rules are the framework's and are tested with it.
/// </summary>
/// <remarks>
/// Every write assertion goes through <c>Eventually</c> because <c>SendAsync</c> enqueues and returns —
/// the write happens on the queue processor. The delay provider is instant so a retry costs no wall
/// clock.
/// </remarks>
public sealed class OutgoingDataPortTests : IDisposable
{
    private const string Channel = "MotorSpeed";
    private const string TagName = "MotorSpeed";

    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ILogixClient _client = Substitute.For<ILogixClient>();
    private readonly IClientLifecycleManager<ILogixClient, LogixClientInformation> _lifecycleManager =
        Substitute.For<IClientLifecycleManager<ILogixClient, LogixClientInformation>>();

    public OutgoingDataPortTests()
    {
        _client.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _client.DisconnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _client.WriteAsync(Arg.Any<IReadOnlyList<ILogixDataPointValue>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        ResolvesEveryTagAsDInt();

        _lifecycleManager
            .AcquireConnectedAsync(Arg.Any<LogixClientInformation>(), Arg.Any<CancellationToken>())
            .Returns(_client);
    }

    [Fact]
    public async Task ClientInformation_IsTheDeviceNodesOwn()
    {
        // Arrange
        await using var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));

        // Act
        var clientInformation = outgoing.ClientInformation;

        // Assert
        clientInformation.ConnectionEndpoint.Value.Should().Be(DefaultConnectionEndpoint);
    }

    [Fact]
    public async Task ConnectAsync_AcquiresTheClientForItsController()
    {
        // Arrange
        await using var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));

        // Act
        await outgoing.ConnectAsync(CancellationToken.None);

        // Assert
        await _lifecycleManager.Received(1)
            .AcquireConnectedAsync(outgoing.ClientInformation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DisconnectAsync_ReleasesTheClient()
    {
        // Arrange
        await using var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.DisconnectAsync(CancellationToken.None);

        // Assert
        await _lifecycleManager.Received(1)
            .ReleaseAsync(Arg.Any<LogixClientInformation>(), Arg.Any<CancellationToken>());
    }

    /// <remarks>
    /// The verifier the port creates is the incoming port's, so a tag the controller declares as
    /// something else fails the connect here too rather than becoming a write that retries forever.
    /// </remarks>
    [Fact]
    public async Task ConnectAsync_TagMissingFromTheController_FailsTheConnect()
    {
        // Arrange
        var communication = CommunicationWithSingleDInt(Channel, TagName);
        _client.ResolveDataPoints(Arg.Any<IReadOnlyList<ILogixDataPoint>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<IReadOnlyList<ResolvedDataPoint>>(
            [
                .. callInfo.Arg<IReadOnlyList<ILogixDataPoint>>()
                    .Select(static dataPoint => new ResolvedDataPoint(dataPoint, TagDefinition: null)),
            ]));

        await using var outgoing = CreatePort(communication);

        // Act
        var connect = () => outgoing.ConnectAsync(CancellationToken.None);

        // Assert
        await connect.Should().ThrowAsync<InvalidConfigurationException>();
    }

    [Fact]
    public async Task SendAsync_ValueOnAConfiguredChannel_WritesItAsThatTagsTypedValue()
    {
        // Arrange
        await using var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(1, [ExternalValue(Channel, 42)], CancellationToken.None);

        // Assert
        Eventually(() => WrittenValues.Count).Should().Be(1, "the queue processor should have written once");
        var written = WrittenValues[0].Should().ContainSingle().Subject;
        written.DataPoint.Should().BeOfType<DIntDataPoint>()
            .Which.TagName.Value.Should().Be(TagName);
        written.Value.Should().Be(42);
        written.Quality.Should().Be(LogixQuality.Good);
    }

    [Fact]
    public async Task SendAsync_ValueOfTheWrongType_DropsTheBatch()
    {
        // Arrange
        await using var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act — a DINT carries an int, and "not a number" cannot become one
        await outgoing.SendAsync(1, [ExternalValue(Channel, "not a number")], CancellationToken.None);

        // Assert
        await NothingIsWritten(outgoing);
    }

    /// <remarks>
    /// One unmatched channel is a configuration concern, not a threat to controller state, so the
    /// framework skips it and keeps going — unlike a conversion failure, which drops the batch.
    /// </remarks>
    [Fact]
    public async Task SendAsync_ValueOnAnUnconfiguredChannel_IsSkipped()
    {
        // Arrange
        await using var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        var send = () => outgoing.SendAsync(1, [ExternalValue("NoSuchChannel", 42)], CancellationToken.None);

        // Assert
        await send.Should().NotThrowAsync();
        await NothingIsWritten(outgoing);
    }

    [Fact]
    public async Task SendAsync_WriteFailsTwice_KeepsRetryingUntilItLands()
    {
        // Arrange
        var attempts = 0;
        _client.WriteAsync(Arg.Any<IReadOnlyList<ILogixDataPointValue>>(), Arg.Any<CancellationToken>())
            .Returns(_ => ++attempts < 3
                ? throw new LogixTagException("The controller refused the write.")
                : ValueTask.CompletedTask);

        await using var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(1, [ExternalValue(Channel, 42)], CancellationToken.None);

        // Assert
        Eventually(() => attempts).Should().Be(3, "the infinite retry policy should not give up on two failures");
    }

    /// <remarks>
    /// The pair below is what holds <c>MaxPendingMessages</c> and <c>Strategy</c> to the queue the port
    /// builds from them. Both jam the head batch in retry and then overrun a two-slot queue; they differ
    /// only in the strategy, and so does their outcome. Neither would fail if the size were left at its
    /// hundred-thousand default, because a queue that never fills has no strategy to apply.
    /// </remarks>
    [Fact]
    public async Task SendAsync_WhenTheQueueOverrunsUnderDropOldest_TheStuckHeadGivesWayToNewerValues()
    {
        // Arrange
        await using var outgoing = CreatePort(SmallQueue(QueueStrategy.DropOldest));
        FailsOnlyTheFirstValue();
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await SendStuckHeadThenOverrunTheQueue(outgoing);

        // Assert
        Eventually(() => WrittenValues.Any(batch => Payload(batch) != FirstValue)).Should()
            .BeTrue("DropOldest displaces the batch that cannot be written, so a newer one gets through");
    }

    [Fact]
    public async Task SendAsync_WhenTheQueueOverrunsUnderDropNewest_TheStuckHeadHoldsItsPlace()
    {
        // Arrange
        await using var outgoing = CreatePort(SmallQueue(QueueStrategy.DropNewest));
        FailsOnlyTheFirstValue();
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await SendStuckHeadThenOverrunTheQueue(outgoing);
        await Task.Delay(100);

        // Assert
        WrittenValues.Should().OnlyContain(batch => Payload(batch) == FirstValue,
            "DropNewest preserves write order, so nothing overtakes a head that will not land");
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_ReleasesTheClientOnce()
    {
        // Arrange
        var outgoing = CreatePort(CommunicationWithSingleDInt(Channel, TagName));
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.DisposeAsync();
        await outgoing.DisposeAsync();

        // Assert
        await _lifecycleManager.Received(1)
            .ReleaseAsync(Arg.Any<LogixClientInformation>(), Arg.Any<CancellationToken>());
    }

    public void Dispose()
    {
        _client.Dispose();
        _loggerFactory.Dispose();
    }

    private OutgoingDataPort CreatePort(LogixCommunication communication) =>
        new(communication, _loggerFactory, _lifecycleManager, new InstantDelayProvider());

    private static ExternalValue ExternalValue(string channel, object value) =>
        new() { Channel = channel, Value = value, Validity = 1, Timestamp = DateTime.UtcNow };

    /// <summary>Every batch the client was handed, in the order the queue processor wrote them.</summary>
    private List<IReadOnlyList<ILogixDataPointValue>> WrittenValues =>
    [
        .. _client.ReceivedCalls()
            .Where(static call => call.GetMethodInfo().Name == nameof(ILogixClient.WriteAsync))
            .Select(static call => (IReadOnlyList<ILogixDataPointValue>)call.GetArguments()[0]!),
    ];

    // A drop is the absence of a write, and absence needs a moment to be worth asserting. Disconnecting
    // first is what supplies it: it stops the queue processor, so anything that was going to be written
    // has been by the time it returns.
    private async Task NothingIsWritten(OutgoingDataPort outgoing)
    {
        await outgoing.DisconnectAsync(CancellationToken.None);
        await _client.DidNotReceive()
            .WriteAsync(Arg.Any<IReadOnlyList<ILogixDataPointValue>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>The value the queue-overrun cases jam the head of the queue with.</summary>
    private const int FirstValue = 1;

    /// <summary>A device whose queue is small enough that a handful of sends overruns it.</summary>
    private static LogixCommunication SmallQueue(QueueStrategy strategy) =>
        CreateCommunication([CreateDIntNode(Channel, TagName)], maxPendingMessages: 2, strategy);

    private static int Payload(IReadOnlyList<ILogixDataPointValue> batch) => (int)batch[0].Value!;

    // Only the head batch fails, and it fails forever: under the infinite retry policy that leaves it at
    // the head of the queue, which is the state a queue has to overrun for its strategy to show.
    private void FailsOnlyTheFirstValue() =>
        _client.WriteAsync(Arg.Any<IReadOnlyList<ILogixDataPointValue>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
                Payload(callInfo.Arg<IReadOnlyList<ILogixDataPointValue>>()) == FirstValue
                    ? throw new LogixTagException("The controller will not take this one.")
                    : ValueTask.CompletedTask);

    private static async Task SendStuckHeadThenOverrunTheQueue(OutgoingDataPort outgoing)
    {
        await outgoing.SendAsync(1, [ExternalValue(Channel, FirstValue)], CancellationToken.None);

        // The processor has to have picked the first batch up before the rest arrive, or there is no
        // stuck head for them to pile up behind.
        await Task.Delay(25);

        for (var value = 2; value <= 8; value++)
        {
            await outgoing.SendAsync((ulong)value, [ExternalValue(Channel, value)], CancellationToken.None);
        }
    }

    // The controller agrees with every configuration under test unless a case says otherwise, so a
    // connect gets past verification and the test can be about the write path.
    private void ResolvesEveryTagAsDInt() =>
        _client.ResolveDataPoints(Arg.Any<IReadOnlyList<ILogixDataPoint>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<IReadOnlyList<ResolvedDataPoint>>(
            [
                .. callInfo.Arg<IReadOnlyList<ILogixDataPoint>>().Select(static dataPoint =>
                    new ResolvedDataPoint(
                        dataPoint,
                        new TagDefinition(
                            dataPoint.TagName,
                            LogixTypeKind.Atomic,
                            AllenBradleyDataType.Dint,
                            MaxLength: null,
                            DimensionCount.Scalar,
                            new ElementCount(1)))),
            ]));
}
