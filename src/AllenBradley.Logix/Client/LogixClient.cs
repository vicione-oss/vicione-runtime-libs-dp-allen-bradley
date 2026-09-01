using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

/// <summary>
/// The controller client: reads and writes Logix data points over one shared tag manager, and owns that
/// manager's lifetime.
/// </summary>
/// <remarks>
/// One class serves both directions so the incoming and outgoing dataports — created separately but
/// targeting the same controller — share its connection; the <see cref="ILogixReadClient"/> /
/// <see cref="ILogixWriteClient"/> seams stay split so each dataport depends only on the direction
/// it uses (<c>ADR/2026-07-16-maximizing-throughput-with-one-shared-connection.md</c>).
/// <para>
/// The whole lifecycle is the tag manager's: connect loads its schema, disconnect drains it, dispose
/// ends it. Nothing else about the client is connection state, which is why the flag below is the only
/// bookkeeping here.
/// </para>
/// <para>
/// <b>What "connected" claims.</b> libplctag opens no socket until a handle is first read and exposes
/// no connection status, so there is no transport state for the flag to mirror and it does not pretend
/// to be one. It records what this client did: <see cref="ConnectAsync"/> browsed the symbol table and
/// nothing has since dropped it. Connect is the only thing that sets it, which is what lets
/// <see cref="DisconnectAsync"/> read it to decide whether there are handles to free.
/// </para>
/// </remarks>
/// <param name="tagManager">Resolves the tag for each data point this client reads or writes.</param>
/// <param name="clientInformation">The controller this client talks to — named in every log line.</param>
/// <param name="logger">Records the browse, which is the slow part of a connect.</param>
internal sealed class LogixClient(
    ILogixTagManager tagManager,
    LogixClientInformation clientInformation,
    ILogger<LogixClient> logger) : ILogixClient
{
    private readonly string _gateway = clientInformation.Gateway.Value;
    private readonly string _cipRoutePath = clientInformation.CipRoutePath.Value;
    private readonly Lock _gate = new();

    private bool _connected;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsConnected
    {
        get
        {
            lock (_gate)
            {
                return _connected;
            }
        }
    }

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        // Not what makes a second connect free — the tag manager's own gate is, and it holds for
        // concurrent callers as this check cannot. This is the fast path and the log line that says a
        // caller drove connect defensively; two callers slipping past it together cost nothing.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_connected)
            {
                logger.AlreadyConnected(_gateway, _cipRoutePath);
                return;
            }
        }

        try
        {
            logger.LoadingTagDefinitions(_gateway, _cipRoutePath);
            await tagManager.LoadTagDefinitionsAsync(cancellationToken).ConfigureAwait(false);
        }
        // Every way a browse can fail — an unreachable gateway, a route path that goes nowhere, a
        // controller that will not answer @tags — means the same thing to the caller of a connect, so
        // they arrive as the one exception the framework expects from an acquire.
        //
        // The exclusions are not answers about the device and would be lies as connection failures: a
        // cancellation is the caller's own shutdown, and a client disposed mid-browse is a caller bug.
        catch (Exception ex) when (ex is not (OperationCanceledException or ObjectDisposedException))
        {
            logger.ConnectFailed(ex, _gateway, _cipRoutePath);
            throw new ConnectionFailureException(
                $"Could not connect to the Logix controller at '{_gateway}' via CIP route path '{_cipRoutePath}'.", ex);
        }

        lock (_gate)
        {
            _connected = true;
        }

        logger.Connected(_gateway, _cipRoutePath);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Freeing handles is local work with nothing to wait on, and it is cleanup, so the token is not
    /// honoured: a cancelled disconnect that skipped the drain would leave handles for the finalizer.
    /// </remarks>
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_connected)
            {
                logger.AlreadyDisconnected(_gateway, _cipRoutePath);
                return Task.CompletedTask;
            }

            tagManager.Drain();
            _connected = false;
        }

        logger.Disconnected(_gateway, _cipRoutePath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ResolvedDataPoint>> ResolveDataPoints(
        IReadOnlyList<ILogixDataPoint> dataPoints, CancellationToken cancellationToken)
    {
        ThrowIfNotConnected();

        var resolved = new ResolvedDataPoint[dataPoints.Count];
        for (var i = 0; i < dataPoints.Count; i++)
        {
            // The tag the poll will use, projected to its (configured, reported) pair: what verification
            // diffs is the very tag the reads run against, not a second lookup of it.
            resolved[i] = tagManager.TagFor(dataPoints[i]).Resolved;
        }

        return Task.FromResult<IReadOnlyList<ResolvedDataPoint>>(resolved);
    }

    /// <summary>
    /// The connect this client has to have behind it before its schema can be read off. Stated here
    /// rather than worked around: the dataport base verifies against the client it has just acquired, so
    /// an unconnected one reaching this is a caller that has broken the lifecycle, and browsing on its
    /// behalf would hide that.
    /// </summary>
    private void ThrowIfNotConnected()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_connected)
            {
                throw new InvalidOperationException(
                    $"The client for '{_gateway}' via CIP route path '{_cipRoutePath}' must be connected before its data " +
                    "points can be resolved; connect is what browses the symbol table they resolve against.");
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<ILogixDataPointValue>> ReadAsync(
        LogixDataPointGroup dataPointGroup, CancellationToken cancellationToken) =>
        await new LogixReadBatch(dataPointGroup.DataPoints, tagManager)
            .ReadAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async ValueTask WriteAsync(
        IReadOnlyList<ILogixDataPointValue> values, CancellationToken cancellationToken) =>
        await new LogixWriteBatch(values, tagManager).WriteAsync(cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Ends the client and the tag manager with it. Disposing is what frees the native handles, and a
    /// handle left to its finalizer fail-fasts the process at CLR teardown.
    /// </summary>
    public void Dispose()
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

        tagManager.Dispose();
    }
}
