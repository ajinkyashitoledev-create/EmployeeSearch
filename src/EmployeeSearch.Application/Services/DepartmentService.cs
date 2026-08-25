using EmployeeSearch.Application.Common.Exceptions;
using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Application.Mapping;
using EmployeeSearch.Domain.Entities;

namespace EmployeeSearch.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _unitOfWork.Departments.GetAllAsync(cancellationToken);
        return departments.Select(d => d.ToDto()).ToList();
    }

    public async Task<DepartmentDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Department), id);

        return department.ToDto();
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var department = new Department { Name = request.Name, Location = request.Location };
        await _unitOfWork.Departments.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return department.ToDto();
    }
}
