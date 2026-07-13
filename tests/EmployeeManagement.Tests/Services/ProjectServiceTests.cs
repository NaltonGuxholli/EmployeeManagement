using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Exceptions;
using EmployeeManagement.Api.Messaging;
using EmployeeManagement.Api.Models.DTOs.Project;
using EmployeeManagement.Api.Services.Implementations;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public class ProjectServiceTests
{
    private static ApplicationUser MakeUser(int id, string email) => new()
    {
        Id = id,
        Email = email,
        UserName = email,
        FirstName = "First" + id,
        LastName = "Last" + id
    };

    private static ProjectService CreateSut(ApplicationDbContext db, out Mock<IEventPublisher> publisher)
    {
        publisher = new Mock<IEventPublisher>();
        return new ProjectService(db, publisher.Object, NullLogger<ProjectService>.Instance);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowConflict_WhenProjectHasOpenTasks()
    {
        await using var db = TestDbContextFactory.Create();
        var admin = MakeUser(1, "admin@test.com");
        var employee = MakeUser(2, "emp@test.com");
        db.Users.AddRange(admin, employee);

        var project = new Project { Id = 1, Name = "Project X", CreatedAt = DateTime.UtcNow };
        project.Tasks.Add(new TaskItem
        {
            Id = 1,
            Title = "Open task",
            ProjectId = 1,
            CreatedById = admin.Id,
            Status = EmployeeTaskStatus.Open
        });
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var sut = CreateSut(db, out _);

        var act = async () => await sut.DeleteAsync(1, admin.Id);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenAllTasksCompleted()
    {
        await using var db = TestDbContextFactory.Create();
        var admin = MakeUser(1, "admin@test.com");
        db.Users.Add(admin);

        var project = new Project { Id = 1, Name = "Project Y", CreatedAt = DateTime.UtcNow };
        project.Tasks.Add(new TaskItem
        {
            Id = 1,
            Title = "Done task",
            ProjectId = 1,
            CreatedById = admin.Id,
            Status = EmployeeTaskStatus.Completed
        });
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var sut = CreateSut(db, out var publisher);

        await sut.DeleteAsync(1, admin.Id);

        (await db.Projects.FindAsync(1)).Should().BeNull();
        publisher.Verify(p => p.Publish("project.removed", It.IsAny<ProjectRemovedEvent>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFound_WhenProjectDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create();
        var sut = CreateSut(db, out _);

        var act = async () => await sut.DeleteAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddMemberAsync_ShouldThrowConflict_WhenEmployeeAlreadyMember()
    {
        await using var db = TestDbContextFactory.Create();
        var admin = MakeUser(1, "admin@test.com");
        var employee = MakeUser(2, "emp@test.com");
        db.Users.AddRange(admin, employee);

        var project = new Project { Id = 1, Name = "Project Z", CreatedAt = DateTime.UtcNow };
        project.ProjectEmployees.Add(new ProjectEmployee { ProjectId = 1, EmployeeId = 2 });
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var sut = CreateSut(db, out _);

        var act = async () => await sut.AddMemberAsync(1, 2, admin.Id);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldThrowConflict_WhenEmployeeHasOpenTasksInProject()
    {
        await using var db = TestDbContextFactory.Create();
        var admin = MakeUser(1, "admin@test.com");
        var employee = MakeUser(2, "emp@test.com");
        db.Users.AddRange(admin, employee);

        var project = new Project { Id = 1, Name = "Project W", CreatedAt = DateTime.UtcNow };
        project.ProjectEmployees.Add(new ProjectEmployee { ProjectId = 1, EmployeeId = 2 });
        project.Tasks.Add(new TaskItem
        {
            Id = 1,
            Title = "Assigned open task",
            ProjectId = 1,
            CreatedById = admin.Id,
            AssignedToId = 2,
            Status = EmployeeTaskStatus.Open
        });
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var sut = CreateSut(db, out _);

        var act = async () => await sut.RemoveMemberAsync(1, 2, admin.Id);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldOnlyReturnMemberProjects_ForNonAdmin()
    {
        await using var db = TestDbContextFactory.Create();
        var employee = MakeUser(2, "emp@test.com");
        var otherEmployee = MakeUser(3, "other@test.com");
        db.Users.AddRange(employee, otherEmployee);

        var myProject = new Project { Id = 1, Name = "Mine", CreatedAt = DateTime.UtcNow };
        myProject.ProjectEmployees.Add(new ProjectEmployee { ProjectId = 1, EmployeeId = 2 });

        var otherProject = new Project { Id = 2, Name = "NotMine", CreatedAt = DateTime.UtcNow };
        otherProject.ProjectEmployees.Add(new ProjectEmployee { ProjectId = 2, EmployeeId = 3 });

        db.Projects.AddRange(myProject, otherProject);
        await db.SaveChangesAsync();

        var sut = CreateSut(db, out _);

        var result = await sut.GetAllAsync(2, isAdmin: false);

        result.Should().ContainSingle(p => p.Id == 1);
    }
}
