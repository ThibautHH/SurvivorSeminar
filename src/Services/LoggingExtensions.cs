namespace Microsoft.Extensions.Logging;

internal static partial class LoggingExtensions
{
    [LoggerMessage(0, LogLevel.Debug, "{ServiceName} is now running.", EventName = "ServicesRunning")]
    public static partial void ServiceRunning(this ILogger logger, string serviceName);
}
