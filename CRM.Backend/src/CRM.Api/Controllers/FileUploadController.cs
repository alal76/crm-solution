using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace CRM.API.Controllers;

/// <summary>
/// Controller for handling file uploads (logos, photos)
/// Supports image resizing for navigation logo (150x150) and login logo (400px)
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
    
    // Logo size constants
    private const int NavLogoSize = 150;
    private const int LoginLogoWidth = 400;

    public FileUploadController(ILogger<FileUploadController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Upload a company navigation logo - resized to 150x150 pixels
    /// Returns base64 data URL for database storage
    /// </summary>
    [HttpPost("logo")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FileUploadResponse>> UploadLogo(IFormFile file)
    {
        // For nav logos, resize to 150x150 and return base64 data URL
        return await UploadFileAsBase64WithResize(file, NavLogoSize, NavLogoSize);
    }

    /// <summary>
    /// Upload a company login page logo - resized to 400px width (maintains aspect ratio)
    /// Returns base64 data URL for database storage
    /// </summary>
    [HttpPost("login-logo")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FileUploadResponse>> UploadLoginLogo(IFormFile file)
    {
        // For login logos, resize to 400px width maintaining aspect ratio
        return await UploadFileAsBase64WithResize(file, LoginLogoWidth, null);
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
    /// Upload file with resizing, return as base64 data URL (for database storage)
    /// If height is null, maintains aspect ratio based on width
    /// </summary>
    private async Task<ActionResult<FileUploadResponse>> UploadFileAsBase64WithResize(IFormFile file, int width, int? height)
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

            // Read file into memory
            using var inputStream = new MemoryStream();
            await file.CopyToAsync(inputStream);
            inputStream.Position = 0;

            // Load and resize image
            using var image = await Image.LoadAsync(inputStream);
            
            int targetHeight;
            if (height.HasValue)
            {
                // Fixed dimensions (square logo)
                targetHeight = height.Value;
            }
            else
            {
                // Maintain aspect ratio based on width
                var aspectRatio = (double)image.Height / image.Width;
                targetHeight = (int)(width * aspectRatio);
            }

            // Resize the image
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, targetHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));

            // Convert to PNG for consistent output
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            var bytes = outputStream.ToArray();
            var base64 = Convert.ToBase64String(bytes);

            // Create data URL (always PNG after resize)
            var dataUrl = $"data:image/png;base64,{base64}";

            _logger.LogInformation("File uploaded and resized to {Width}x{Height}: {FileName}, original size: {OriginalSize} bytes, new size: {NewSize} bytes", 
                width, targetHeight, file.FileName, file.Length, bytes.Length);

            return Ok(new FileUploadResponse
            {
                Success = true,
                Url = dataUrl,
                FileName = file.FileName,
                OriginalFileName = file.FileName,
                FileSize = bytes.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading and resizing file");
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
