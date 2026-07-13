using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Exceptions;
using EmployeeManagement.Api.Messaging;
using EmployeeManagement.Api.Models.DTOs.User;
using EmployeeManagement.Api.Services.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IFileStorageService fileStorageService,
        IEventPublisher eventPublisher,
        ILogger<UserService> logger)
    {
        _db = db;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.LastName).ToListAsync();
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            result.Add(await MapToDtoAsync(user));
        }
        return result;
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException($"User with id {id} was not found.");
        return await MapToDtoAsync(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, int createdByUserId)
    {
        if (dto.Role != UserRoles.Administrator && dto.Role != UserRoles.Employee)
            throw new BadRequestException($"Role must be either '{UserRoles.Administrator}' or '{UserRoles.Employee}'.");

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            throw new ConflictException($"A user with email '{dto.Email}' already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Position = dto.Position,
            Department = dto.Department,
            PhoneNumber = dto.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Could not create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, dto.Role);

        _logger.LogInformation("Administrator {AdminId} created user {UserId} ({Email}) with role {Role}.",
            createdByUserId, user.Id, user.Email, dto.Role);

        _eventPublisher.Publish("user.created",
            new UserCreatedEvent(user.Id, user.Email ?? string.Empty, dto.Role, createdByUserId, DateTime.UtcNow));

        return await MapToDtoAsync(user);
    }

    public async Task<UserDto> AdminUpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException($"User with id {id} was not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Position = dto.Position;
        user.Department = dto.Department;
        user.PhoneNumber = dto.PhoneNumber;
        user.IsActive = dto.IsActive;

        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrWhiteSpace(dto.Role) &&
            (dto.Role == UserRoles.Administrator || dto.Role == UserRoles.Employee))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);
            }
        }

        _logger.LogInformation("User {UserId} updated by administrator.", id);
        return await MapToDtoAsync(user);
    }

    public async Task DeleteAsync(int id, int removedByUserId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException($"User with id {id} was not found.");

        if (id == removedByUserId)
            throw new ConflictException("You cannot delete your own account.");

        var hasOpenTasks = await _db.Tasks.AnyAsync(t => t.AssignedToId == id && t.Status != EmployeeTaskStatus.Completed);
        if (hasOpenTasks)
            throw new ConflictException("Cannot remove a user who has open (non-completed) tasks assigned. Reassign or complete them first.");

        _fileStorageService.DeleteProfilePicture(user.ProfilePicturePath);

        await _userManager.DeleteAsync(user);

        _logger.LogInformation("User {UserId} removed by administrator {AdminId}.", id, removedByUserId);
        _eventPublisher.Publish("user.removed", new UserRemovedEvent(id, removedByUserId, DateTime.UtcNow));
    }

    public async Task<UserDto> UpdateOwnProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Position = dto.Position;
        user.Department = dto.Department;
        user.PhoneNumber = dto.PhoneNumber;

        await _userManager.UpdateAsync(user);
        return await MapToDtoAsync(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Could not change password: {errors}");
        }
    }

    public async Task<string> UpdateProfilePictureAsync(int userId, IFormFile file)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found.");

        var oldPath = user.ProfilePicturePath;
        var newPath = await _fileStorageService.SaveProfilePictureAsync(userId, file);

        user.ProfilePicturePath = newPath;
        await _userManager.UpdateAsync(user);

        _fileStorageService.DeleteProfilePicture(oldPath);

        return newPath;
    }

    private async Task<UserDto> MapToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Position = user.Position,
            Department = user.Department,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePicturePath,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }
}
