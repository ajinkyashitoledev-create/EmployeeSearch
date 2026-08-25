using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Application.Validators;
using FluentAssertions;

namespace EmployeeSearch.UnitTests.Validators;

public class EmployeeSearchRequestValidatorTests
{
    private readonly EmployeeSearchRequestValidator _validator = new();

    [Fact]
    public void Validate_DefaultRequest_HasNoErrors()
    {
        var result = _validator.Validate(new EmployeeSearchRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PageSizeTooLarge_HasError()
    {
        var request = new EmployeeSearchRequest { PageSize = 500 };
        var result = _validator.Validate(request);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EmployeeSearchRequest.PageSize));
    }

    [Fact]
    public void Validate_UnknownSortField_HasError()
    {
        var request = new EmployeeSearchRequest { SortBy = "SocialSecurityNumber" };
        var result = _validator.Validate(request);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EmployeeSearchRequest.SortBy));
    }

    [Fact]
    public void Validate_MaxSalaryLessThanMinSalary_HasError()
    {
        var request = new EmployeeSearchRequest { MinSalary = 100000, MaxSalary = 50000 };
        var result = _validator.Validate(request);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EmployeeSearchRequest.MaxSalary));
    }

    [Fact]
    public void Validate_HiredBeforeEarlierThanHiredAfter_HasError()
    {
        var request = new EmployeeSearchRequest
        {
            HiredAfter = new DateTime(2024, 1, 1),
            HiredBefore = new DateTime(2020, 1, 1)
        };
        var result = _validator.Validate(request);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EmployeeSearchRequest.HiredBefore));
    }
}
