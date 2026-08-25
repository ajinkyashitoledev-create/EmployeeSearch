namespace EmployeeSearch.Application.DTOs;

public class UpdateDeveloperDetailsRequest
{
    public string ProgrammingLanguage { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? GitHubProfile { get; set; }
}

public class UpdateManagerDetailsRequest
{
    public int TeamSize { get; set; }
    public decimal Bonus { get; set; }
}
