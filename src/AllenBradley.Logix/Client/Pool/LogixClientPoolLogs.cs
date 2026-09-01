using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Pool;

internal static partial class LogixClientPoolLogs
{
    // Acquisition
    [LoggerMessage(400, LogLevel.Debug, "Acquiring client for {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void AcquiringClient(this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(401, LogLevel.Information, "Created new pooled client for {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void CreatedPooledClient(
        this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(402, LogLevel.Debug,
        "Reusing existing pooled client for {ConnectionEndpoint} via CIP route path {CipRoutePath}. Reference count: {RefCount}")]
    internal static partial void ReusingPooledClient(
        this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath, int refCount);

    // Release
    [LoggerMessage(410, LogLevel.Debug, "Releasing client for {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void ReleasingClient(this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(411, LogLevel.Debug,
        "Client released for {ConnectionEndpoint} via CIP route path {CipRoutePath}. Reference count: {RefCount}")]
    internal static partial void ClientReleased(
        this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath, int refCount);

    [LoggerMessage(412, LogLevel.Information,
        "Disconnecting and removing pooled client for {ConnectionEndpoint} via CIP route path {CipRoutePath} (no more references)")]
    internal static partial void DisconnectingPooledClient(
        this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath);

    // Errors
    [LoggerMessage(430, LogLevel.Error, "Failed to acquire client for {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void AcquireClientFailed(
        this ILogger<LogixClientPool> logger, Exception exception, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(431, LogLevel.Warning,
        "Attempted to release client for {ConnectionEndpoint} via CIP route path {CipRoutePath} but it was not found in the pool")]
    internal static partial void ClientNotFoundInPool(
        this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(432, LogLevel.Error, "Failed to disconnect pooled client for {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void DisconnectPooledClientFailed(
        this ILogger<LogixClientPool> logger, Exception exception, string connectionEndpoint, string cipRoutePath);

    [LoggerMessage(433, LogLevel.Warning,
        "Reference count underflow for {ConnectionEndpoint} via CIP route path {CipRoutePath} — possible double-release by a caller")]
    internal static partial void RefCountUnderflow(
        this ILogger<LogixClientPool> logger, string connectionEndpoint, string cipRoutePath);

    // Lifecycle
    [LoggerMessage(440, LogLevel.Information, "Connection pool disposed")]
    internal static partial void PoolDisposed(this ILogger<LogixClientPool> logger);
}
