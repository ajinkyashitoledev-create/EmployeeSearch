using EmployeeSearch.Application.DTOs;
using FluentValidation;

namespace EmployeeSearch.Application.Validators;

public class CreateDeveloperRequestValidator : AbstractValidator<CreateDeveloperRequest>
{
    public CreateDeveloperRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.HireDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1));
        RuleFor(x => x.Salary).GreaterThan(0);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.ProgrammingLanguage).NotEmpty().MaximumLength(50);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 60);
        RuleFor(x => x.GitHubProfile).MaximumLength(200);
    }
}
