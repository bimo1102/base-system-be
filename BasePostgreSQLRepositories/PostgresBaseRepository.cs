using BaseDomains;
using Z.Dapper.Plus;

namespace BasePostgreSQLRepositories;

public abstract class PostgresBaseRepository<T> : IPostgresBaseRepository<T> where T : BaseDomain
{
    protected readonly IPostgresConnectionFactory ConnectionFactory;

    protected PostgresBaseRepository(IPostgresConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory;
    }

    public async Task<PostgresSqlDbResult> Add(T obj)
    {
        // Trả ra DbResult tường minh, tầng nghiệp vụ (Business) check IsSuccess để xử lý tiếp
        return await ConnectionFactory.WithConnection(async connection =>
        {
            await connection.BulkInsertAsync(obj);
        });
    }

    public async Task<PostgresSqlDbResult> Change(T obj)
    {
        return await ConnectionFactory.WithConnection(async connection =>
        {
            await connection.BulkUpdateAsync(obj);
        });
    }

    public async Task<PostgresSqlDbResult> Remove(T obj)
    {
        return await ConnectionFactory.WithConnection(async connection =>
        {
            await connection.BulkDeleteAsync(obj);
        });
    }

    public async Task<PostgresSqlDbResult> RemoveRange(IEnumerable<T> objs)
    {
        return await ConnectionFactory.WithConnection(async connection =>
        {
            await connection.BulkDeleteAsync(objs);
        });
    }
}