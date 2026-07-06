using System.Data;

namespace BasePostgreSQLRepositories;

public interface IPostgresConnectionFactory
{
    Task<PostgresSqlDbResult<T>> WithConnection<T>(Func<IDbConnection, Task<T>> getData);
    Task<PostgresSqlDbResult> WithConnection(Func<IDbConnection, Task> getData);
    Task<PostgresSqlDbResult<T>> WithConnection<T>(Func<IDbConnection, IDbTransaction, Task<T>> getData);
    Task<PostgresSqlDbResult> WithConnection(Func<IDbConnection, IDbTransaction, Task> getData);
}