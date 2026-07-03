using System.Data;

namespace BaseSQLServerRepository;

public interface IDbConnectionFactory
{
    Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData);
    Task WithConnection(Func<IDbConnection, Task> getData);
    Task<T> WithConnection<T>(Func<IDbConnection, IDbTransaction, Task<T>> getData);
    Task WithConnection(Func<IDbConnection, IDbTransaction, Task> getData);
    Task BulkCopy(DataTable table);
    Task BulkCopy(DataTable table, IDbConnection connection, IDbTransaction transaction);
}