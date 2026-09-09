using Microsoft.Extensions.Logging;
using NSubstitute;
using ViciOne.ManagedEngine.ExternalCommunication;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client;
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
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ControllerTagsNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ScalarNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;
using static ViciOne.Suite.DataPort.Extensions.Testing.Assertions.EventualAssertions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Outgoing;

/// <summary>
/// Only what the port adds on top of <c>OutgoingDataPortBase</c>; the queue, the retry loop and the
/// backpressure rules are the framework's and are tested with it.
/// </summary>
public sealed class OutgoingDataPortTests : IDisposable
{
    private const string Channel = "MotorSpeed";
    private const string TagName = "MotorSpeed";

    /// <summary>The value the queue-overrun cases jam the head of the queue with.</summary>
    private const int FirstValue = 1;

    /// <summary>Controller scope on the device, the container the single configured tag hangs off.</summary>
    private static readonly Node ControllerTagsNode = CreateControllerTagsNodeFor();

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
        ResolveEveryTagAs(dataPoint => DefaultAtomicTagDefinition() with { TagName = dataPoint.TagName });

        _lifecycleManager
            .AcquireConnectedAsync(Arg.Any<LogixClientInformation>(), Arg.Any<CancellationToken>())
            .Returns(_client);
    }

    [Fact]
    public async Task ThePortsClientInformationIsTheOneItsDeviceNodeCarries()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());

        // Act
        var clientInformation = outgoing.ClientInformation;

        // Assert
        clientInformation.ConnectionEndpoint.Value.Should().Be(DefaultConnectionEndpoint);
    }

    [Fact]
    public async Task AConnectAcquiresTheClientForItsOwnController()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());

        // Act
        await outgoing.ConnectAsync(CancellationToken.None);

        // Assert
        await _lifecycleManager.Received(1)
            .AcquireConnectedAsync(outgoing.ClientInformation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ADisconnectReleasesTheClientItAcquired()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.DisconnectAsync(CancellationToken.None);

        // Assert
        await _lifecycleManager.Received(1)
            .ReleaseAsync(Arg.Any<LogixClientInformation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AConfiguredTagMissingFromTheControllerFailsTheConnect()
    {
        // Arrange
        ResolveEveryTagAs(static _ => null);
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());

        // Act
        // ReSharper disable once AccessToDisposedClosure — the assertion below invokes it in scope
        var connecting = outgoing.Awaiting(port => port.ConnectAsync(CancellationToken.None));

        // Assert
        await connecting.Should().ThrowAsync<InvalidConfigurationException>();
    }

    [Fact]
    public async Task AValueOnAConfiguredChannelIsWrittenAsThatTagsTypedValue()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(1, [ExternalValue(Channel, 42)], CancellationToken.None);

        // Assert
        Eventually(() => WrittenValues.Count).Should().Be(1, "the queue processor should have written once");
        var written = WrittenValues[0].Should().ContainSingle().Subject;
        written.DataPoint.Should().BeOfType<DIntDataPoint>().Which.TagName.Value.Should().Be(TagName);
        written.Value.Should().Be(42);
    }

    [Fact]
    public async Task AValueThatCannotBecomeTheTagsTypeIsDropped()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        // A DINT carries an int, and "not a number" cannot become one.
        await outgoing.SendAsync(1, [ExternalValue(Channel, "not a number")], CancellationToken.None);

        // Assert
        await NothingIsWritten(outgoing);
    }

    [Fact]
    public async Task AValueOnAnUnconfiguredChannelIsSkippedWithoutFailing()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        // ReSharper disable once AccessToDisposedClosure — Record invokes it before it returns
        var send = await Record.ExceptionAsync(
            () => outgoing.SendAsync(1, [ExternalValue("NoSuchChannel", 42)], CancellationToken.None));

        // Assert
        send.Should().BeNull();
        await NothingIsWritten(outgoing);
    }

    [Fact]
    public async Task AWriteThatFailsTwiceIsRetriedUntilItLands()
    {
        // Arrange
        var attempts = 0;
        _client.WriteAsync(Arg.Any<IReadOnlyList<ILogixDataPointValue>>(), Arg.Any<CancellationToken>())
            .Returns(_ => ++attempts < 3
                ? throw new LogixTagException("The controller refused the write.")
                : ValueTask.CompletedTask);
        await using var outgoing = CreatePort(DeviceWithOneConfiguredTag());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(1, [ExternalValue(Channel, 42)], CancellationToken.None);

        // Assert
        Eventually(() => attempts).Should().Be(3, "the infinite retry policy should not give up on two failures");
    }

    [Fact]
    public async Task AQueueOverrunUnderDropOldestLetsNewerValuesPastTheStuckHead()
    {
        // Arrange
        await using var outgoing = CreatePort(SmallQueue(QueueStrategy.DropOldest));
        FailOnlyTheFirstValue();
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await SendStuckHeadThenOverrunTheQueue(outgoing);

        // Assert
        Eventually(() => WrittenValues.Any(batch => Payload(batch) != FirstValue)).Should()
            .BeTrue("DropOldest displaces the batch that cannot be written, so a newer one gets through");
    }

    [Fact]
    public async Task AQueueOverrunUnderDropNewestLeavesTheStuckHeadInPlace()
    {
        // Arrange
        await using var outgoing = CreatePort(SmallQueue(QueueStrategy.DropNewest));
        FailOnlyTheFirstValue();
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        // A bounded wait is the only way to observe that nothing overtook the head.
        await SendStuckHeadThenOverrunTheQueue(outgoing);
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert
        WrittenValues.Should().OnlyContain(batch => Payload(batch) == FirstValue,
            "DropNewest preserves write order, so nothing overtakes a head that will not land");
    }

    [Fact]
    public async Task APortDisposedTwiceReleasesItsClientOnce()
    {
        // Arrange
        var outgoing = CreatePort(DeviceWithOneConfiguredTag());
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

    private static LogixCommunication DeviceWithOneConfiguredTag() =>
        CreateCommunicationOf([ControllerTagsNode, CreateDIntNode(Channel, TagName, ControllerTagsNode.Id)]);

    /// <summary>A device whose queue is small enough that a handful of sends overruns it.</summary>
    private static LogixCommunication SmallQueue(QueueStrategy strategy) =>
        DeviceWithOneConfiguredTag() with { MaxPendingMessages = 2, Strategy = (byte)strategy };

    private static ExternalValue ExternalValue(string channel, object value) =>
        new() { Channel = channel, Value = value, Validity = 1, Timestamp = DateTime.UtcNow };

    private static int Payload(IReadOnlyList<ILogixDataPointValue> batch) => (int)batch[0].Value!;

    private static async Task SendStuckHeadThenOverrunTheQueue(OutgoingDataPort outgoing)
    {
        await outgoing.SendAsync(1, [ExternalValue(Channel, FirstValue)], CancellationToken.None);

        // The processor has to have picked the first batch up before the rest arrive, or there is no
        // stuck head for them to pile up behind.
        await Task.Delay(25, TestContext.Current.CancellationToken);

        for (var value = 2; value <= 8; value++)
        {
            await outgoing.SendAsync((ulong)value, [ExternalValue(Channel, value)], CancellationToken.None);
        }
    }

    private OutgoingDataPort CreatePort(LogixCommunication communication) =>
        new(communication, _loggerFactory, _lifecycleManager, new InstantDelayProvider());

    /// <summary>Every batch the client was handed, in the order the queue processor wrote them.</summary>
    private List<IReadOnlyList<ILogixDataPointValue>> WrittenValues =>
    [
        .. _client.ReceivedCalls()
            .Where(static call => call.GetMethodInfo().Name == nameof(ILogixClient.WriteAsync))
            .Select(static call => (IReadOnlyList<ILogixDataPointValue>)call.GetArguments()[0]!),
    ];

    // A drop is the absence of a write, and the disconnect is what makes the absence worth asserting: it
    // stops the queue processor, so anything that was going to be written has been by the time it returns.
    private async Task NothingIsWritten(OutgoingDataPort outgoing)
    {
        await outgoing.DisconnectAsync(CancellationToken.None);
        await _client.DidNotReceive()
            .WriteAsync(Arg.Any<IReadOnlyList<ILogixDataPointValue>>(), Arg.Any<CancellationToken>());
    }

    // Only the head batch fails, and it fails forever: under the infinite retry policy that leaves it at
    // the head of the queue, which is the state a queue has to overrun for its strategy to show.
    private void FailOnlyTheFirstValue() =>
        _client.WriteAsync(Arg.Any<IReadOnlyList<ILogixDataPointValue>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
                Payload(callInfo.Arg<IReadOnlyList<ILogixDataPointValue>>()) == FirstValue
                    ? throw new LogixTagException("The controller will not take this one.")
                    : ValueTask.CompletedTask);

    private void ResolveEveryTagAs(Func<ILogixDataPoint, TagDefinition?> definitionOf) =>
        _client.ResolveDataPoints(Arg.Any<IReadOnlyList<ILogixDataPoint>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<IReadOnlyList<ResolvedDataPoint>>(
            [
                .. callInfo.Arg<IReadOnlyList<ILogixDataPoint>>()
                    .Select(dataPoint => new ResolvedDataPoint(dataPoint, definitionOf(dataPoint))),
            ]));
}
