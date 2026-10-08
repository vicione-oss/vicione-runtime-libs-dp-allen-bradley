using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData;

/// <summary>Builds the device configuration as the engine delivers it, before anything is mapped from it.</summary>
internal static class LegacyCommunicationTestDataFactory
{
    internal const string DefaultConnectionEndpoint = "10.0.0.1";

    /// <summary>An SLC 500 with Ethernet of its own, configured with every other property at its default.</summary>
    internal static LegacyCommunication DefaultTestCommunication() =>
        new()
        {
            DesignId = DeviceNode.Slc500DesignId,
            ConnectionEndpoint = DefaultConnectionEndpoint,
            Nodes = [],
        };
}
