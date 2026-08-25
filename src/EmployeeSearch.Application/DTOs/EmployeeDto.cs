using System.Text.Json.Serialization;

namespace EmployeeSearch.Application.DTOs;

/// <summary>
/// Base response DTO mirroring the Employee domain hierarchy. Serialized
/// polymorphically so a search result can mix Developers and Managers in
/// one JSON array, each carrying its own extra fields plus an "employeeType"
/// discriminator.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "employeeType")]
[JsonDerivedType(typeof(DeveloperDto), typeDiscriminator: "developer")]
[JsonDerivedType(typeof(ManagerDto), typeDiscriminator: "manager")]
public abstract class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}

public class DeveloperDto : EmployeeDto
{
    public string ProgrammingLanguage { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? GitHubProfile { get; set; }
}

public class ManagerDto : EmployeeDto
{
    public int TeamSize { get; set; }
    public decimal Bonus { get; set; }
}
