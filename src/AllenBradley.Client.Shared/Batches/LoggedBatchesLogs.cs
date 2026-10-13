using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Batches;

internal static partial class LoggedBatchesLogs
{
    [LoggerMessage(205, LogLevel.Debug, "Reading a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadingBatch(this ILogger logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(206, LogLevel.Debug, "Read a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadBatchSucceeded(this ILogger logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(207, LogLevel.Debug, "Writing a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WritingBatch(this ILogger logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(208, LogLevel.Debug, "Wrote a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WriteBatchSucceeded(this ILogger logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(209, LogLevel.Debug, "The batch against the controller at {ConnectionEndpoint} was cancelled by the caller")]
    internal static partial void BatchCancelled(this ILogger logger, Exception exception, string connectionEndpoint);

    [LoggerMessage(210, LogLevel.Warning, "{FailedCount} of {DataPointCount} data points could not be read from the controller at {ConnectionEndpoint}; the values the rest produced were kept. Failures: {Failures}")]
    internal static partial void ReadBatchPartiallyFailed(
        this ILogger logger, int failedCount, int dataPointCount, string connectionEndpoint, string failures);

    [LoggerMessage(231, LogLevel.Error, "Failed to read a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadBatchFailed(
        this ILogger logger, Exception exception, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(232, LogLevel.Error, "Failed to write a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WriteBatchFailed(
        this ILogger logger, Exception exception, int dataPointCount, string connectionEndpoint);
}
