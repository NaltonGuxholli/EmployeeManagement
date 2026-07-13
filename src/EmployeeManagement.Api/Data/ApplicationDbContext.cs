using EmployeeManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Data;

/// <summary>
/// EF Core Code-First database context. Extends IdentityDbContext so that
/// ASP.NET Core Identity tables (Users, Roles, Claims, etc.) are managed
/// alongside the domain tables (Projects, Tasks, ProjectEmployees).
/// </summary>
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectEmployee> ProjectEmployees => Set<ProjectEmployee>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename default Identity tables to something more readable (optional).
        builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
        builder.Entity<ApplicationRole>(b => b.ToTable("Roles"));
        builder.Entity<IdentityUserRole<int>>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<int>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<int>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<int>>(b => b.ToTable("UserTokens"));

        // Composite key for the ProjectEmployee join entity (many-to-many with payload).
        builder.Entity<ProjectEmployee>(b =>
        {
            b.HasKey(pe => new { pe.ProjectId, pe.EmployeeId });

            b.HasOne(pe => pe.Project)
                .WithMany(p => p.ProjectEmployees)
                .HasForeignKey(pe => pe.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pe => pe.Employee)
                .WithMany(e => e.ProjectMemberships)
                .HasForeignKey(pe => pe.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskItem relationships.
        builder.Entity<TaskItem>(b =>
        {
            b.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            b.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        });

        builder.Entity<Project>(b =>
        {
            b.HasIndex(p => p.Name);
        });
    }
}
