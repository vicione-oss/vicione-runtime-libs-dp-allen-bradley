using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// One controller connection with both directions on it: the read and write seams, plus the connect /
/// disconnect / dispose lifecycle a client lifecycle manager drives them through.
/// </summary>
public interface ILogixClient : ILogixReadClient, ILogixWriteClient, IDisposable
{
    /// <summary>
    /// Whether a schema has been browsed and not since dropped. Not a transport check: libplctag exposes
    /// no connection status, so this reports what the client did, not what the network is doing.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Browses the controller's symbol table, which reads and writes resolve their tags against. A no-op
    /// when already connected.
    /// </summary>
    /// <exception cref="ConnectionFailureException">The controller could not be browsed.</exception>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Frees every libplctag handle this client created and drops the schema. A no-op when already
    /// disconnected.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task DisconnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Pairs every data point with what the controller's symbol table reports for its tag, in the order
    /// they were asked for. Resolves in memory against the browsed schema; a tag the controller does not
    /// have comes back with where the lookup stopped, as its <see cref="ResolvedDataPoint.Declaration"/>, rather
    /// than being left out.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The client is not connected, so there is no symbol table to resolve against.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task<IReadOnlyList<ResolvedDataPoint>> ResolveDataPoints(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken);
}
