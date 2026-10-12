using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData;

/// <summary>Builds client information the same way the device mapper creates it.</summary>
internal static class LegacyClientTestDataFactory
{
    internal static readonly ConnectionEndpoint DefaultConnectionEndpoint = new("10.0.0.1");
    internal static readonly OperationTimeout DefaultOperationTimeout = new(TimeSpan.FromSeconds(5));

    /// <summary>An SLC 500 with its own Ethernet port, reached on TCP port 44818.</summary>
    internal static LegacyClientInformation DefaultClientInformation() =>
        new(
            LegacyControllerFamily.Slc500,
            DefaultConnectionEndpoint,
            TcpPort.EtherNetIp,
            CipRoutePath.Direct,
            DefaultOperationTimeout);
}
