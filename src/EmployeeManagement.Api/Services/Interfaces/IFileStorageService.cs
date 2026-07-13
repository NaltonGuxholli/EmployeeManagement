namespace EmployeeManagement.Api.Services.Interfaces;

public interface IFileStorageService
{
    /// <summary>Saves an uploaded profile picture and returns its relative (web-accessible) path.</summary>
    Task<string> SaveProfilePictureAsync(int userId, IFormFile file);

    /// <summary>Deletes a previously stored profile picture, if present.</summary>
    void DeleteProfilePicture(string? relativePath);
}
