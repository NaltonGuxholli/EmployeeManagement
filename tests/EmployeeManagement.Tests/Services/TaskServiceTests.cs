using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Exceptions;
using EmployeeManagement.Api.Messaging;
using EmployeeManagement.Api.Models.DTOs.Task;
using EmployeeManagement.Api.Services.Implementations;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public class TaskServiceTests
{
    private static ApplicationUser MakeUser(int id, string email) => new()
    {
        Id = id,
        Email = email,
        UserName = email,
        FirstName = "First" + id,
        LastName = "Last" + id
    };

    private static async Task<ApplicationDbContext> SeedProjectWithMembersAsync(
        int projectId, params int[] memberIds)
    {
        var db = TestDbContextFactory.Create();

        foreach (var id in memberIds.Distinct())
        {
            db.Users.Add(MakeUser(id, $"user{id}@test.com"));
        }

        var project = new Project { Id = projectId, Name = "Test Project", CreatedAt = DateTime.UtcNow };
        foreach (var id in memberIds)
        {
            project.ProjectEmployees.Add(new ProjectEmployee { ProjectId = projectId, EmployeeId = id });
        }
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return db;
    }

    private static TaskService CreateSut(ApplicationDbContext db) =>
        new(db, Mock.Of<IEventPublisher>(), NullLogger<TaskService>.Instance);

    [Fact]
    public async Task CreateAsync_ShouldThrowForbidden_WhenEmployeeNotProjectMember()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2); // only user 2 is a member
        db.Users.Add(MakeUser(5, "outsider@test.com"));
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateTaskDto { Title = "New task", ProjectId = 1 };

        var act = async () => await sut.CreateAsync(dto, currentUserId: 5, isAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBadRequest_WhenAssigneeNotProjectMember()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2);
        db.Users.Add(MakeUser(9, "notmember@test.com"));
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new CreateTaskDto { Title = "New task", ProjectId = 1, AssignedToId = 9 };

        var act = async () => await sut.CreateAsync(dto, currentUserId: 2, isAdmin: false);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenEmployeeIsMemberAndAssigneeIsMember()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3);
        var sut = CreateSut(db);
        var dto = new CreateTaskDto { Title = "New task", ProjectId = 1, AssignedToId = 3 };

        var result = await sut.CreateAsync(dto, currentUserId: 2, isAdmin: false);

        result.Title.Should().Be("New task");
        result.AssignedToId.Should().Be(3);
        result.Status.Should().Be(EmployeeTaskStatus.Open);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbidden_WhenEmployeeNotAssignee()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3);
        var task = new TaskItem { Id = 1, Title = "T", ProjectId = 1, CreatedById = 2, AssignedToId = 3, Status = EmployeeTaskStatus.Open };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new UpdateTaskDto { Title = "Updated", Status = EmployeeTaskStatus.InProgress };

        // user 2 is a project member but the task is assigned to user 3, not them.
        var act = async () => await sut.UpdateAsync(1, dto, currentUserId: 2, isAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldSucceed_WhenEmployeeIsAssignee()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3);
        var task = new TaskItem { Id = 1, Title = "T", ProjectId = 1, CreatedById = 2, AssignedToId = 3, Status = EmployeeTaskStatus.Open };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);
        var dto = new UpdateTaskDto { Title = "Updated by assignee", Status = EmployeeTaskStatus.InProgress, AssignedToId = 3 };

        var result = await sut.UpdateAsync(1, dto, currentUserId: 3, isAdmin: false);

        result.Title.Should().Be("Updated by assignee");
        result.Status.Should().Be(EmployeeTaskStatus.InProgress);
    }

    [Fact]
    public async Task MarkCompletedAsync_ShouldThrowForbidden_WhenNotAssignedToEmployee()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3);
        var task = new TaskItem { Id = 1, Title = "T", ProjectId = 1, CreatedById = 2, AssignedToId = 3, Status = EmployeeTaskStatus.Open };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        var act = async () => await sut.MarkCompletedAsync(1, currentUserId: 2, isAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task MarkCompletedAsync_ShouldSetStatusAndCompletedAt_WhenAssignee()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3);
        var task = new TaskItem { Id = 1, Title = "T", ProjectId = 1, CreatedById = 2, AssignedToId = 3, Status = EmployeeTaskStatus.Open };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        var result = await sut.MarkCompletedAsync(1, currentUserId: 3, isAdmin: false);

        result.Status.Should().Be(EmployeeTaskStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkCompletedAsync_ShouldSucceed_ForAdminRegardlessOfAssignee()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3);
        var task = new TaskItem { Id = 1, Title = "T", ProjectId = 1, CreatedById = 2, AssignedToId = 3, Status = EmployeeTaskStatus.Open };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        var result = await sut.MarkCompletedAsync(1, currentUserId: 999, isAdmin: true);

        result.Status.Should().Be(EmployeeTaskStatus.Completed);
    }

    [Fact]
    public async Task AssignAsync_ShouldThrowForbidden_WhenEmployeeNeitherAssigneeNorCreatorAndTaskAlreadyAssigned()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3, 4);
        // Created by 2, assigned to 3: user 4 (a mere project member) must not be able to reassign it.
        var task = new TaskItem { Id = 1, Title = "T", ProjectId = 1, CreatedById = 2, AssignedToId = 3, Status = EmployeeTaskStatus.Open };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        var act = async () => await sut.AssignAsync(1, employeeId: 4, currentUserId: 4, isAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task AssignAsync_ShouldSucceed_WhenEmployeeIsTaskCreator()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2, 3, 4);
        var task = new TaskItem { Id = 1, Title = "T", ProjectId = 1, CreatedById = 2, AssignedToId = 3, Status = EmployeeTaskStatus.Open };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        var result = await sut.AssignAsync(1, employeeId: 4, currentUserId: 2, isAdmin: false);

        result.AssignedToId.Should().Be(4);
    }

    [Fact]
    public async Task GetAllAsync_ShouldOnlyReturnProjectTasks_ForNonAdmin()
    {
        await using var db = await SeedProjectWithMembersAsync(1, 2);
        var otherProject = new Project { Id = 2, Name = "Other", CreatedAt = DateTime.UtcNow };
        db.Projects.Add(otherProject);
        db.Tasks.Add(new TaskItem { Id = 1, Title = "In my project", ProjectId = 1, CreatedById = 2, Status = EmployeeTaskStatus.Open });
        db.Tasks.Add(new TaskItem { Id = 2, Title = "Not my project", ProjectId = 2, CreatedById = 2, Status = EmployeeTaskStatus.Open });
        await db.SaveChangesAsync();

        var sut = CreateSut(db);

        var result = await sut.GetAllAsync(currentUserId: 2, isAdmin: false, projectId: null);

        result.Should().ContainSingle(t => t.Id == 1);
    }
}
