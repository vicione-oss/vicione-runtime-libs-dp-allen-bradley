using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Incoming;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Client;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerFileNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.IntegerNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.Extensions.Testing.Assertions.EventualAssertions;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Incoming;

public sealed class IncomingDataPortTests
{
    private const int FastPollFrequencyMilliseconds = 100;
    private const int SlowPollFrequencyMilliseconds = 200;

    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ILegacyClient _client = Substitute.For<ILegacyClient>();

    private readonly IClientLifecycleManager<ILegacyClient, LegacyClientInformation> _lifecycleManager =
        Substitute.For<IClientLifecycleManager<ILegacyClient, LegacyClientInformation>>();

    private readonly ConcurrentQueue<LegacyDataPointGroup> _readGroups = new();

    public IncomingDataPortTests()
    {
        _client.ReadAsync(Arg.Any<LegacyDataPointGroup>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                _readGroups.Enqueue(call.Arg<LegacyDataPointGroup>());
                return ValueTask.FromResult<IReadOnlyList<ILegacyDataPointValue>>([]);
            });
        _lifecycleManager
            .AcquireConnectedAsync(Arg.Any<LegacyClientInformation>(), Arg.Any<CancellationToken>())
            .Returns(_client);
    }

    [Fact]
    public async Task AConfiguredDeviceIsGroupedByPollFrequency()
    {
        // Arrange

        // Act
        await using var incoming = CreatePort(DeviceWithElementsPolledAt(
            FastPollFrequencyMilliseconds, FastPollFrequencyMilliseconds, SlowPollFrequencyMilliseconds));

        // Assert
        var expected = new[]
        {
            (PollFrequency.FromMilliseconds(FastPollFrequencyMilliseconds), 2),
            (PollFrequency.FromMilliseconds(SlowPollFrequencyMilliseconds), 1),
        };
        incoming.DataPointGroups.Select(group => (group.PollFrequency, group.DataPoints.Count))
            .Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AConnectedPortReadsEveryGroupThroughTheLegacyClient()
    {
        // Arrange
        await using var incoming = CreatePort(DeviceWithElementsPolledAt(
            FastPollFrequencyMilliseconds, SlowPollFrequencyMilliseconds));

        // Act
        await incoming.ConnectAsync(CancellationToken.None);

        // Assert
        var expected = new[]
        {
            PollFrequency.FromMilliseconds(FastPollFrequencyMilliseconds),
            PollFrequency.FromMilliseconds(SlowPollFrequencyMilliseconds),
        };
        Eventually(() => expected.All(pollFrequency => _readGroups.Any(group => group.PollFrequency == pollFrequency)))
            .Should().Be(true, "every poll frequency should have had its group read");
    }

    private IncomingDataPort CreatePort(LegacyCommunication communication) =>
        new(communication, _loggerFactory, TimeProvider.System, _lifecycleManager);

    private static LegacyCommunication DeviceWithElementsPolledAt(params int[] pollFrequenciesInMilliseconds)
    {
        var integerFile = IntegerFile();
        var elements = pollFrequenciesInMilliseconds.Select((pollFrequency, element) =>
            IntegerElement(integerFile.Id, (ushort)element, pollFrequency));

        return DefaultTestCommunication() with { Nodes = [integerFile, .. elements] };
    }
}
