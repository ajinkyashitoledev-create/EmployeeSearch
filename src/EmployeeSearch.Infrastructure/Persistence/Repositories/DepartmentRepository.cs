using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSearch.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(d => d.Id == id, cancellationToken);

    public override async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Include(d => d.Employees).ToListAsync(cancellationToken);
}
