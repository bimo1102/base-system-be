using Enums;

namespace BasePostgreSQLRepositories;

public class PostgresSqlDbResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public PostgresDbErrorEnum Error { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    public static PostgresSqlDbResult<T> Ok(T data) => 
        new() { IsSuccess = true, Data = data, Error = PostgresDbErrorEnum.None };

    public static PostgresSqlDbResult<T> Fail(PostgresDbErrorEnum error, string message, Exception? ex = null) => 
        new() { IsSuccess = false, Error = error, ErrorMessage = message, Exception = ex };
}

public class PostgresSqlDbResult : PostgresSqlDbResult<object>
{
    public static PostgresSqlDbResult Ok() => 
        new() { IsSuccess = true, Error = PostgresDbErrorEnum.None };
        
    public static new PostgresSqlDbResult Fail(PostgresDbErrorEnum error, string message, Exception? ex = null) => 
        new() { IsSuccess = false, Error = error, ErrorMessage = message, Exception = ex };
}