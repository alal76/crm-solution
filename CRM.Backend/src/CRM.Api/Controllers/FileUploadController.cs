using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// Controller for handling file uploads (logos, photos)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileUploadController : ControllerBase
{
    private readonly ILogger<FileUploadController> _logger;
    private readonly IWebHostEnvironment _environment;
    
    // Allowed image extensions
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FileUploadController(ILogger<FileUploadController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Upload a company logo
    /// </summary>
    [HttpPost("logo")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FileUploadResponse>> UploadLogo(IFormFile file)
    {
        return await UploadFile(file, "logos");
    }

    /// <summary>
    /// Upload a user photo
    /// </summary>
    [HttpPost("user-photo")]
    public async Task<ActionResult<FileUploadResponse>> UploadUserPhoto(IFormFile file)
    {
        return await UploadFile(file, "users");
    }

    /// <summary>
    /// Upload a customer logo
    /// </summary>
    [HttpPost("customer-logo")]
    public async Task<ActionResult<FileUploadResponse>> UploadCustomerLogo(IFormFile file)
    {
        return await UploadFile(file, "customers");
    }

    /// <summary>
    /// Upload a contact photo
    /// </summary>
    [HttpPost("contact-photo")]
    public async Task<ActionResult<FileUploadResponse>> UploadContactPhoto(IFormFile file)
    {
        return await UploadFile(file, "contacts");
    }

    private async Task<ActionResult<FileUploadResponse>> UploadFile(IFormFile file, string folder)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5MB limit" });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: jpg, jpeg, png, gif, webp" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", folder);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL path
            var urlPath = $"/uploads/{folder}/{uniqueFileName}";
            
            _logger.LogInformation("File uploaded successfully: {UrlPath}", urlPath);

            return Ok(new FileUploadResponse
            {
                Success = true,
                Url = urlPath,
                FileName = uniqueFileName,
                OriginalFileName = file.FileName,
                FileSize = file.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { message = "Error uploading file" });
        }
    }

    /// <summary>
    /// Delete an uploaded file
    /// </summary>
    [HttpDelete]
    public ActionResult DeleteFile([FromQuery] string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                return BadRequest(new { message = "File path is required" });
            }

            // Sanitize path to prevent directory traversal
            var safePath = path.Replace("..", "").TrimStart('/');
            if (!safePath.StartsWith("uploads/"))
            {
                return BadRequest(new { message = "Invalid file path" });
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", safePath);
            
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                _logger.LogInformation("File deleted: {Path}", safePath);
                return Ok(new { message = "File deleted successfully" });
            }

            return NotFound(new { message = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { message = "Error deleting file" });
        }
    }
}

/// <summary>
/// Response DTO for file uploads
/// </summary>
public class FileUploadResponse
{
    public bool Success { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
