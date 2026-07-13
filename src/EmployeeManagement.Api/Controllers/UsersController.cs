using EmployeeManagement.Api.Models.DTOs.User;
using EmployeeManagement.Api.Services.Interfaces;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IUserService userService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    /// <summary>Administrator only: list all users.</summary>
    [HttpGet]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll()
        => Ok(await _userService.GetAllAsync());

    /// <summary>Administrator only: get a specific user by id.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<UserDto>> GetById(int id)
        => Ok(await _userService.GetByIdAsync(id));

    /// <summary>Administrator only: create a new user account (Employee or Administrator).</summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        var created = await _userService.CreateAsync(dto, _currentUser.UserId!.Value);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Administrator only: update any user's profile, active status and role.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<ActionResult<UserDto>> AdminUpdate(int id, [FromBody] UpdateUserDto dto)
        => Ok(await _userService.AdminUpdateAsync(id, dto));

    /// <summary>Administrator only: permanently remove a user.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id, _currentUser.UserId!.Value);
        return NoContent();
    }

    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
        => Ok(await _userService.GetByIdAsync(_currentUser.UserId!.Value));

    /// <summary>Updates the profile data of the currently authenticated user.</summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateProfileDto dto)
        => Ok(await _userService.UpdateOwnProfileAsync(_currentUser.UserId!.Value, dto));

    /// <summary>Changes the password of the currently authenticated user.</summary>
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDto dto)
    {
        await _userService.ChangePasswordAsync(_currentUser.UserId!.Value, dto);
        return NoContent();
    }

    /// <summary>Uploads/replaces the profile picture of the currently authenticated user.</summary>
    [HttpPost("me/profile-picture")]
    [RequestSizeLimit(6_000_000)]
    public async Task<ActionResult<object>> UploadMyProfilePicture(IFormFile file)
    {
        var path = await _userService.UpdateProfilePictureAsync(_currentUser.UserId!.Value, file);
        return Ok(new { profilePictureUrl = path });
    }
}
