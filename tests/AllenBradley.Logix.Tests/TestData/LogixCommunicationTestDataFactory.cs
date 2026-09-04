using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Outgoing.QueueProcessing;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Builds the engine-shaped configuration a data port is constructed from: a
/// <see cref="LogixCommunication"/> carrying the flat node list the typed node mapper turns into a tree.
/// The nodes themselves come from the per-node factories, and the tree they form is composed by the test.
/// </summary>
internal static class LogixCommunicationTestDataFactory
{
    /// <summary>The device node type every configuration the factory makes is configured under.</summary>
    internal const string DeviceDesignId = DeviceNode.ControlLogix5X70DesignId;

    /// <summary>The controller every configuration the factory makes points at.</summary>
    internal const string DefaultConnectionEndpoint = "10.0.0.1";

    /// <summary>The hops to the CPU every configuration the factory makes routes over.</summary>
    internal const string DefaultCipRoutePath = "1,0";

    /// <summary>The outgoing queue depth every configuration the factory makes is given.</summary>
    internal const int DefaultMaxPendingMessages = 100_000;

    /// <summary>What every configuration the factory makes does when its outgoing queue is full.</summary>
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
