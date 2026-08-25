using EmployeeSearch.Application.Common.Exceptions;
using EmployeeSearch.Application.Common.Interfaces;
using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Application.Services;
using EmployeeSearch.Application.Validators;
using EmployeeSearch.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeSearch.UnitTests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IDepartmentRepository> _departmentRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<INotificationDispatcher> _dispatcher = new();

    public EmployeeServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Employees).Returns(_employeeRepo.Object);
        _unitOfWork.SetupGet(u => u.Departments).Returns(_departmentRepo.Object);
    }

    private EmployeeService CreateSut() => new(
        _unitOfWork.Object,
        _dispatcher.Object,
        new EmployeeSearchRequestValidator(),
        new CreateDeveloperRequestValidator(),
        new CreateManagerRequestValidator(),
        new UpdateEmployeeRequestValidator(),
        new UpdateDeveloperDetailsRequestValidator(),
        new UpdateManagerDetailsRequestValidator(),
        Mock.Of<ILogger<EmployeeService>>());

    private static CreateDeveloperRequest ValidDeveloperRequest() => new()
    {
        FirstName = "Asha",
        LastName = "Rao",
        Email = "asha.rao@example.com",
        PhoneNumber = "+919900011122",
        HireDate = DateTime.UtcNow.Date.AddYears(-1),
        Salary = 95000,
        DepartmentId = 1,
        ProgrammingLanguage = "C#",
        YearsOfExperience = 6
    };

    [Fact]
    public async Task CreateDeveloperAsync_ValidRequest_AddsDeveloperAndDispatchesNotifications()
    {
        _departmentRepo.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _employeeRepo.Setup(r => r.ExistsWithEmailAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.CreateDeveloperAsync(ValidDeveloperRequest());

        result.ProgrammingLanguage.Should().Be("C#");
        result.FullName.Should().Be("Asha Rao");
        _employeeRepo.Verify(r => r.AddAsync(It.IsAny<Developer>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<ChannelNotification>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDeveloperAsync_DepartmentDoesNotExist_ThrowsNotFoundException()
    {
        _departmentRepo.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();
        var act = async () => await sut.CreateDeveloperAsync(ValidDeveloperRequest());

        await act.Should().ThrowAsync<NotFoundException>();
        _employeeRepo.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDeveloperAsync_DuplicateEmail_ThrowsConflictException()
    {
        _departmentRepo.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _employeeRepo.Setup(r => r.ExistsWithEmailAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();
        var act = async () => await sut.CreateDeveloperAsync(ValidDeveloperRequest());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateDeveloperAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = ValidDeveloperRequest();
        request.FirstName = string.Empty;
        request.Email = "not-an-email";

        var sut = CreateSut();
        var act = async () => await sut.CreateDeveloperAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _departmentRepo.Verify(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_EmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepo.Setup(r => r.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var sut = CreateSut();
        var act = async () => await sut.GetByIdAsync(42);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingManager_ReturnsManagerDto()
    {
        var manager = new Manager
        {
            Id = 7,
            FirstName = "Vikram",
            LastName = "Shah",
            Email = "vikram.shah@example.com",
            PhoneNumber = "+919900033344",
            DepartmentId = 1,
            TeamSize = 8,
            Bonus = 15000
        };
        _employeeRepo.Setup(r => r.GetByIdWithDetailsAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(manager);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(7);

        result.Should().BeOfType<ManagerDto>();
        ((ManagerDto)result).TeamSize.Should().Be(8);
    }

    [Fact]
    public async Task DeleteAsync_EmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Employee?)null);

        var sut = CreateSut();
        var act = async () => await sut.DeleteAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
        _employeeRepo.Verify(r => r.Remove(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task UpdateDeveloperDetailsAsync_DeveloperNotFound_ThrowsNotFoundException()
    {
        _employeeRepo.Setup(r => r.GetDeveloperByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Developer?)null);

        var sut = CreateSut();
        var act = async () => await sut.UpdateDeveloperDetailsAsync(5, new UpdateDeveloperDetailsRequest
        {
            ProgrammingLanguage = "Rust",
            YearsOfExperience = 3
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SearchAsync_DelegatesToRepositoryAndMapsPagedResult()
    {
        var developer = new Developer { Id = 1, FirstName = "Asha", LastName = "Rao", ProgrammingLanguage = "C#" };
        _employeeRepo.Setup(r => r.SearchAsync(It.IsAny<EmployeeSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Employee> { developer }, 1));

        var sut = CreateSut();
        var result = await sut.SearchAsync(new EmployeeSearchRequest { PageNumber = 1, PageSize = 10 });

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle().Which.Should().BeOfType<DeveloperDto>();
    }
}
