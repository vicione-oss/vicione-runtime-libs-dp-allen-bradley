using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

internal static partial class LogixClientLogs
{
    [LoggerMessage(200, LogLevel.Debug, "Loading the tag-definitions (@tags) of the controller at {Gateway} via path {Path}")]
    internal static partial void LoadingTagDefinitions(this ILogger<LogixClient> logger, string gateway, string path);

    [LoggerMessage(201, LogLevel.Information, "Connected to the controller at {Gateway} via path {Path}")]
    internal static partial void Connected(this ILogger<LogixClient> logger, string gateway, string path);

    [LoggerMessage(202, LogLevel.Debug, "Already connected to the controller at {Gateway} via path {Path}")]
    internal static partial void AlreadyConnected(this ILogger<LogixClient> logger, string gateway, string path);

    [LoggerMessage(203, LogLevel.Information, "Disconnected from the controller at {Gateway} via path {Path}")]
    internal static partial void Disconnected(this ILogger<LogixClient> logger, string gateway, string path);

    [LoggerMessage(204, LogLevel.Debug, "Already disconnected from the controller at {Gateway} via path {Path}")]
    internal static partial void AlreadyDisconnected(this ILogger<LogixClient> logger, string gateway, string path);

    [LoggerMessage(230, LogLevel.Error, "Failed to connect to the controller at {Gateway} via path {Path}")]
    internal static partial void ConnectFailed(
        this ILogger<LogixClient> logger, Exception exception, string gateway, string path);
}
