using System.ComponentModel.DataAnnotations;

namespace Configs;

/// <summary>
/// Các key cấu hình dùng chung cho mọi microservice.
/// Tên enum = key appsettings (trừ khi có [Display(Name = "...")]).
/// </summary>
public enum ConfigSettingEnum
{
    AppName,
    AppVersion,
    HttpType,
    HttpPort,
    HttpPort2,
    Https,
    LogEventLevel,
    LogToEsUrl,
    AuthenticationType,
    XApiKeyEnable,
    CookieAuthenName,
    CookieDomain,

    [Display(Name = "JwtTokens:Key")]
    JwtTokensKey,

    Cors,
    IsDevEnvironment,
    UseSwagger,
    StartWorker,

    // SQL Server — Write side (Command / Domain)
    [Display(Name = "ConnectionStrings:DbConnectionString")]
    DbConnectionString,

    // PostgreSQL — Read side (Query / ReadModel)
    [Display(Name = "ConnectionStrings:PostgresConnectionString")]
    PostgresConnectionString,

    // Redis cache
    RedisHostIps,
    RedisPersistenceHostIps,
    RedisPassword,
    RedisCacheDbId,
    RedisPersistenceDbId,
    RedisPoolSize,
    DataProtectionRedisKey,
    CacheEnable,

    // RabbitMQ event bus
    RabbitMqHost,
    VirtualHost,
    RabbitMqUserName,
    RabbitMqPassword,
    RabbitMqPoolSize,
    RabbitMqExChange,
    RabbitMqExChangeTrigger,
    RabbitMqRoutingRoot,
    RabbitMqRouting,
    RabbitMqQueues,
    RabbitMqPrefetchCount,
    RabbitMqExChangeNotifyListen,
    RabbitMqExChangeTriggerListen,

    // Event storage (optional)
    EventDatabaseConnectionString,
    EventDatabaseName,
    EventCollectionName,

    AccountManagerAutomaticKeyGeneration,
}
