using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSearch.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<ApplicationUser>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(u => u.Username == username || u.Email == email, cancellationToken);
}
