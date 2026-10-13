using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Pool;

/// <summary>The one client pool that every Legacy dataport in the process shares.</summary>
internal static class LegacyClientPool
{
    /// <summary>The first caller's <paramref name="loggerFactory"/> wins.</summary>
    internal static ClientPool<ILegacyClient, LegacyClientInformation> GetInstance(ILoggerFactory loggerFactory) =>
        ProcessClientPool<ILegacyClient, LegacyClientInformation>.GetInstance(() =>
            new ClientPool<ILegacyClient, LegacyClientInformation>(
                loggerFactory.CreateLogger(typeof(LegacyClientPool)),
                new LegacyClientFactory(loggerFactory).Create,
                AddressOf));

    private static ClientAddress AddressOf(LegacyClientInformation clientInformation) =>
        new(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
}
