using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

internal static partial class LogixClientPoolLogs
{
    // Acquisition
    [LoggerMessage(400, LogLevel.Debug, "Acquiring client for {Gateway} via path {Path}")]
    internal static partial void AcquiringClient(this ILogger<LogixClientPool> logger, string gateway, string path);

    [LoggerMessage(401, LogLevel.Information, "Created new pooled client for {Gateway} via path {Path}")]
    internal static partial void CreatedPooledClient(
        this ILogger<LogixClientPool> logger, string gateway, string path);

    [LoggerMessage(402, LogLevel.Debug,
        "Reusing existing pooled client for {Gateway} via path {Path}. Reference count: {RefCount}")]
    internal static partial void ReusingPooledClient(
        this ILogger<LogixClientPool> logger, string gateway, string path, int refCount);

    // Release
    [LoggerMessage(410, LogLevel.Debug, "Releasing client for {Gateway} via path {Path}")]
    internal static partial void ReleasingClient(this ILogger<LogixClientPool> logger, string gateway, string path);

    [LoggerMessage(411, LogLevel.Debug,
        "Client released for {Gateway} via path {Path}. Reference count: {RefCount}")]
    internal static partial void ClientReleased(
        this ILogger<LogixClientPool> logger, string gateway, string path, int refCount);

    [LoggerMessage(412, LogLevel.Information,
        "Disconnecting and removing pooled client for {Gateway} via path {Path} (no more references)")]
    internal static partial void DisconnectingPooledClient(
        this ILogger<LogixClientPool> logger, string gateway, string path);

    // Errors
    [LoggerMessage(430, LogLevel.Error, "Failed to acquire client for {Gateway} via path {Path}")]
    internal static partial void AcquireClientFailed(
        this ILogger<LogixClientPool> logger, Exception exception, string gateway, string path);

    [LoggerMessage(431, LogLevel.Warning,
        "Attempted to release client for {Gateway} via path {Path} but it was not found in the pool")]
    internal static partial void ClientNotFoundInPool(
        this ILogger<LogixClientPool> logger, string gateway, string path);

    [LoggerMessage(432, LogLevel.Error, "Failed to disconnect pooled client for {Gateway} via path {Path}")]
    internal static partial void DisconnectPooledClientFailed(
        this ILogger<LogixClientPool> logger, Exception exception, string gateway, string path);

    [LoggerMessage(433, LogLevel.Warning,
        "Reference count underflow for {Gateway} via path {Path} — possible double-release by a caller")]
    internal static partial void RefCountUnderflow(
        this ILogger<LogixClientPool> logger, string gateway, string path);

    // Lifecycle
    [LoggerMessage(440, LogLevel.Information, "Connection pool disposed")]
    internal static partial void PoolDisposed(this ILogger<LogixClientPool> logger);
}
