using EmployeeSearch.Domain.Common;

namespace EmployeeSearch.Domain.Entities;

/// <summary>
/// Base type for the Employee hierarchy. Mapped as Table-Per-Type (TPT):
/// this class owns the "Employees" table; Developer/Manager each get their
/// own table joined 1:1 on the shared primary key.
/// </summary>
public abstract class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public void GiveRaise(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Raise amount must be positive.");

        Salary += amount;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
