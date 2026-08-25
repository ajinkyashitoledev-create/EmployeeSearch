namespace EmployeeSearch.Application.DTOs;

public class EmployeeSearchRequest
{
    /// <summary>Free-text match against first name, last name, and email.</summary>
    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public EmployeeTypeFilter EmployeeType { get; set; } = EmployeeTypeFilter.All;
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public DateTime? HiredAfter { get; set; }
    public DateTime? HiredBefore { get; set; }

    public string SortBy { get; set; } = "LastName";
    public bool SortDescending { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
