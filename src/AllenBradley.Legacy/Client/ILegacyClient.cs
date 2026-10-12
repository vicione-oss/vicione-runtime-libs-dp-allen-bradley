using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// One controller connection with both directions on it: the read and write seams, plus the connect /
/// disconnect / dispose lifecycle a client lifecycle manager drives them through.
/// </summary>
public interface ILegacyClient : ILegacyReadClient, ILegacyWriteClient, IConnectableClient
{
    /// <summary>Whether the client has connected and not since disconnected.</summary>
    bool IsConnected { get; }
}
