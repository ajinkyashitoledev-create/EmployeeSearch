using EmployeeSearch.Application.Common.Models;
using EmployeeSearch.Application.DTOs;

namespace EmployeeSearch.Application.Services;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> SearchAsync(EmployeeSearchRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DeveloperDto> CreateDeveloperAsync(CreateDeveloperRequest request, CancellationToken cancellationToken = default);
    Task<ManagerDto> CreateManagerAsync(CreateManagerRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<DeveloperDto> UpdateDeveloperDetailsAsync(int id, UpdateDeveloperDetailsRequest request, CancellationToken cancellationToken = default);
    Task<ManagerDto> UpdateManagerDetailsAsync(int id, UpdateManagerDetailsRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
