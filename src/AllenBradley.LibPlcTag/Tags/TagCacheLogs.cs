using Microsoft.Extensions.Logging;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tags;

internal static partial class TagCacheLogs
{
    [LoggerMessage(100, LogLevel.Warning, "Failed to free tag for data point {dataPoint}")]
    internal static partial void FailedToFreeTagForDataPoint(this ILogger logger, object dataPoint, Exception exception);
}
