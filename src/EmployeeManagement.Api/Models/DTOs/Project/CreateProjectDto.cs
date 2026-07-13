using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Models.DTOs.Project;

public class CreateProjectDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
}

public class UpdateProjectDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ProjectMembershipDto
{
    [Required]
    public int EmployeeId { get; set; }
}
