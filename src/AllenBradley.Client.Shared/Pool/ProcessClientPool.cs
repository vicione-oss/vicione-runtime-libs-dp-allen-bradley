using ViciOne.Suite.DataPort.Extensions.Client;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;

/// <summary>
/// The one <see cref="ClientPool{TClient, TKey}"/> for each client type that every dataport in the process
/// shares, so both dataports for a controller share its connection.
/// </summary>
internal static class ProcessClientPool<TClient, TKey>
    where TClient : class, IConnectableClient
    where TKey : IClientInformation
{
    private static readonly Lock SInitLock = new();
    private static ClientPool<TClient, TKey>? _sInstance;

    /// <summary>The pool every dataport shares. Only the first caller's <paramref name="create"/> runs.</summary>
    internal static ClientPool<TClient, TKey> GetInstance(Func<ClientPool<TClient, TKey>> create)
    {
        lock (SInitLock)
        {
            return _sInstance ??= create();
        }
    }
}
