namespace BaseApplication.Services;

public class LogAction(ILogger<LogAction> logger) : ILogAction
{
    public void LogInformation(string message, params object?[] args) =>
        logger.LogInformation(message, args);

    public void LogWarning(string message, params object?[] args) =>
        logger.LogWarning(message, args);

    public void LogError(Exception? ex, string message, params object?[] args) =>
        logger.LogError(ex, message, args);
}
