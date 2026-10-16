using Microsoft.Extensions.Logging;
using NSubstitute;
using ViciOne.ManagedEngine.ExternalCommunication;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.StringFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Outgoing;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;
using ViciOne.Suite.DataPort.Extensions.Testing.Assertions;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerFileNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.Extensions.Testing.Assertions.EventualAssertions;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Outgoing;

/// <summary>
/// Only what the port adds on top of <c>OutgoingDataPortBase</c>; the queue, the retry loop and the
/// backpressure rules are the framework's and are tested with it.
/// </summary>
public sealed class OutgoingDataPortTests : IDisposable
{
    private const ushort ElementNumber = 3;
    private const int PollFrequencyMilliseconds = 100;

    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ILegacyClient _client = Substitute.For<ILegacyClient>();

    private readonly IClientLifecycleManager<ILegacyClient, LegacyClientInformation> _lifecycleManager =
        Substitute.For<IClientLifecycleManager<ILegacyClient, LegacyClientInformation>>();

    public OutgoingDataPortTests()
    {
        _client.WriteAsync(Arg.Any<IReadOnlyList<ILegacyDataPointValue>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        _lifecycleManager
            .AcquireConnectedAsync(Arg.Any<LegacyClientInformation>(), Arg.Any<CancellationToken>())
            .Returns(_client);
    }

    [Fact]
    public async Task AValueOnAConfiguredElementsChannelIsWrittenToThatElement()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneIntegerElement());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(1, [ExternalValue(ElementChannel(ElementNumber), (short)42)], CancellationToken.None);

        // Assert
        Eventually(() => WrittenValues.Count).Should().Be(1, "the queue processor should have written once");
        var written = WrittenValues[0].Should().ContainSingle().Subject;
        var expectedAddress = new DataFileAddress(
            DataFileType.Integer, new FileNumber(DefaultIntegerFileNumber), new ElementNumber(ElementNumber));
        written.DataPoint.Address.Should().Be(expectedAddress);
        written.Value.Should().Be((short)42);
    }

    [Fact]
    public async Task AValueThatCannotBecomeTheElementsTypeIsDropped()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneIntegerElement());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(
            1, [ExternalValue(ElementChannel(ElementNumber), "not a number")], CancellationToken.None);

        // Assert
        await NothingIsWritten(outgoing);
    }

    [Fact]
    public async Task AStringOf83CharactersIsRefusedAndNothingIsWritten()
    {
        // Arrange
        await using var outgoing = CreatePort(DeviceWithOneStringElement());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(
            1, [ExternalValue(ElementChannel(ElementNumber), new string('x', 83))], CancellationToken.None);

        // Assert
        await NothingIsWritten(outgoing);
    }

    [Fact]
    public async Task AWriteThatFailsTwiceIsRetriedUntilItLands()
    {
        // Arrange
        var attempts = 0;
        _client.WriteAsync(Arg.Any<IReadOnlyList<ILegacyDataPointValue>>(), Arg.Any<CancellationToken>())
            .Returns(_ => ++attempts < 3
                ? throw new ConnectionFailureException("The controller did not answer.")
                : ValueTask.CompletedTask);
        await using var outgoing = CreatePort(DeviceWithOneIntegerElement());
        await outgoing.ConnectAsync(CancellationToken.None);

        // Act
        await outgoing.SendAsync(1, [ExternalValue(ElementChannel(ElementNumber), (short)42)], CancellationToken.None);

        // Assert
        Eventually(() => attempts).Should().Be(3, "the infinite retry policy should not give up on two failures");
    }

    public void Dispose()
    {
        _client.Dispose();
        _loggerFactory.Dispose();
    }

    private static LegacyCommunication DeviceWithOneIntegerElement()
    {
        var integerFile = IntegerFile();
        return DefaultTestCommunication() with
        {
            Nodes = [integerFile, IntegerElement(integerFile.Id, ElementNumber, PollFrequencyMilliseconds)],
        };
    }

    private static LegacyCommunication DeviceWithOneStringElement()
    {
        var stringFile = new Node
        {
            DesignId = StringFileNode.LinkedNodeTypeId,
            Name = StringFileNode.LinkedNodeTypeId,
            Id = Guid.NewGuid(),
            Properties = new Dictionary<string, Property>
            {
                [DataFileNode.FileNumberPropertyName] = new() { Value = (ushort)9 },
            },
        };
        var stringElement = new Node
        {
            DesignId = StringNode.LinkedNodeTypeId,
            Name = ElementChannel(ElementNumber),
            Id = Guid.NewGuid(),
            ParentId = stringFile.Id,
            AffectedChannels = [ElementChannel(ElementNumber)],
            TransferredChannels = [ElementChannel(ElementNumber)],
            Properties = new Dictionary<string, Property>
            {
                [ILegacyDataPointNode.ElementNumberPropertyName] = new() { Value = ElementNumber },
                [ILegacyDataPointNode.PollFrequencyPropertyName] = new() { Value = PollFrequencyMilliseconds },
            },
        };
        return DefaultTestCommunication() with { Nodes = [stringFile, stringElement] };
    }

    private static ExternalValue ExternalValue(string channel, object value) =>
        new() { Channel = channel, Value = value, Validity = 1, Timestamp = DateTime.UtcNow };

    private OutgoingDataPort CreatePort(LegacyCommunication communication) =>
        new(communication, _loggerFactory, _lifecycleManager, new InstantDelayProvider());

    private List<IReadOnlyList<ILegacyDataPointValue>> WrittenValues =>
    [
        .. _client.ReceivedCalls()
            .Where(static call => call.GetMethodInfo().Name == nameof(ILegacyClient.WriteAsync))
            .Select(static call => (IReadOnlyList<ILegacyDataPointValue>)call.GetArguments()[0]!),
    ];

    // The disconnect stops the queue processor, so any write that was going to happen already has.
    private async Task NothingIsWritten(OutgoingDataPort outgoing)
    {
        await outgoing.DisconnectAsync(CancellationToken.None);
        await _client.DidNotReceive()
            .WriteAsync(Arg.Any<IReadOnlyList<ILegacyDataPointValue>>(), Arg.Any<CancellationToken>());
    }
}
