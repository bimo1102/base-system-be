using Common;

namespace BaseApplication.Services;

/// <summary>
/// Context request hiện tại: IP, header, service provider scope.
/// AuthenService / login info sẽ bổ sung khi có Cache (Redis).
/// </summary>
public class ContextService(
    ILogger<ContextService> logger,
    IHttpContextAccessor? httpContextAccessor,
    IServiceProvider serviceProvider,
    ILogAction logAction)
{
    private readonly IServiceProvider _serviceProvider =
        httpContextAccessor?.HttpContext?.RequestServices ?? serviceProvider;

    public readonly ILogAction LogAction = logAction;

    public T GetService<T>() where T : notnull =>
        _serviceProvider.GetRequiredService<T>();

    public string GetIp()
    {
        try
        {
            var result = string.Empty;
            if (httpContextAccessor?.HttpContext?.Request?.Headers != null)
            {
                var forwardedHeader = httpContextAccessor.HttpContext.Request.Headers["X-FORWARDED-FOR"];
                if (!string.IsNullOrEmpty(forwardedHeader))
                {
                    result = forwardedHeader.FirstOrDefault() ?? string.Empty;
                }
            }

            if (string.IsNullOrEmpty(result) &&
                httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress != null)
            {
                result = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            if (result.Equals("::1", StringComparison.InvariantCultureIgnoreCase))
            {
                result = "127.0.0.1";
            }

            return result.AsEmpty();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "GetIp failed");
            return string.Empty;
        }
    }

    public string? GetHeader(string name)
    {
        if (httpContextAccessor?.HttpContext?.Request?.Headers == null)
        {
            return null;
        }

        return httpContextAccessor.HttpContext.Request.Headers.TryGetValue(name, out var value)
            ? value.FirstOrDefault()
            : null;
    }
}
