using EmployeeSearch.Application.Common.Interfaces;

namespace EmployeeSearch.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context, IEmployeeRepository employees, IDepartmentRepository departments, IUserRepository users)
    {
        _context = context;
        Employees = employees;
        Departments = departments;
        Users = users;
    }

    public IEmployeeRepository Employees { get; }
    public IDepartmentRepository Departments { get; }
    public IUserRepository Users { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
