using EmployeeSearch.Application.Common.Exceptions;
using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Application.Common.Models;
using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Application.Mapping;
using EmployeeSearch.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmployeeSearch.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IValidator<EmployeeSearchRequest> _searchValidator;
    private readonly IValidator<CreateDeveloperRequest> _createDeveloperValidator;
    private readonly IValidator<CreateManagerRequest> _createManagerValidator;
    private readonly IValidator<UpdateEmployeeRequest> _updateValidator;
    private readonly IValidator<UpdateDeveloperDetailsRequest> _updateDeveloperDetailsValidator;
    private readonly IValidator<UpdateManagerDetailsRequest> _updateManagerDetailsValidator;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher,
        IValidator<EmployeeSearchRequest> searchValidator,
        IValidator<CreateDeveloperRequest> createDeveloperValidator,
        IValidator<CreateManagerRequest> createManagerValidator,
        IValidator<UpdateEmployeeRequest> updateValidator,
        IValidator<UpdateDeveloperDetailsRequest> updateDeveloperDetailsValidator,
        IValidator<UpdateManagerDetailsRequest> updateManagerDetailsValidator,
        ILogger<EmployeeService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
        _searchValidator = searchValidator;
        _createDeveloperValidator = createDeveloperValidator;
        _createManagerValidator = createManagerValidator;
        _updateValidator = updateValidator;
        _updateDeveloperDetailsValidator = updateDeveloperDetailsValidator;
        _updateManagerDetailsValidator = updateManagerDetailsValidator;
        _logger = logger;
    }

    public async Task<PagedResult<EmployeeDto>> SearchAsync(EmployeeSearchRequest request, CancellationToken cancellationToken = default)
    {
        await _searchValidator.ValidateAndThrowAsync(request, cancellationToken);

        var (items, totalCount) = await _unitOfWork.Employees.SearchAsync(request, cancellationToken);

        return new PagedResult<EmployeeDto>
        {
            Items = items.ToDtoList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EmployeeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), id);

        return employee.ToDto();
    }

    public async Task<DeveloperDto> CreateDeveloperAsync(CreateDeveloperRequest request, CancellationToken cancellationToken = default)
    {
        await _createDeveloperValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureDepartmentExistsAsync(request.DepartmentId, cancellationToken);
        await EnsureEmailIsUniqueAsync(request.Email, excludeEmployeeId: null, cancellationToken);

        var developer = request.ToEntity();
        await _unitOfWork.Employees.AddAsync(developer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await NotifyWelcomeAsync(developer, cancellationToken);

        _logger.LogInformation("Created Developer {EmployeeId} ({Email})", developer.Id, developer.Email);

        return (DeveloperDto)developer.ToDto();
    }

    public async Task<ManagerDto> CreateManagerAsync(CreateManagerRequest request, CancellationToken cancellationToken = default)
    {
        await _createManagerValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureDepartmentExistsAsync(request.DepartmentId, cancellationToken);
        await EnsureEmailIsUniqueAsync(request.Email, excludeEmployeeId: null, cancellationToken);

        var manager = request.ToEntity();
        await _unitOfWork.Employees.AddAsync(manager, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await NotifyWelcomeAsync(manager, cancellationToken);

        _logger.LogInformation("Created Manager {EmployeeId} ({Email})", manager.Id, manager.Email);

        return (ManagerDto)manager.ToDto();
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), id);

        await EnsureDepartmentExistsAsync(request.DepartmentId, cancellationToken);
        await EnsureEmailIsUniqueAsync(request.Email, excludeEmployeeId: id, cancellationToken);

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.PhoneNumber = request.PhoneNumber;
        employee.Salary = request.Salary;
        employee.DepartmentId = request.DepartmentId;
        employee.ModifiedAtUtc = DateTime.UtcNow;

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.ToDto();
    }

    public async Task<DeveloperDto> UpdateDeveloperDetailsAsync(int id, UpdateDeveloperDetailsRequest request, CancellationToken cancellationToken = default)
    {
        await _updateDeveloperDetailsValidator.ValidateAndThrowAsync(request, cancellationToken);

        var developer = await _unitOfWork.Employees.GetDeveloperByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Developer), id);

        developer.ProgrammingLanguage = request.ProgrammingLanguage;
        developer.YearsOfExperience = request.YearsOfExperience;
        developer.GitHubProfile = request.GitHubProfile;
        developer.ModifiedAtUtc = DateTime.UtcNow;

        _unitOfWork.Employees.Update(developer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (DeveloperDto)developer.ToDto();
    }

    public async Task<ManagerDto> UpdateManagerDetailsAsync(int id, UpdateManagerDetailsRequest request, CancellationToken cancellationToken = default)
    {
        await _updateManagerDetailsValidator.ValidateAndThrowAsync(request, cancellationToken);

        var manager = await _unitOfWork.Employees.GetManagerByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Manager), id);

        manager.TeamSize = request.TeamSize;
        manager.Bonus = request.Bonus;
        manager.ModifiedAtUtc = DateTime.UtcNow;

        _unitOfWork.Employees.Update(manager);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (ManagerDto)manager.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), id);

        _unitOfWork.Employees.Remove(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDepartmentExistsAsync(int departmentId, CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Departments.ExistsAsync(departmentId, cancellationToken))
            throw new NotFoundException(nameof(Department), departmentId);
    }

    private async Task EnsureEmailIsUniqueAsync(string email, int? excludeEmployeeId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Employees.ExistsWithEmailAsync(email, excludeEmployeeId, cancellationToken))
            throw new ConflictException($"An employee with email '{email}' already exists.");
    }

    private Task NotifyWelcomeAsync(Employee employee, CancellationToken cancellationToken)
    {
        var body = $"Hi {employee.FirstName}, your employee profile has been created.";

        var notifications = new[]
        {
            new ChannelNotification(
                NotificationChannel.Email,
                new NotificationMessage(employee.Email, "Welcome to the company!", body)),
            new ChannelNotification(
                NotificationChannel.Sms,
                new NotificationMessage(employee.PhoneNumber, "Welcome!", body))
        };

        return _notificationDispatcher.DispatchAsync(notifications, cancellationToken);
    }
}
