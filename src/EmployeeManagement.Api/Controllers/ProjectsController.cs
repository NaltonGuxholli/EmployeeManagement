using EmployeeManagement.Api.Models.DTOs.Project;
using EmployeeManagement.Api.Services.Interfaces;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ICurrentUserService _currentUser;

    public ProjectsController(IProjectService projectService, ICurrentUserService currentUser)
    {
        _projectService = projectService;
        _currentUser = currentUser;
    }

    /// <summary>Administrators see all projects; employees see only the projects they belong to.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll()
        => Ok(await _projectService.GetAllAsync(_currentUser.UserId!.Value, _currentUser.IsAdministrator));

    /// <summary>Get a project by id. Employees must be a member; otherwise 403.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetById(int id)
        => Ok(await _projectService.GetByIdAsync(id, _currentUser.UserId!.Value, _currentUser.IsAdministrator));

    /// <summary>Administrator only: create a project.</summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto)
    {
        var created = await _projectService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Administrator only: update a project.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] UpdateProjectDto dto)
        => Ok(await _projectService.UpdateAsync(id, dto));

    /// <summary>Administrator only: remove a project. Fails (409) if it has open tasks.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<IActionResult> Delete(int id)
    {
        await _projectService.DeleteAsync(id, _currentUser.UserId!.Value);
        return NoContent();
    }

    /// <summary>Administrator only: add an employee to the project.</summary>
    [HttpPost("{id:int}/members")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<ProjectDto>> AddMember(int id, [FromBody] ProjectMembershipDto dto)
        => Ok(await _projectService.AddMemberAsync(id, dto.EmployeeId, _currentUser.UserId!.Value));

    /// <summary>Administrator only: remove an employee from the project.</summary>
    [HttpDelete("{id:int}/members/{employeeId:int}")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<ProjectDto>> RemoveMember(int id, int employeeId)
        => Ok(await _projectService.RemoveMemberAsync(id, employeeId, _currentUser.UserId!.Value));
}
