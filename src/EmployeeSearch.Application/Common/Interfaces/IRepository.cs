using EmployeeSearch.Domain.Common;

namespace EmployeeSearch.Application.Common.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);

    /// <summary>Composable queryable for LINQ-based filtering/sorting in derived repositories.</summary>
    IQueryable<T> Query();
}
