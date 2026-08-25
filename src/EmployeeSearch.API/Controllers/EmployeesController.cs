using EmployeeSearch.Application.Common.Models;
using EmployeeSearch.Application.DTOs;
using EmployeeSearch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSearch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>Search employees with filtering, sorting, and paging.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> Search(
        [FromQuery] EmployeeSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single employee (Developer or Manager) by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new Developer.</summary>
    [HttpPost("developers")]
    [ProducesResponseType(typeof(DeveloperDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeveloperDto>> CreateDeveloper(
        [FromBody] CreateDeveloperRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateDeveloperAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Create a new Manager.</summary>
    [HttpPost("managers")]
    [ProducesResponseType(typeof(ManagerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerDto>> CreateManager(
        [FromBody] CreateManagerRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateManagerAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update common Employee fields (name, contact info, salary, department).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDto>> Update(
        int id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Partially update Developer-specific fields (language, experience, GitHub profile).</summary>
    [HttpPatch("developers/{id:int}")]
    [ProducesResponseType(typeof(DeveloperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeveloperDto>> PatchDeveloper(
        int id, [FromBody] UpdateDeveloperDetailsRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateDeveloperDetailsAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Partially update Manager-specific fields (team size, bonus).</summary>
    [HttpPatch("managers/{id:int}")]
    [ProducesResponseType(typeof(ManagerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerDto>> PatchManager(
        int id, [FromBody] UpdateManagerDetailsRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateManagerDetailsAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete an employee. Requires the Admin role.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _employeeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
