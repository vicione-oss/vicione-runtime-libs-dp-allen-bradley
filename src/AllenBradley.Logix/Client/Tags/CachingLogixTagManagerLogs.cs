using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

internal static partial class CachingLogixTagManagerLogs
{
    [LoggerMessage(100, LogLevel.Warning, "Failed to free tag for data point {dataPoint}")]
    internal static partial void FailedToFreeTagForDataPoint(
        this ILogger<CachingLogixTagManager> logger, ILogixDataPoint dataPoint, Exception exception);
}
