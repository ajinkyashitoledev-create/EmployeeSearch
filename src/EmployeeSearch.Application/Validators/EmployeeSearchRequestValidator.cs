using EmployeeSearch.Application.DTOs;
using FluentValidation;

namespace EmployeeSearch.Application.Validators;

public class EmployeeSearchRequestValidator : AbstractValidator<EmployeeSearchRequest>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "FirstName", "LastName", "Salary", "HireDate", "DepartmentId"
    };

    public EmployeeSearchRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).Must(field => AllowedSortFields.Contains(field))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}");
        RuleFor(x => x.MaxSalary).GreaterThanOrEqualTo(x => x.MinSalary)
            .When(x => x.MinSalary.HasValue && x.MaxSalary.HasValue)
            .WithMessage("MaxSalary must be greater than or equal to MinSalary.");
        RuleFor(x => x.HiredBefore).GreaterThanOrEqualTo(x => x.HiredAfter)
            .When(x => x.HiredAfter.HasValue && x.HiredBefore.HasValue)
            .WithMessage("HiredBefore must be greater than or equal to HiredAfter.");
    }
}
