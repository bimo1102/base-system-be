using System.Data;
using System.Data.Common;
using Enums;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;

namespace BasePostgreSQLRepositories;

public class PostgresConnectionFactory : IPostgresConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresConnectionFactory> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public PostgresConnectionFactory(string connectionString, ILogger<PostgresConnectionFactory> logger)
    {
        _connectionString = connectionString;
        _logger = logger;

        // Cấu hình Polly v8 Retry Pattern (Mục số 5 trong file plan)
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>().Handle<TimeoutException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1)
            })
            .Build();
    }

    private async Task<IDbConnection> GetNewConnectionAsync()
    {
        try
        {
            DbConnection dbConnection = new NpgsqlConnection(_connectionString);
            await dbConnection.OpenAsync();
            return dbConnection;
        }
        catch (Exception e)
        {
            e.Data["PostgresFactory.Message-CreateConnection"] = "Khởi tạo NpgsqlConnection thất bại";
            _logger.LogError(e, "Postgres Connection Exception: {Message}", e.Message);
            throw;
        }
    }

    public async Task<PostgresSqlDbResult<T>> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
    {
        try
        {
            // Bọc qua Polly để tự động Retry khi lỗi mạng ngắt quãng
            return await _resiliencePipeline.ExecuteAsync(async _ =>
            {
                using var dbConnection = await GetNewConnectionAsync();
                var data = await getData(dbConnection);
                return PostgresSqlDbResult<T>.Ok(data);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Postgres Execute Exception: {Message}", ex.Message);
            return PostgresSqlDbResult<T>.Fail(PostgresDbErrorEnum.ExecutionFailed, ex.Message, ex);
        }
    }

    public async Task<PostgresSqlDbResult> WithConnection(Func<IDbConnection, Task> getData)
    {
        try
        {
            return await _resiliencePipeline.ExecuteAsync(async _ =>
            {
                using var dbConnection = await GetNewConnectionAsync();
                await getData(dbConnection);
                return PostgresSqlDbResult.Ok();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Postgres Execute Exception: {Message}", ex.Message);
            return PostgresSqlDbResult.Fail(PostgresDbErrorEnum.ExecutionFailed, ex.Message, ex);
        }
    }

    public async Task<PostgresSqlDbResult<T>> WithConnection<T>(Func<IDbConnection, IDbTransaction, Task<T>> getData)
    {
        try
        {
            return await _resiliencePipeline.ExecuteAsync(async _ =>
            {
                using var dbConnection = await GetNewConnectionAsync();
                using var transaction = dbConnection.BeginTransaction();
                try
                {
                    var result = await getData(dbConnection, transaction);
                    transaction.Commit();
                    return PostgresSqlDbResult<T>.Ok(result);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw; // Throw để Polly có thể bắt được và thực hiện Retry lại toàn bộ Trans nếu cấu hình
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Postgres Transaction Exception: {Message}", ex.Message);
            return PostgresSqlDbResult<T>.Fail(PostgresDbErrorEnum.ExecutionFailed, ex.Message, ex);
        }
    }

    public async Task<PostgresSqlDbResult> WithConnection(Func<IDbConnection, IDbTransaction, Task> getData)
    {
        try
        {
            return await _resiliencePipeline.ExecuteAsync(async _ =>
            {
                using var dbConnection = await GetNewConnectionAsync();
                using var transaction = dbConnection.BeginTransaction();
                try
                {
                    await getData(dbConnection, transaction);
                    transaction.Commit();
                    return PostgresSqlDbResult.Ok();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Postgres Transaction Exception: {Message}", ex.Message);
            return PostgresSqlDbResult.Fail(PostgresDbErrorEnum.ExecutionFailed, ex.Message, ex);
        }
    }
}