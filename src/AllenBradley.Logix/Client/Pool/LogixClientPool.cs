using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

/// <summary>The one client pool that every Logix dataport in the process shares.</summary>
internal static class LogixClientPool
{
    /// <summary>The first caller's <paramref name="loggerFactory"/> wins.</summary>
    internal static ClientPool<ILogixClient, LogixClientInformation> GetInstance(ILoggerFactory loggerFactory) =>
        ProcessClientPool<ILogixClient, LogixClientInformation>.GetInstance(() =>
            new ClientPool<ILogixClient, LogixClientInformation>(
                loggerFactory.CreateLogger(typeof(LogixClientPool)),
                new LogixClientFactory(loggerFactory).Create,
                AddressOf));

    private static ClientAddress AddressOf(LogixClientInformation clientInformation) =>
        new(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
}
