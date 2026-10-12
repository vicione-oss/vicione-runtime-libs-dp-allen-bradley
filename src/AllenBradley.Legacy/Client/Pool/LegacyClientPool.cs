using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Pool;

internal static class LegacyClientPool
{
    private static readonly Lock SInitLock = new();
    private static ClientPool<ILegacyClient, LegacyClientInformation>? _sInstance;

    internal static ClientPool<ILegacyClient, LegacyClientInformation> GetInstance(ILoggerFactory loggerFactory)
    {
        lock (SInitLock)
        {
            return _sInstance ??= new ClientPool<ILegacyClient, LegacyClientInformation>(
                loggerFactory.CreateLogger(typeof(LegacyClientPool)),
                new LegacyClientFactory(loggerFactory).Create,
                AddressOf);
        }
    }

    private static ClientAddress AddressOf(LegacyClientInformation clientInformation) =>
        new(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
}
