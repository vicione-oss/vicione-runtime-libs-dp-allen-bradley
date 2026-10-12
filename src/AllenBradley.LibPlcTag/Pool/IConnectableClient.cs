namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Pool;

/// <summary>
/// A controller client that <see cref="ClientPool{TClient, TKey}"/> can connect, disconnect and dispose.
/// Public, because the public client interface of each dataport extends it.
/// </summary>
public interface IConnectableClient : IDisposable
{
    /// <summary>Connects to the controller. A no-op when already connected.</summary>
    /// <exception cref="ViciOne.Suite.DataPort.Extensions.Exceptions.ConnectionFailureException">
    /// The controller could not be reached.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Frees every libplctag handle the client created. A no-op when already disconnected.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task DisconnectAsync(CancellationToken cancellationToken);
}
