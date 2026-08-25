using EmployeeSearch.Application.DTOs;

namespace EmployeeSearch.API.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(AuthUserDto user);
}
