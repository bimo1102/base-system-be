using BaseDomains;
using Z.Dapper.Plus;

namespace BaseSQLServerRepository;

public abstract class SqlServerDbBaseRepository<T>(IDbConnectionFactory dbConnectionFactory) : ISqlServerDbBaseRepository<T> where T : BaseDomain
{
    protected readonly IDbConnectionFactory DbConnectionFactory = dbConnectionFactory;

    public async Task Add(T obj)
    {
        await DbConnectionFactory.WithConnection(async connection =>
        {
            await connection.BulkInsertAsync(obj);
        });
    }

    public async Task Change(T obj)
    {
        await DbConnectionFactory.WithConnection(async (connection) =>
        {
            await connection.BulkUpdateAsync(obj);
        });
    }
    
    public async Task Remove(T obj)
    {
        await DbConnectionFactory.WithConnection(async (connection) =>
        {
            await connection.BulkDeleteAsync(obj);
        });
    }
    
    public async Task RemoveRange(IEnumerable<T> obj)
    {
        await DbConnectionFactory.WithConnection(async (connection) =>
        {
            await connection.BulkDeleteAsync(obj);
        });
    }
}