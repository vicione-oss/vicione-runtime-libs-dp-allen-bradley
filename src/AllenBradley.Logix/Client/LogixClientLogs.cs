using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client;

internal static partial class LogixClientLogs
{
    [LoggerMessage(200, LogLevel.Debug, "Loading the symbol table of the controller at {ConnectionEndpoint} via CIP route path {CipRoutePath}")]
    internal static partial void LoadingSymbolTable(this ILogger<LogixClient> logger, string connectionEndpoint, string cipRoutePath);
}
