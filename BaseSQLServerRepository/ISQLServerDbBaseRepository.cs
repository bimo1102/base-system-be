using BaseDomains;

namespace BaseSQLServerRepository;

public interface ISqlServerDbBaseRepository<in T> where T : BaseDomain
{
    Task Add(T obj);
    Task Change(T obj);
    Task Remove(T obj);
    Task RemoveRange(IEnumerable<T> obj);
}