using EmployeeSearch.Domain.Entities;

namespace EmployeeSearch.Application.Common.Interfaces;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
