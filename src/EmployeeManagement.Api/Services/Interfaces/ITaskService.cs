using EmployeeManagement.Api.Models.DTOs.Task;

namespace EmployeeManagement.Api.Services.Interfaces;

public interface ITaskService
{
    /// <summary>
    /// Admin: all tasks (optionally filtered by project).
    /// Employee: all tasks belonging to projects they are a member of (read-only for tasks not theirs).
    /// </summary>
    Task<IReadOnlyList<TaskDto>> GetAllAsync(int currentUserId, bool isAdmin, int? projectId);

    Task<TaskDto> GetByIdAsync(int id, int currentUserId, bool isAdmin);

    /// <summary>Tasks assigned to the current user.</summary>
    Task<IReadOnlyList<TaskDto>> GetMyTasksAsync(int currentUserId);

    /// <summary>
    /// Employees may create tasks only within projects they belong to, and may only assign
    /// them to fellow project members. Admins may create tasks for any project/assignee.
    /// </summary>
    Task<TaskDto> CreateAsync(CreateTaskDto dto, int currentUserId, bool isAdmin);

    /// <summary>
    /// Full update. Employees may only update tasks currently assigned to them;
    /// admins may update any task.
    /// </summary>
    Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, int currentUserId, bool isAdmin);

    /// <summary>Assign (or re-assign) a task to a project member.</summary>
    Task<TaskDto> AssignAsync(int id, int employeeId, int currentUserId, bool isAdmin);

    /// <summary>Marks a task completed. Employees may only complete their own tasks.</summary>
    Task<TaskDto> MarkCompletedAsync(int id, int currentUserId, bool isAdmin);

    /// <summary>Toggles task completion status. If completed, marks as open; if open/in-progress, marks as completed. Employees may only toggle their own tasks.</summary>
    Task<TaskDto> ToggleCompletionAsync(int id, int currentUserId, bool isAdmin);

    /// <summary>Admin only.</summary>
    Task DeleteAsync(int id, int removedByUserId);
}
