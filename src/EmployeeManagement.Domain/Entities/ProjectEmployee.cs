namespace EmployeeManagement.Domain.Entities;

/// <summary>
/// Join entity representing the membership of an <see cref="ApplicationUser"/>
/// (employee) within a <see cref="Project"/>.
/// </summary>
public class ProjectEmployee
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int EmployeeId { get; set; }
    public ApplicationUser Employee { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
