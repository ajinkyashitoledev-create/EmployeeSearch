namespace EmployeeSearch.Domain.Entities;

public class Developer : Employee
{
    public string ProgrammingLanguage { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? GitHubProfile { get; set; }
}
