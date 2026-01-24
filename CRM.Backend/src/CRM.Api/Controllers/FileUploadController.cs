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
    /// Upload a company logo - returns base64 data URL for database storage
    /// </summary>
    [HttpPost("logo")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FileUploadResponse>> UploadLogo(IFormFile file)
    {
        // For logos, return base64 data URL to store in database (persists across pod restarts)
        return await UploadFileAsBase64(file);
    }

    /// <summary>
    /// Upload a user photo
    /// </summary>
    [HttpPost("user-photo")]
    public async Task<ActionResult<FileUploadResponse>> UploadUserPhoto(IFormFile file)
    {
        return await UploadFileAsBase64(file);
    }

    /// <summary>
    /// Upload a customer logo
    /// </summary>
    [HttpPost("customer-logo")]
    public async Task<ActionResult<FileUploadResponse>> UploadCustomerLogo(IFormFile file)
    {
        return await UploadFileAsBase64(file);
    }

    /// <summary>
    /// Upload a contact photo
    /// </summary>
    [HttpPost("contact-photo")]
    public async Task<ActionResult<FileUploadResponse>> UploadContactPhoto(IFormFile file)
    {
        return await UploadFileAsBase64(file);
    }

    /// <summary>
    /// Upload file and return as base64 data URL (for database storage)
    /// </summary>
    private async Task<ActionResult<FileUploadResponse>> UploadFileAsBase64(IFormFile file)
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

            // Read file into memory and convert to base64
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            var base64 = Convert.ToBase64String(bytes);

            // Determine MIME type
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            // Create data URL
            var dataUrl = $"data:{mimeType};base64,{base64}";

            _logger.LogInformation("File uploaded as base64: {FileName}, size: {Size} bytes", file.FileName, file.Length);

            return Ok(new FileUploadResponse
            {
                Success = true,
                Url = dataUrl,
                FileName = file.FileName,
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
