using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Application.Validators;
using FluentAssertions;

namespace EmployeeSearch.UnitTests.Validators;

public class CreateDeveloperRequestValidatorTests
{
    private readonly CreateDeveloperRequestValidator _validator = new();

    private static CreateDeveloperRequest ValidRequest() => new()
    {
        FirstName = "Asha",
        LastName = "Rao",
        Email = "asha.rao@example.com",
        PhoneNumber = "+919900011122",
        HireDate = DateTime.UtcNow.Date,
        Salary = 95000,
        DepartmentId = 1,
        ProgrammingLanguage = "C#",
        YearsOfExperience = 6
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.Validate(ValidRequest());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_MissingFirstName_HasError(string? firstName)
    {
        var request = ValidRequest();
        request.FirstName = firstName!;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDeveloperRequest.FirstName));
    }

    [Fact]
    public void Validate_InvalidEmail_HasError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDeveloperRequest.Email));
    }

    [Fact]
    public void Validate_NegativeSalary_HasError()
    {
        var request = ValidRequest();
        request.Salary = -1;

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDeveloperRequest.Salary));
    }

    [Fact]
    public void Validate_YearsOfExperienceOutOfRange_HasError()
    {
        var request = ValidRequest();
        request.YearsOfExperience = 200;

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDeveloperRequest.YearsOfExperience));
    }

    [Fact]
    public void Validate_DepartmentIdZero_HasError()
    {
        var request = ValidRequest();
        request.DepartmentId = 0;

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDeveloperRequest.DepartmentId));
    }
}
