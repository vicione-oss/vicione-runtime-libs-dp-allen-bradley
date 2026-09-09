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
/// <param name="tagManager">Resolves the tag for each data point this client reads or writes.</param>
/// <param name="clientInformation">The controller this client talks to — named in every log line.</param>
/// <param name="logger">Records the browse, which is the slow part of a connect.</param>
internal sealed class LogixClient(
    ILogixTagManager tagManager,
    LogixClientInformation clientInformation,
    ILogger<LogixClient> logger) : ILogixClient
{
    private readonly string _connectionEndpoint = clientInformation.ConnectionEndpoint.Value;
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
                logger.AlreadyConnected(_connectionEndpoint, _cipRoutePath);
                return;
            }
        }

        try
        {
            logger.LoadingTagDefinitions(_connectionEndpoint, _cipRoutePath);
            await tagManager.LoadTagDefinitionsAsync(cancellationToken).ConfigureAwait(false);
        }
        // Every way a browse can fail — an unreachable connection endpoint, a route path that goes nowhere, a
        // controller that will not answer @tags — means the same thing to the caller of a connect, so
        // they arrive as the one exception the framework expects from an acquire.
        //
        // The exclusions are not answers about the device and would be lies as connection failures: a
        // cancellation is the caller's own shutdown, and a client disposed mid-browse is a caller bug.
        catch (Exception ex) when (ex is not (OperationCanceledException or ObjectDisposedException))
        {
            logger.ConnectFailed(ex, _connectionEndpoint, _cipRoutePath);
            throw new ConnectionFailureException(
                $"Could not connect to the Logix controller at '{_connectionEndpoint}' via CIP route path '{_cipRoutePath}'.", ex);
        }

        lock (_gate)
        {
            _connected = true;
        }

        logger.Connected(_connectionEndpoint, _cipRoutePath);
    }

    /// <inheritdoc />
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_connected)
            {
                logger.AlreadyDisconnected(_connectionEndpoint, _cipRoutePath);
                return Task.CompletedTask;
            }

            tagManager.Drain();
            _connected = false;
        }

        logger.Disconnected(_connectionEndpoint, _cipRoutePath);
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
                    $"The client for '{_connectionEndpoint}' via CIP route path '{_cipRoutePath}' must be connected before its data " +
                    "points can be resolved; connect is what browses the symbol table they resolve against.");
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<ILogixDataPointValue>> ReadAsync(
        LogixDataPointGroup dataPointGroup, CancellationToken cancellationToken)
    {
        var count = dataPointGroup.DataPoints.Count;
        logger.ReadingBatch(count, _connectionEndpoint);
        try
        {
            var result = await new LogixReadBatch(dataPointGroup.DataPoints, tagManager)
                .ReadAsync(cancellationToken).ConfigureAwait(false);

            // The tags that did answer are still worth publishing, so a partial batch is a warning rather
            // than a failure. It is logged here because this is where the logger is: the batch names the
            // tags, the client says which controller they were on.
            if (result.Failures.Count > 0)
            {
                logger.ReadBatchPartiallyFailed(
                    result.Failures.Count, count, _connectionEndpoint, result.DescribeFailures());
            }

            logger.ReadBatchSucceeded(result.Values.Count, _connectionEndpoint);
            return result.Values;
        }
        // A cancelled batch is the caller's own shutdown, not the controller failing to answer, so it is
        // logged as what it is and travels untouched. Everything else is a batch that read nothing at all.
        catch (OperationCanceledException ex)
        {
            logger.BatchCancelled(ex, _connectionEndpoint);
            throw;
        }
        catch (Exception ex)
        {
            logger.ReadBatchFailed(ex, count, _connectionEndpoint);
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask WriteAsync(
        IReadOnlyList<ILogixDataPointValue> values, CancellationToken cancellationToken)
    {
        var count = values.Count;
        logger.WritingBatch(count, _connectionEndpoint);

        try
        {
            await new LogixWriteBatch(values, tagManager).WriteAsync(cancellationToken).ConfigureAwait(false);
            logger.WriteBatchSucceeded(count, _connectionEndpoint);
        }
        catch (OperationCanceledException ex)
        {
            logger.BatchCancelled(ex, _connectionEndpoint);
            throw;
        }
        catch (Exception ex)
        {
            logger.WriteBatchFailed(ex, count, _connectionEndpoint);
            throw;
        }
    }

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
