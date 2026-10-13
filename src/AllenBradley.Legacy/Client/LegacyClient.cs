using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

/// <summary>
/// Reads and writes the data points of one Legacy controller. It gets its tag handles from a
/// <see cref="LegacyTagSource"/> and disposes that source when it is disposed.
/// </summary>
internal sealed class LegacyClient(
    LegacyTagSource tagSource,
    LegacyClientInformation clientInformation,
    ILogger<LegacyClient> logger) : ILegacyClient
{
    private readonly string _connectionEndpoint = clientInformation.ConnectionEndpoint.Value;
    private readonly string _cipRoutePath = clientInformation.CipRoutePath.Value;
    private readonly Lock _gate = new();

    private bool _connected;
    private bool _disposed;

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

    // Every PCCC controller has the status file S2, and nobody can delete it. A read of its first word goes
    // the same route as the data points, so it is the smallest request that shows the controller answers.
    internal static readonly DataFileAddress FirstWordOfTheStatusFile =
        new(DataFileType.Status, new FileNumber(2), new ElementNumber(0));

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_connected)
            {
                logger.AlreadyConnected(_connectionEndpoint, _cipRoutePath);
                return;
            }
        }

        TagReadResult statusWord;
        try
        {
            using var statusWordAccess = tagSource.CreateUncachedAccess(FirstWordOfTheStatusFile);
            statusWord = await statusWordAccess.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
        // A cancellation is the caller's own shutdown, and a disposal during the read is a caller bug. Every
        // other exception reaches the framework as the one exception it expects from an acquire.
        catch (Exception ex) when (ex is not (OperationCanceledException or ObjectDisposedException))
        {
            logger.ConnectThrew(ex, _connectionEndpoint, _cipRoutePath);
            throw new ConnectionFailureException(
                $"Could not connect to the Legacy controller at '{_connectionEndpoint}' via CIP route path '{_cipRoutePath}'.", ex);
        }

        if (!statusWord.Succeeded)
        {
            logger.ConnectFailed(_connectionEndpoint, _cipRoutePath, statusWord.Error);
            throw new ConnectionFailureException(
                $"Could not connect to the Legacy controller at '{_connectionEndpoint}' via CIP route path '{_cipRoutePath}'. {statusWord.Error}");
        }

        lock (_gate)
        {
            _connected = true;
        }

        logger.Connected(_connectionEndpoint, _cipRoutePath);
    }

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

            tagSource.Drain();
            _connected = false;
        }

        logger.Disconnected(_connectionEndpoint, _cipRoutePath);
        return Task.CompletedTask;
    }

    public async ValueTask<IReadOnlyList<ILegacyDataPointValue>> ReadAsync(
        LegacyDataPointGroup dataPointGroup, CancellationToken cancellationToken)
    {
        var count = dataPointGroup.DataPoints.Count;
        logger.ReadingBatch(count, _connectionEndpoint);

        try
        {
            var result = await new LegacyReadBatch(dataPointGroup.DataPoints, tagSource)
                .ReadAsync(cancellationToken).ConfigureAwait(false);

            // The values that were read are still published. The failed elements are only logged as a warning.
            if (result.Failures.Count > 0)
            {
                logger.ReadBatchPartiallyFailed(
                    result.Failures.Count, count, _connectionEndpoint, result.DescribeFailures());
            }

            logger.ReadBatchSucceeded(result.Values.Count, _connectionEndpoint);
            return result.Values;
        }
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

    public async ValueTask WriteAsync(IReadOnlyList<ILegacyDataPointValue> values, CancellationToken cancellationToken)
    {
        var count = values.Count;
        logger.WritingBatch(count, _connectionEndpoint);

        try
        {
            await new LegacyWriteBatch(values, tagSource).WriteAsync(cancellationToken).ConfigureAwait(false);
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
    /// Disposes the tag source, which disposes every tag handle. A tag handle left to its finalizer
    /// crashes the process at CLR teardown.
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

        tagSource.Dispose();
    }
}
