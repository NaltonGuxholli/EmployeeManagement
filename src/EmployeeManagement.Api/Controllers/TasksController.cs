using EmployeeManagement.Api.Models.DTOs.Task;
using EmployeeManagement.Api.Services.Interfaces;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ICurrentUserService _currentUser;

    public TasksController(ITaskService taskService, ICurrentUserService currentUser)
    {
        _taskService = taskService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Administrators see all tasks (optionally filtered by project).
    /// Employees see all tasks belonging to projects they are members of (read-only unless assigned to them).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetAll([FromQuery] int? projectId)
        => Ok(await _taskService.GetAllAsync(_currentUser.UserId!.Value, _currentUser.IsAdministrator, projectId));

    /// <summary>Tasks assigned to the currently authenticated employee.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetMyTasks()
        => Ok(await _taskService.GetMyTasksAsync(_currentUser.UserId!.Value));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id)
        => Ok(await _taskService.GetByIdAsync(id, _currentUser.UserId!.Value, _currentUser.IsAdministrator));

    /// <summary>
    /// Employees can create tasks only for projects they belong to, and can only assign
    /// them to fellow project members. Administrators can create tasks for any project.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto dto)
    {
        var created = await _taskService.CreateAsync(dto, _currentUser.UserId!.Value, _currentUser.IsAdministrator);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Full update of a task. Employees may only update tasks currently assigned to them.
    /// Administrators may update any task.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> Update(int id, [FromBody] UpdateTaskDto dto)
        => Ok(await _taskService.UpdateAsync(id, dto, _currentUser.UserId!.Value, _currentUser.IsAdministrator));

    /// <summary>Assign (or re-assign) a task to a project member.</summary>
    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult<TaskDto>> Assign(int id, [FromBody] AssignTaskDto dto)
        => Ok(await _taskService.AssignAsync(id, dto.EmployeeId, _currentUser.UserId!.Value, _currentUser.IsAdministrator));

    /// <summary>Marks a task as completed. Employees may only complete tasks assigned to them.</summary>
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<TaskDto>> Complete(int id)
        => Ok(await _taskService.MarkCompletedAsync(id, _currentUser.UserId!.Value, _currentUser.IsAdministrator));

    /// <summary>Toggles task completion status (completed ↔ open). Employees may only toggle tasks assigned to them.</summary>
    [HttpPost("{id:int}/toggle-completion")]
    public async Task<ActionResult<TaskDto>> ToggleCompletion(int id)
        => Ok(await _taskService.ToggleCompletionAsync(id, _currentUser.UserId!.Value, _currentUser.IsAdministrator));

    /// <summary>Administrator only: remove a task.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id, _currentUser.UserId!.Value);
        return NoContent();
    }
}
