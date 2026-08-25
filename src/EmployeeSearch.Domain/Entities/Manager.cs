namespace EmployeeSearch.Domain.Entities;

public class Manager : Employee
{
    public int TeamSize { get; set; }
    public decimal Bonus { get; set; }
}
