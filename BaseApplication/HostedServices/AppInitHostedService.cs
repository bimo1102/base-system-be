namespace BaseApplication.HostedServices;

/// <summary>
/// Hosted service khởi tạo khi app start (Redis pool, RabbitMQ pool, EventProcessor... sẽ cắm thêm sau).
/// </summary>
public class AppInitHostedService(ILogger<AppInitHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "AppInitHostedService starting. Host={HostName}",
            BaseProgram.HostName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AppInitHostedService stopping");
        return Task.CompletedTask;
    }
}
