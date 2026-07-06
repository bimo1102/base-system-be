using BaseDomains;

namespace BasePostgreSQLRepositories;

public interface IPostgresBaseRepository<in T> where T : BaseDomain
{
    Task<PostgresSqlDbResult> Add(T obj);
    Task<PostgresSqlDbResult> Change(T obj);
    Task<PostgresSqlDbResult> Remove(T obj);
    Task<PostgresSqlDbResult> RemoveRange(IEnumerable<T> objs);
}