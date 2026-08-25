using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Domain.Entities;

namespace EmployeeSearch.Application.Common.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Developer?> GetDeveloperByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Manager?> GetManagerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(EmployeeSearchRequest request, CancellationToken cancellationToken = default);
}
