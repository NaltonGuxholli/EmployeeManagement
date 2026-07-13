using EmployeeManagement.Api.Models.DTOs.User;

namespace EmployeeManagement.Api.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync();
    Task<UserDto> GetByIdAsync(int id);

    /// <summary>Admin only: create a new user account with a role.</summary>
    Task<UserDto> CreateAsync(CreateUserDto dto, int createdByUserId);

    /// <summary>Admin only: update any user's profile, active flag and role.</summary>
    Task<UserDto> AdminUpdateAsync(int id, UpdateUserDto dto);

    /// <summary>Admin only: permanently remove a user account.</summary>
    Task DeleteAsync(int id, int removedByUserId);

    /// <summary>Employee self-service: update own profile fields.</summary>
    Task<UserDto> UpdateOwnProfileAsync(int userId, UpdateProfileDto dto);

    /// <summary>Employee self-service: change own password.</summary>
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);

    /// <summary>Employee or admin: upload/replace the profile picture for the given user.</summary>
    Task<string> UpdateProfilePictureAsync(int userId, IFormFile file);
}
