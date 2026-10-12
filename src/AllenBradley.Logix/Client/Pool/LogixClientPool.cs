using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

/// <summary>
/// The one <see cref="ClientPool{TClient, TKey}"/> every Logix dataport in the process shares, so both
/// dataports for a controller share its connection.
/// </summary>
internal static class LogixClientPool
{
    private static readonly Lock SInitLock = new();
    private static ClientPool<ILogixClient, LogixClientInformation>? _sInstance;

    /// <summary>The pool every dataport shares. The first caller's <paramref name="loggerFactory"/> wins.</summary>
    internal static ClientPool<ILogixClient, LogixClientInformation> GetInstance(ILoggerFactory loggerFactory)
    {
        lock (SInitLock)
        {
            return _sInstance ??= new ClientPool<ILogixClient, LogixClientInformation>(
                loggerFactory.CreateLogger(typeof(LogixClientPool)),
                new LogixClientFactory(loggerFactory).Create,
                AddressOf);
        }
    }

    private static ClientAddress AddressOf(LogixClientInformation clientInformation) =>
        new(clientInformation.ConnectionEndpoint.Value, clientInformation.CipRoutePath.Value);
}
