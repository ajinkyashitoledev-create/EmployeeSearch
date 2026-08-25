using EmployeeSearch.Application.Common.Exceptions;
using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Domain.Entities;
using FluentValidation;

namespace EmployeeSearch.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterRequest> _registerValidator;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IValidator<RegisterRequest> registerValidator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _registerValidator = registerValidator;
    }

    public async Task<AuthUserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _unitOfWork.Users.ExistsByUsernameOrEmailAsync(request.Username, request.Email, cancellationToken))
            throw new ConflictException($"A user with username '{request.Username}' or email '{request.Email}' already exists.");

        var user = new ApplicationUser
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = "User"
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<AuthUserDto?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username, cancellationToken);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            return null;

        return ToDto(user);
    }

    private static AuthUserDto ToDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role
    };
}
