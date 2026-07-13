using EmployeeManagement.Api.Exceptions;
using EmployeeManagement.Api.Services.Interfaces;

namespace EmployeeManagement.Api.Services.Implementations;

public class FileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SaveProfilePictureAsync(int userId, IFormFile file)
    {
        if (file.Length == 0)
            throw new BadRequestException("The uploaded file is empty.");

        if (file.Length > MaxFileSizeBytes)
            throw new BadRequestException("The profile picture must be 5 MB or smaller.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new BadRequestException($"Unsupported file type '{extension}'. Allowed types: {string.Join(", ", AllowedExtensions)}.");

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "profile-pictures");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"user-{userId}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("Saved profile picture for user {UserId} at {Path}.", userId, fullPath);

        return $"/uploads/profile-pictures/{fileName}";
    }

    public void DeleteProfilePicture(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRoot, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete old profile picture at {Path}.", fullPath);
            }
        }
    }
}
