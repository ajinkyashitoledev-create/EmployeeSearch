using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSearch.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    public Task<Employee?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        DbSet.Include(e => e.Department)
             .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Developer?> GetDeveloperByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Context.Set<Developer>().Include(d => d.Department)
             .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<Manager?> GetManagerByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Context.Set<Manager>().Include(m => m.Department)
             .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<bool> ExistsWithEmailAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(e => e.Email == email && (excludeEmployeeId == null || e.Id != excludeEmployeeId), cancellationToken);

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(EmployeeSearchRequest request, CancellationToken cancellationToken = default)
    {
        // Base query over the Employees table; EF materializes each row as its
        // real runtime type (Developer/Manager) via the TPT join, so mixed
        // results come back correctly typed with no extra mapping code.
        IQueryable<Employee> query = DbSet.Include(e => e.Department);

        query = request.EmployeeType switch
        {
            EmployeeTypeFilter.Developer => query.OfType<Developer>(),
            EmployeeTypeFilter.Manager => query.OfType<Manager>(),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(e =>
                EF.Functions.Like(e.FirstName, $"%{term}%") ||
                EF.Functions.Like(e.LastName, $"%{term}%") ||
                EF.Functions.Like(e.Email, $"%{term}%"));
        }

        if (request.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == request.DepartmentId.Value);

        if (request.MinSalary.HasValue)
            query = query.Where(e => e.Salary >= request.MinSalary.Value);

        if (request.MaxSalary.HasValue)
            query = query.Where(e => e.Salary <= request.MaxSalary.Value);

        if (request.HiredAfter.HasValue)
            query = query.Where(e => e.HireDate >= request.HiredAfter.Value);

        if (request.HiredBefore.HasValue)
            query = query.Where(e => e.HireDate <= request.HiredBefore.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string sortBy, bool descending) =>
        sortBy.ToLowerInvariant() switch
        {
            "firstname" => descending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
            "salary" => descending ? query.OrderByDescending(e => e.Salary) : query.OrderBy(e => e.Salary),
            "hiredate" => descending ? query.OrderByDescending(e => e.HireDate) : query.OrderBy(e => e.HireDate),
            "departmentid" => descending ? query.OrderByDescending(e => e.DepartmentId) : query.OrderBy(e => e.DepartmentId),
            _ => descending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName)
        };
}
