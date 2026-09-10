using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the engine-shaped configuration a data port is constructed from: a
/// <see cref="LogixCommunication"/> carrying the flat node list the typed node mapper turns into a tree.
/// </summary>
internal static class LogixCommunicationTestDataFactory
{
    internal const string DeviceDesignId = DeviceNode.ControlLogix5X70DesignId;

    internal const string DefaultConnectionEndpoint = "10.0.0.1";

    internal const string DefaultCipRoutePath = "1,0";

    internal const int DefaultMaxPendingMessages = 100_000;

    internal const QueueStrategy DefaultStrategy = QueueStrategy.DropOldest;

    /// <summary>A device carrying <paramref name="nodes"/> exactly as the caller composed them.</summary>
    internal static LogixCommunication CreateCommunicationOf(
        List<Node> nodes, string deviceDesignId = DeviceDesignId) =>
        new()
        {
            DesignId = deviceDesignId,
            ConnectionEndpoint = DefaultConnectionEndpoint,
            CipRoutePath = DefaultCipRoutePath,
            MaxPendingMessages = DefaultMaxPendingMessages,
            Strategy = (byte)DefaultStrategy,
            Nodes = nodes,
        };

    /// <summary>A device with no tags configured under it, and so no container either.</summary>
    internal static LogixCommunication DefaultTestCommunication() => CreateCommunicationOf([]);
}
