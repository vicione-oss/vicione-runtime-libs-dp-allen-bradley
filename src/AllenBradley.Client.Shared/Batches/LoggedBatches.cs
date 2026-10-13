using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;

/// <summary>
/// Runs the read and write batches of one controller client and logs how each one ended. A batch that
/// throws is logged and the exception travels on to the caller.
/// </summary>
internal sealed class LoggedBatches(ILogger logger, string connectionEndpoint)
{
    internal async ValueTask<IReadOnlyList<TValue>> ReadAsync<TDataPoint, TValue>(
        int dataPointCount,
        Func<CancellationToken, Task<ReadBatch<TDataPoint, TValue>.BatchReadResult>> read,
        CancellationToken cancellationToken)
        where TDataPoint : IDataPoint
        where TValue : class
    {
        logger.ReadingBatch(dataPointCount, connectionEndpoint);

        try
        {
            var result = await read(cancellationToken).ConfigureAwait(false);

            // The values that were read are still published, so a partial batch is only a warning.
            if (result.Failures.Count > 0)
            {
                logger.ReadBatchPartiallyFailed(
                    result.Failures.Count, dataPointCount, connectionEndpoint, result.DescribeFailures());
            }

            logger.ReadBatchSucceeded(result.Values.Count, connectionEndpoint);
            return result.Values;
        }
        catch (OperationCanceledException ex)
        {
            logger.BatchCancelled(ex, connectionEndpoint);
            throw;
        }
        catch (Exception ex)
        {
            logger.ReadBatchFailed(ex, dataPointCount, connectionEndpoint);
            throw;
        }
    }

    internal async ValueTask WriteAsync(
        int dataPointCount, Func<CancellationToken, Task> write, CancellationToken cancellationToken)
    {
        logger.WritingBatch(dataPointCount, connectionEndpoint);

        try
        {
            await write(cancellationToken).ConfigureAwait(false);
            logger.WriteBatchSucceeded(dataPointCount, connectionEndpoint);
        }
        catch (OperationCanceledException ex)
        {
            logger.BatchCancelled(ex, connectionEndpoint);
            throw;
        }
        catch (Exception ex)
        {
            logger.WriteBatchFailed(ex, dataPointCount, connectionEndpoint);
            throw;
        }
    }
}
