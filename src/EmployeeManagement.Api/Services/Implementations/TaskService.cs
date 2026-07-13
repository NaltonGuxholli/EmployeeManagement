using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Exceptions;
using EmployeeManagement.Api.Messaging;
using EmployeeManagement.Api.Models.DTOs.Task;
using EmployeeManagement.Api.Services.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _db;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext db, IEventPublisher eventPublisher, ILogger<TaskService> logger)
    {
        _db = db;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskDto>> GetAllAsync(int currentUserId, bool isAdmin, int? projectId)
    {
        var query = _db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (!isAdmin)
        {
            // Employees only ever see tasks for projects they belong to.
            var myProjectIds = await _db.ProjectEmployees
                .Where(pe => pe.EmployeeId == currentUserId)
                .Select(pe => pe.ProjectId)
                .ToListAsync();

            query = query.Where(t => myProjectIds.Contains(t.ProjectId));
        }

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskDto> GetByIdAsync(int id, int currentUserId, bool isAdmin)
    {
        var task = await LoadTaskAsync(id);
        await EnsureCanViewAsync(task, currentUserId, isAdmin);
        return MapToDto(task);
    }

    public async Task<IReadOnlyList<TaskDto>> GetMyTasksAsync(int currentUserId)
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.AssignedToId == currentUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, int currentUserId, bool isAdmin)
    {
        var project = await _db.Projects
            .Include(p => p.ProjectEmployees)
            .FirstOrDefaultAsync(p => p.Id == dto.ProjectId)
            ?? throw new NotFoundException($"Project with id {dto.ProjectId} was not found.");

        if (!isAdmin && !project.ProjectEmployees.Any(pe => pe.EmployeeId == currentUserId))
            throw new ForbiddenException("You can only create tasks for projects you are a member of.");

        if (dto.AssignedToId.HasValue)
        {
            var assigneeIsMember = project.ProjectEmployees.Any(pe => pe.EmployeeId == dto.AssignedToId.Value);
            if (!assigneeIsMember)
                throw new BadRequestException("The task can only be assigned to an employee who is a member of the project.");
        }

        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < DateTime.UtcNow.Date)
            throw new BadRequestException("Due date cannot be in the past. Please select today or a future date.");

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            ProjectId = dto.ProjectId,
            AssignedToId = dto.AssignedToId,
            CreatedById = currentUserId,
            Status = EmployeeTaskStatus.Open,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        _eventPublisher.Publish("task.created",
            new TaskCreatedEvent(task.Id, task.Title, task.ProjectId, currentUserId, DateTime.UtcNow));

        if (task.AssignedToId.HasValue)
            await PublishAssignedEventAsync(task, currentUserId);

        return MapToDto(await LoadTaskAsync(task.Id));
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, int currentUserId, bool isAdmin)
    {
        var task = await LoadTaskAsync(id);

        if (!isAdmin && task.AssignedToId != currentUserId)
            throw new ForbiddenException("You can only modify tasks that are assigned to you.");

        if (dto.AssignedToId.HasValue && dto.AssignedToId != task.AssignedToId)
        {
            var isMember = await _db.ProjectEmployees
                .AnyAsync(pe => pe.ProjectId == task.ProjectId && pe.EmployeeId == dto.AssignedToId.Value);
            if (!isMember)
                throw new BadRequestException("The task can only be assigned to an employee who is a member of the project.");
        }

        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < DateTime.UtcNow.Date)
            throw new BadRequestException("Due date cannot be in the past. Please select today or a future date.");

        var wasCompleted = task.Status == EmployeeTaskStatus.Completed;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.AssignedToId = dto.AssignedToId;
        task.DueDate = dto.DueDate;
        task.Status = dto.Status;
        task.CompletedAt = dto.Status == EmployeeTaskStatus.Completed
            ? (task.CompletedAt ?? DateTime.UtcNow)
            : null;

        await _db.SaveChangesAsync();

        if (!wasCompleted && task.Status == EmployeeTaskStatus.Completed)
        {
            _eventPublisher.Publish("task.completed",
                new TaskCompletedEvent(task.Id, task.Title, task.ProjectId, currentUserId, DateTime.UtcNow));
        }

        return MapToDto(await LoadTaskAsync(id));
    }

    public async Task<TaskDto> AssignAsync(int id, int employeeId, int currentUserId, bool isAdmin)
    {
        var task = await LoadTaskAsync(id);

        if (!isAdmin)
        {
            // An employee may assign a task when: it is currently unassigned, it is already
            // assigned to them (re-assigning it away), or they are the task's creator.
            // They may never reassign a task that is assigned to someone else and that they
            // did not create - that would be "modifying" a task that isn't theirs.
            var canAssign = task.AssignedToId is null
                || task.AssignedToId == currentUserId
                || task.CreatedById == currentUserId;

            if (!canAssign)
                throw new ForbiddenException("You can only assign tasks that are unassigned, assigned to you, or created by you.");

            var currentUserIsMember = await _db.ProjectEmployees
                .AnyAsync(pe => pe.ProjectId == task.ProjectId && pe.EmployeeId == currentUserId);
            if (!currentUserIsMember)
                throw new ForbiddenException("You are not a member of the project this task belongs to.");
        }


        var assigneeIsMember = await _db.ProjectEmployees
            .AnyAsync(pe => pe.ProjectId == task.ProjectId && pe.EmployeeId == employeeId);
        if (!assigneeIsMember)
            throw new BadRequestException("The task can only be assigned to an employee who is a member of the project.");

        task.AssignedToId = employeeId;
        await _db.SaveChangesAsync();

        await PublishAssignedEventAsync(task, currentUserId);

        return MapToDto(await LoadTaskAsync(id));
    }

    public async Task<TaskDto> MarkCompletedAsync(int id, int currentUserId, bool isAdmin)
    {
        var task = await LoadTaskAsync(id);

        if (!isAdmin && task.AssignedToId != currentUserId)
            throw new ForbiddenException("You can only complete tasks that are assigned to you.");

        if (task.Status != EmployeeTaskStatus.Completed)
        {
            task.Status = EmployeeTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _eventPublisher.Publish("task.completed",
                new TaskCompletedEvent(task.Id, task.Title, task.ProjectId, currentUserId, DateTime.UtcNow));
        }

        return MapToDto(await LoadTaskAsync(id));
    }

    public async Task<TaskDto> ToggleCompletionAsync(int id, int currentUserId, bool isAdmin)
    {
        var task = await LoadTaskAsync(id);

        if (!isAdmin && task.AssignedToId != currentUserId)
            throw new ForbiddenException("You can only toggle completion status for tasks assigned to you.");

        var wasCompleted = task.Status == EmployeeTaskStatus.Completed;

        if (wasCompleted)
        {
            // Mark as open if it was completed
            task.Status = EmployeeTaskStatus.Open;
            task.CompletedAt = null;
        }
        else
        {
            // Mark as completed if it was open or in-progress
            task.Status = EmployeeTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        if (!wasCompleted && task.Status == EmployeeTaskStatus.Completed)
        {
            _eventPublisher.Publish("task.completed",
                new TaskCompletedEvent(task.Id, task.Title, task.ProjectId, currentUserId, DateTime.UtcNow));
        }

        return MapToDto(await LoadTaskAsync(id));
    }

    public async Task DeleteAsync(int id, int removedByUserId)
    {
        var task = await LoadTaskAsync(id);
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        _eventPublisher.Publish("task.removed",
            new TaskRemovedEvent(id, task.ProjectId, removedByUserId, DateTime.UtcNow));
    }

    private async Task<TaskItem> LoadTaskAsync(int id)
    {
        return await _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException($"Task with id {id} was not found.");
    }

    private async System.Threading.Tasks.Task EnsureCanViewAsync(TaskItem task, int currentUserId, bool isAdmin)
    {
        if (isAdmin) return;

        var isMember = await _db.ProjectEmployees
            .AnyAsync(pe => pe.ProjectId == task.ProjectId && pe.EmployeeId == currentUserId);
        if (!isMember)
            throw new ForbiddenException("You are not a member of the project this task belongs to.");
    }

    private async System.Threading.Tasks.Task PublishAssignedEventAsync(TaskItem task, int assignedByUserId)
    {
        if (!task.AssignedToId.HasValue) return;

        var assignee = task.AssignedTo ?? await _db.Users.FirstOrDefaultAsync(u => u.Id == task.AssignedToId.Value);
        if (assignee is null) return;

        _eventPublisher.Publish("task.assigned", new TaskAssignedEvent(
            task.Id, task.Title, task.ProjectId, assignee.Id, assignee.Email ?? string.Empty,
            assignedByUserId, DateTime.UtcNow));
    }

    private static TaskDto MapToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        ProjectId = task.ProjectId,
        ProjectName = task.Project?.Name ?? string.Empty,
        AssignedToId = task.AssignedToId,
        AssignedToName = task.AssignedTo?.FullName,
        CreatedById = task.CreatedById,
        CreatedByName = task.CreatedBy?.FullName ?? string.Empty,
        Status = task.Status,
        CreatedAt = task.CreatedAt,
        DueDate = task.DueDate,
        CompletedAt = task.CompletedAt
    };
}
