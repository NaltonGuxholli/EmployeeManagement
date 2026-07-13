using EmployeeManagement.Api.Models.DTOs.Project;

namespace EmployeeManagement.Api.Services.Interfaces;

public interface IProjectService
{
    /// <summary>Admin sees all projects; employee sees only projects they belong to.</summary>
    Task<IReadOnlyList<ProjectDto>> GetAllAsync(int currentUserId, bool isAdmin);

    Task<ProjectDto> GetByIdAsync(int id, int currentUserId, bool isAdmin);

    Task<ProjectDto> CreateAsync(CreateProjectDto dto);

    Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto);

    /// <summary>Admin only. Throws if the project still has open (non-completed) tasks.</summary>
    Task DeleteAsync(int id, int removedByUserId);

    Task<ProjectDto> AddMemberAsync(int projectId, int employeeId, int addedByUserId);

    Task<ProjectDto> RemoveMemberAsync(int projectId, int employeeId, int removedByUserId);
}
