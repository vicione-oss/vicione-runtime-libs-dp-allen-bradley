using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client;

internal static partial class LegacyClientLogs
{
    [LoggerMessage(201, LogLevel.Information, "Connected to the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void Connected(this ILogger<LegacyClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(202, LogLevel.Debug, "Already connected to the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void AlreadyConnected(this ILogger<LegacyClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(203, LogLevel.Information, "Disconnected from the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void Disconnected(this ILogger<LegacyClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(204, LogLevel.Debug, "Already disconnected from the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void AlreadyDisconnected(this ILogger<LegacyClient> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(230, LogLevel.Error, "Failed to connect to the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}: {Error}")]
    internal static partial void ConnectFailed(
        this ILogger<LegacyClient> logger, string connectionEndpoint, string cipRoutePath, string? error);

    [LoggerMessage(233, LogLevel.Error, "Failed to connect to the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void ConnectThrew(
        this ILogger<LegacyClient> logger, Exception exception, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(205, LogLevel.Debug, "Reading a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadingBatch(this ILogger<LegacyClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(206, LogLevel.Debug, "Read a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadBatchSucceeded(this ILogger<LegacyClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(210, LogLevel.Warning, "{FailedCount} of {DataPointCount} data points could not be read from the controller at {ConnectionEndpoint}; the values the rest produced were kept. Failures: {Failures}")]
    internal static partial void ReadBatchPartiallyFailed(
        this ILogger<LegacyClient> logger, int failedCount, int dataPointCount, string connectionEndpoint, string failures);

    [LoggerMessage(209, LogLevel.Debug, "The batch against the controller at {ConnectionEndpoint} was cancelled by the caller")]
    internal static partial void BatchCancelled(this ILogger<LegacyClient> logger, Exception exception, string connectionEndpoint);

    [LoggerMessage(231, LogLevel.Error, "Failed to read a batch of {DataPointCount} data points from the controller at {ConnectionEndpoint}")]
    internal static partial void ReadBatchFailed(
        this ILogger<LegacyClient> logger, Exception exception, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(207, LogLevel.Debug, "Writing a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WritingBatch(this ILogger<LegacyClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(208, LogLevel.Debug, "Wrote a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WriteBatchSucceeded(this ILogger<LegacyClient> logger, int dataPointCount, string connectionEndpoint);

    [LoggerMessage(232, LogLevel.Error, "Failed to write a batch of {DataPointCount} data points to the controller at {ConnectionEndpoint}")]
    internal static partial void WriteBatchFailed(
        this ILogger<LegacyClient> logger, Exception exception, int dataPointCount, string connectionEndpoint);
}
