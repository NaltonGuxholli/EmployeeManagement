using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Exceptions;
using EmployeeManagement.Api.Messaging;
using EmployeeManagement.Api.Models.DTOs.Project;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _db;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApplicationDbContext db, IEventPublisher eventPublisher, ILogger<ProjectService> logger)
    {
        _db = db;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(int currentUserId, bool isAdmin)
    {
        var query = _db.Projects
            .AsNoTracking()
            .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
            .Include(p => p.Tasks)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(p => p.ProjectEmployees.Any(pe => pe.EmployeeId == currentUserId));
        }

        var projects = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto> GetByIdAsync(int id, int currentUserId, bool isAdmin)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Project with id {id} was not found.");

        if (!isAdmin && !project.ProjectEmployees.Any(pe => pe.EmployeeId == currentUserId))
            throw new ForbiddenException("You are not a member of this project.");

        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Project {ProjectId} '{Name}' created.", project.Id, project.Name);

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto)
    {
        var project = await _db.Projects
            .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Project with id {id} was not found.");

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.DueDate = dto.DueDate;
        project.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return MapToDto(project);
    }

    public async Task DeleteAsync(int id, int removedByUserId)
    {
        var project = await _db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Project with id {id} was not found.");

        var hasOpenTasks = project.Tasks.Any(t => t.Status != EmployeeTaskStatus.Completed);
        if (hasOpenTasks)
            throw new ConflictException("Cannot remove a project that still has open tasks. Complete or remove all tasks first.");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Project {ProjectId} removed by user {UserId}.", id, removedByUserId);
        _eventPublisher.Publish("project.removed",
            new ProjectRemovedEvent(id, project.Name, removedByUserId, DateTime.UtcNow));
    }

    public async Task<ProjectDto> AddMemberAsync(int projectId, int employeeId, int addedByUserId)
    {
        var project = await _db.Projects
            .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId)
            ?? throw new NotFoundException($"Project with id {projectId} was not found.");

        var employee = await _db.Users.FirstOrDefaultAsync(u => u.Id == employeeId)
            ?? throw new NotFoundException($"User with id {employeeId} was not found.");

        if (project.ProjectEmployees.Any(pe => pe.EmployeeId == employeeId))
            throw new ConflictException("This employee is already a member of the project.");

        project.ProjectEmployees.Add(new ProjectEmployee
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            AddedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        _eventPublisher.Publish("project.member.added",
            new ProjectMemberAddedEvent(projectId, employeeId, addedByUserId, DateTime.UtcNow));

        return MapToDto(project);
    }

    public async Task<ProjectDto> RemoveMemberAsync(int projectId, int employeeId, int removedByUserId)
    {
        var project = await _db.Projects
            .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId)
            ?? throw new NotFoundException($"Project with id {projectId} was not found.");

        var membership = project.ProjectEmployees.FirstOrDefault(pe => pe.EmployeeId == employeeId)
            ?? throw new NotFoundException("This employee is not a member of the project.");

        var hasOpenAssignedTasks = project.Tasks.Any(t =>
            t.AssignedToId == employeeId && t.Status != EmployeeTaskStatus.Completed);
        if (hasOpenAssignedTasks)
            throw new ConflictException("Cannot remove this employee: they have open tasks assigned in this project. Reassign or complete those tasks first.");

        project.ProjectEmployees.Remove(membership);
        await _db.SaveChangesAsync();

        _eventPublisher.Publish("project.member.removed",
            new ProjectMemberRemovedEvent(projectId, employeeId, removedByUserId, DateTime.UtcNow));

        return MapToDto(project);
    }

    private static ProjectDto MapToDto(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        CreatedAt = project.CreatedAt,
        DueDate = project.DueDate,
        IsActive = project.IsActive,
        OpenTaskCount = project.Tasks.Count(t => t.Status != EmployeeTaskStatus.Completed),
        TotalTaskCount = project.Tasks.Count,
        Members = project.ProjectEmployees.Select(pe => new ProjectMemberDto
        {
            UserId = pe.EmployeeId,
            FullName = pe.Employee.FullName,
            Email = pe.Employee.Email ?? string.Empty
        }).ToList()
    };
}
