using EmployeeSearch.Application.DTOs;

namespace EmployeeSearch.Application.Services;

public interface IAuthService
{
    Task<AuthUserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthUserDto?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
}
