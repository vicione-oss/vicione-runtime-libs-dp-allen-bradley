using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

internal static partial class LogixClientLogs
{
    [LoggerMessage(200, LogLevel.Debug, "Loading the tag-definitions (@tags) of the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void LoadingTagDefinitions(this ILogger<LogixClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(201, LogLevel.Information, "Connected to the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void Connected(this ILogger<LogixClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(202, LogLevel.Debug, "Already connected to the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void AlreadyConnected(this ILogger<LogixClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(203, LogLevel.Information, "Disconnected from the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void Disconnected(this ILogger<LogixClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(204, LogLevel.Debug, "Already disconnected from the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void AlreadyDisconnected(this ILogger<LogixClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(205, LogLevel.Debug, "Reading a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadingBatch(this ILogger<LogixClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(206, LogLevel.Debug, "Read a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadBatchSucceeded(this ILogger<LogixClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(207, LogLevel.Debug, "Writing a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WritingBatch(this ILogger<LogixClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(208, LogLevel.Debug, "Wrote a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WriteBatchSucceeded(this ILogger<LogixClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(209, LogLevel.Debug, "The batch against the controller at {ConnectionEndpoint} was cancelled by the caller")]
    internal static partial void BatchCancelled(this ILogger<LogixClient> logger, Exception exception, string connectionEndpoint);

    [LoggerMessage(230, LogLevel.Error, "Failed to connect to the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void ConnectFailed(
        this ILogger<LogixClient> logger, Exception exception, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(231, LogLevel.Error, "Failed to read a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadBatchFailed(
        this ILogger<LogixClient> logger, Exception exception, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(232, LogLevel.Error, "Failed to write a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WriteBatchFailed(
        this ILogger<LogixClient> logger, Exception exception, int dataPointCount, string connectionEndpoint);
}
