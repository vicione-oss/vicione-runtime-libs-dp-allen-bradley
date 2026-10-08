using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// One controller connection with the read seam on it, plus the connect / disconnect / dispose lifecycle a
/// client lifecycle manager drives it through.
/// </summary>
public interface ILegacyClient : ILegacyReadClient, IDisposable
{
    /// <summary>Whether the client has connected and not since disconnected.</summary>
    bool IsConnected { get; }

    /// <summary>Opens the connection to the controller. A no-op when already connected.</summary>
    /// <exception cref="ConnectionFailureException">The controller could not be reached.</exception>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>Closes the connection to the controller. A no-op when already disconnected.</summary>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task DisconnectAsync(CancellationToken cancellationToken);
}
