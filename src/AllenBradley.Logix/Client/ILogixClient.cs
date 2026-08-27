using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// One controller connection with both directions on it: the read and write seams, plus the connect /
/// disconnect / dispose lifecycle a client lifecycle manager drives them through. This is the type
/// <c>LogixClientPool</c> pools, and the type either dataport ends up holding.
/// </summary>
/// <remarks>
/// <para>
/// <b>Connecting means browsing the symbol table.</b> libplctag opens no socket until a handle is first
/// read, so there is no transport for a connect to open. What it does establish is the controller
/// metadata every later read is gated on — and a browse that comes back is also the proof the
/// controller is reachable and speaking CIP, which is the answer a connect is asked for.
/// </para>
/// <para>
/// <b>Disconnect is reversible, dispose is terminal.</b> A disconnected client has freed every libplctag
/// handle and dropped the schema; a later connect browses again. A disposed one is finished. Both are
/// idempotent, so a caller can drive them defensively.
/// </para>
/// </remarks>
public interface ILogixClient : ILogixReadClient, ILogixWriteClient, IDisposable
{
    /// <summary>
    /// Whether a schema has been browsed and not since dropped. Not a transport check: libplctag exposes
    /// no connection status and holds no socket to ask about, so this reports what the client did, not
    /// what the network is doing.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Browses the controller's symbol table, which is what makes the client usable: reads and writes
    /// resolve their tags against it. A no-op when already connected.
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
    /// they were asked for. This is the seam configuration verification diffs against, and the pair it
    /// gets is the one the read and write gates compare, so a connect-time error and a degraded poll
    /// cannot disagree.
    /// </summary>
    /// <remarks>
    /// A data point whose tag the controller does not have comes back with a <c>null</c>
    /// <see cref="ResolvedDataPoint.TagDefinition"/> rather than being left out, because an absent tag is
    /// itself something verification reports.
    /// <para>
    /// <b>A connect is the precondition</b>, not something this does on the caller's behalf: the dataport
    /// base creates its verifier from the client it has just acquired, so the schema is already in hand
    /// by the time anything resolves. Against that schema this is an in-memory projection and touches no
    /// device — the <see cref="Task"/> is the seam a structured data point will need, whose template the
    /// controller has to be asked for.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// The client is not connected, so there is no symbol table to resolve against.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    Task<IReadOnlyList<ResolvedDataPoint>> ResolveDataPoints(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken);
}
