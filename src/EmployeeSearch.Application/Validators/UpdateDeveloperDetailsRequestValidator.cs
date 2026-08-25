using EmployeeSearch.Application.DTOs;
using FluentValidation;

namespace EmployeeSearch.Application.Validators;

public class UpdateDeveloperDetailsRequestValidator : AbstractValidator<UpdateDeveloperDetailsRequest>
{
    public UpdateDeveloperDetailsRequestValidator()
    {
        RuleFor(x => x.ProgrammingLanguage).NotEmpty().MaximumLength(50);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 60);
        RuleFor(x => x.GitHubProfile).MaximumLength(200);
    }
}

public class UpdateManagerDetailsRequestValidator : AbstractValidator<UpdateManagerDetailsRequest>
{
    public UpdateManagerDetailsRequestValidator()
    {
        RuleFor(x => x.TeamSize).InclusiveBetween(0, 500);
        RuleFor(x => x.Bonus).GreaterThanOrEqualTo(0);
    }
}
