using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Domain.Entities;

namespace EmployeeSearch.Application.Mapping;

public static class EmployeeMappingExtensions
{
    public static EmployeeDto ToDto(this Employee employee) => employee switch
    {
        Developer dev => new DeveloperDto
        {
            Id = dev.Id,
            FirstName = dev.FirstName,
            LastName = dev.LastName,
            FullName = dev.FullName,
            Email = dev.Email,
            PhoneNumber = dev.PhoneNumber,
            HireDate = dev.HireDate,
            Salary = dev.Salary,
            DepartmentId = dev.DepartmentId,
            DepartmentName = dev.Department?.Name,
            ProgrammingLanguage = dev.ProgrammingLanguage,
            YearsOfExperience = dev.YearsOfExperience,
            GitHubProfile = dev.GitHubProfile
        },
        Manager mgr => new ManagerDto
        {
            Id = mgr.Id,
            FirstName = mgr.FirstName,
            LastName = mgr.LastName,
            FullName = mgr.FullName,
            Email = mgr.Email,
            PhoneNumber = mgr.PhoneNumber,
            HireDate = mgr.HireDate,
            Salary = mgr.Salary,
            DepartmentId = mgr.DepartmentId,
            DepartmentName = mgr.Department?.Name,
            TeamSize = mgr.TeamSize,
            Bonus = mgr.Bonus
        },
        _ => throw new NotSupportedException($"Unknown employee type '{employee.GetType().Name}'.")
    };

    public static IReadOnlyList<EmployeeDto> ToDtoList(this IEnumerable<Employee> employees) =>
        employees.Select(ToDto).ToList();

    public static Developer ToEntity(this CreateDeveloperRequest request) => new()
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        PhoneNumber = request.PhoneNumber,
        HireDate = request.HireDate,
        Salary = request.Salary,
        DepartmentId = request.DepartmentId,
        ProgrammingLanguage = request.ProgrammingLanguage,
        YearsOfExperience = request.YearsOfExperience,
        GitHubProfile = request.GitHubProfile
    };

    public static Manager ToEntity(this CreateManagerRequest request) => new()
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        PhoneNumber = request.PhoneNumber,
        HireDate = request.HireDate,
        Salary = request.Salary,
        DepartmentId = request.DepartmentId,
        TeamSize = request.TeamSize,
        Bonus = request.Bonus
    };

    public static DepartmentDto ToDto(this Department department) => new()
    {
        Id = department.Id,
        Name = department.Name,
        Location = department.Location,
        EmployeeCount = department.Employees.Count
    };
}
