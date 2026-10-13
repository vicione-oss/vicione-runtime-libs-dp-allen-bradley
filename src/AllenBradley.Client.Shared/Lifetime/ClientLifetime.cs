using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Pool;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Lifetime;

/// <summary>
/// Whether one controller client is connected or disposed, and the connect, disconnect and end steps that
/// change it. The client gives the work that is its own: what a connect sends, and how its tag handles are
/// freed.
/// </summary>
internal sealed class ClientLifetime(ILogger logger, ClientAddress address)
{
    private readonly Lock _gate = new();

    private bool _connected;
    private bool _disposed;

    internal bool IsConnected
    {
        get
        {
            lock (_gate)
            {
                return _connected;
            }
        }
    }

    /// <summary>
    /// Runs <paramref name="connect"/> unless the client is connected already. Every way it can fail
    /// arrives as the one exception the framework expects from an acquire.
    /// </summary>
    /// <exception cref="ConnectionFailureException"><paramref name="connect"/> threw.</exception>
    /// <exception cref="ObjectDisposedException">The client has ended.</exception>
    internal async Task ConnectAsync(Func<CancellationToken, Task> connect, CancellationToken cancellationToken)
    {
        // A fast path only: two concurrent connects both run connect, which must therefore be idempotent.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_connected)
            {
                logger.AlreadyConnected(address.ConnectionEndpoint, address.CipRoutePath);
                return;
            }
        }

        try
        {
            await connect(cancellationToken).ConfigureAwait(false);
        }
        // The exclusions are not answers about the device: a cancellation is the caller's own shutdown, and a
        // disposal during the connect is a caller bug.
        catch (Exception ex) when (ex is not (OperationCanceledException or ObjectDisposedException))
        {
            logger.ConnectFailed(ex, address.ConnectionEndpoint, address.CipRoutePath);
            throw new ConnectionFailureException(
                $"Could not connect to the controller at '{address.ConnectionEndpoint}' via CIP route path '{address.CipRoutePath}'.", ex);
        }

        lock (_gate)
        {
            _connected = true;
        }

        logger.Connected(address.ConnectionEndpoint, address.CipRoutePath);
    }

    /// <summary>Runs <paramref name="drain"/> unless the client is disconnected already.</summary>
    /// <exception cref="ObjectDisposedException">The client has ended.</exception>
    internal void Disconnect(Action drain)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_connected)
            {
                logger.AlreadyDisconnected(address.ConnectionEndpoint, address.CipRoutePath);
                return;
            }

            drain();
            _connected = false;
        }

        logger.Disconnected(address.ConnectionEndpoint, address.CipRoutePath);
    }

    /// <summary>Throws unless the client is connected.</summary>
    /// <exception cref="InvalidOperationException">The client is not connected.</exception>
    /// <exception cref="ObjectDisposedException">The client has ended.</exception>
    internal void ThrowIfNotConnected(string message)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_connected)
            {
                throw new InvalidOperationException(message);
            }
        }
    }

    /// <summary>
    /// Ends the client and runs <paramref name="disposeTags"/> on the first call only. A tag handle left to
    /// its finalizer crashes the process at CLR teardown.
    /// </summary>
    internal void End(Action disposeTags)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _connected = false;
        }

        disposeTags();
    }
}
