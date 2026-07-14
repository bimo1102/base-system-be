namespace BaseApplication.Services;

public interface ILogAction
{
    void LogInformation(string message, params object?[] args);
    void LogWarning(string message, params object?[] args);
    void LogError(Exception? ex, string message, params object?[] args);
}
