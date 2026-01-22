using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace CRM.API.Controllers;

/// <summary>
/// Controller for managing system settings
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(ISystemSettingsService settingsService, ILogger<SystemSettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get current system settings (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> GetSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system settings");
            return StatusCode(500, "Error retrieving system settings");
        }
    }

    /// <summary>
    /// Get module status for permission checking (all authenticated users)
    /// </summary>
    [HttpGet("modules")]
    public async Task<ActionResult<ModuleStatusDto>> GetModuleStatus()
    {
        try
        {
            var moduleStatus = await _settingsService.GetModuleStatusAsync();
            return Ok(moduleStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module status");
            return StatusCode(500, "Error retrieving module status");
        }
    }

    /// <summary>
    /// Update system settings (admin only)
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> UpdateSettings([FromBody] UpdateSystemSettingsRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;
            
            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system settings");
            return StatusCode(500, "Error updating system settings");
        }
    }

    /// <summary>
    /// Toggle a specific module on/off (admin only)
    /// </summary>
    [HttpPost("modules/{moduleName}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> ToggleModule(string moduleName, [FromQuery] bool enabled)
    {
        try
        {
            var request = new UpdateSystemSettingsRequest();
            
            switch (moduleName.ToLower())
            {
                case "customers": request.CustomersEnabled = enabled; break;
                case "contacts": request.ContactsEnabled = enabled; break;
                case "leads": request.LeadsEnabled = enabled; break;
                case "opportunities": request.OpportunitiesEnabled = enabled; break;
                case "products": request.ProductsEnabled = enabled; break;
                case "services": request.ServicesEnabled = enabled; break;
                case "campaigns": request.CampaignsEnabled = enabled; break;
                case "quotes": request.QuotesEnabled = enabled; break;
                case "tasks": request.TasksEnabled = enabled; break;
                case "activities": request.ActivitiesEnabled = enabled; break;
                case "notes": request.NotesEnabled = enabled; break;
                case "workflows": request.WorkflowsEnabled = enabled; break;
                case "reports": request.ReportsEnabled = enabled; break;
                case "dashboard": request.DashboardEnabled = enabled; break;
                default:
                    return BadRequest($"Unknown module: {moduleName}");
            }
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;
            
            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling module {ModuleName}", moduleName);
            return StatusCode(500, $"Error toggling module {moduleName}");
        }
    }

    /// <summary>
    /// Remove company logo (admin only)
    /// </summary>
    [HttpDelete("logo")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> RemoveLogo()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;
            
            var request = new UpdateSystemSettingsRequest { CompanyLogoUrl = "" };
            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing logo");
            return StatusCode(500, "Error removing logo");
        }
    }

    /// <summary>
    /// Reset branding to defaults (admin only)
    /// </summary>
    [HttpPost("reset-branding")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> ResetBranding()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;
            
            var request = new UpdateSystemSettingsRequest 
            { 
                CompanyLogoUrl = "",
                PrimaryColor = "#6750A4",
                SecondaryColor = "#625B71",
                SelectedPaletteId = null,
                SelectedPaletteName = null
            };
            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting branding");
            return StatusCode(500, "Error resetting branding");
        }
    }

    /// <summary>
    /// Upload SSL certificate and private key (admin only)
    /// </summary>
    [HttpPost("ssl/upload")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> UploadSslCertificate(
        [FromForm] IFormFile certificate,
        [FromForm] IFormFile? privateKey)
    {
        try
        {
            if (certificate == null || certificate.Length == 0)
                return BadRequest(new { message = "Certificate file is required" });

            var certDir = Path.Combine(Directory.GetCurrentDirectory(), "ssl");
            Directory.CreateDirectory(certDir);

            var certFileName = $"certificate_{DateTime.UtcNow:yyyyMMddHHmmss}.crt";
            var certPath = Path.Combine(certDir, certFileName);

            // Save certificate file
            using (var stream = new FileStream(certPath, FileMode.Create))
            {
                await certificate.CopyToAsync(stream);
            }

            string? keyPath = null;
            if (privateKey != null && privateKey.Length > 0)
            {
                var keyFileName = $"private_{DateTime.UtcNow:yyyyMMddHHmmss}.key";
                keyPath = Path.Combine(certDir, keyFileName);
                using var keyStream = new FileStream(keyPath, FileMode.Create);
                await privateKey.CopyToAsync(keyStream);
            }

            // Try to read certificate info
            DateTime? expiry = null;
            string? subject = null;
            try
            {
                var certBytes = await System.IO.File.ReadAllBytesAsync(certPath);
                var x509 = new X509Certificate2(certBytes);
                expiry = x509.NotAfter;
                subject = x509.Subject;
                x509.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not parse certificate details");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            var request = new UpdateSystemSettingsRequest
            {
                SslCertificatePath = certPath,
                SslPrivateKeyPath = keyPath,
                SslCertificateExpiry = expiry,
                SslCertificateSubject = subject
            };

            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            
            _logger.LogInformation("SSL certificate uploaded by user {UserId}", userId);
            
            return Ok(new { 
                message = "SSL certificate uploaded successfully",
                certificatePath = certPath,
                privateKeyPath = keyPath,
                expiry = expiry,
                subject = subject,
                settings 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading SSL certificate");
            return StatusCode(500, "Error uploading SSL certificate");
        }
    }

    /// <summary>
    /// Toggle HTTPS mode (admin only)
    /// </summary>
    [HttpPost("ssl/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> ToggleHttps([FromBody] ToggleHttpsRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            var updateRequest = new UpdateSystemSettingsRequest
            {
                HttpsEnabled = request.Enabled,
                ForceHttpsRedirect = request.ForceRedirect
            };

            var settings = await _settingsService.UpdateSettingsAsync(updateRequest, userId);
            
            _logger.LogInformation("HTTPS mode toggled to {Enabled} by user {UserId}", request.Enabled, userId);
            
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling HTTPS mode");
            return StatusCode(500, "Error toggling HTTPS mode");
        }
    }

    /// <summary>
    /// Get SSL certificate status (admin only)
    /// </summary>
    [HttpGet("ssl/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetSslStatus()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            
            return Ok(new
            {
                httpsEnabled = settings.HttpsEnabled,
                forceRedirect = settings.ForceHttpsRedirect,
                hasCertificate = !string.IsNullOrEmpty(settings.SslCertificatePath),
                hasPrivateKey = !string.IsNullOrEmpty(settings.SslPrivateKeyPath),
                certificateExpiry = settings.SslCertificateExpiry,
                certificateSubject = settings.SslCertificateSubject,
                isExpired = settings.SslCertificateExpiry.HasValue && settings.SslCertificateExpiry < DateTime.UtcNow,
                expiresInDays = settings.SslCertificateExpiry.HasValue 
                    ? (int)(settings.SslCertificateExpiry.Value - DateTime.UtcNow).TotalDays 
                    : (int?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SSL status");
            return StatusCode(500, "Error getting SSL status");
        }
    }

    /// <summary>
    /// Remove SSL certificate (admin only)
    /// </summary>
    [HttpDelete("ssl")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> RemoveSslCertificate()
    {
        try
        {
            var currentSettings = await _settingsService.GetSettingsAsync();
            
            // Delete certificate files if they exist
            if (!string.IsNullOrEmpty(currentSettings.SslCertificatePath) && System.IO.File.Exists(currentSettings.SslCertificatePath))
            {
                System.IO.File.Delete(currentSettings.SslCertificatePath);
            }
            if (!string.IsNullOrEmpty(currentSettings.SslPrivateKeyPath) && System.IO.File.Exists(currentSettings.SslPrivateKeyPath))
            {
                System.IO.File.Delete(currentSettings.SslPrivateKeyPath);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            var request = new UpdateSystemSettingsRequest
            {
                HttpsEnabled = false,
                ForceHttpsRedirect = false,
                SslCertificatePath = "",
                SslPrivateKeyPath = "",
                SslCertificateExpiry = null,
                SslCertificateSubject = ""
            };

            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            
            _logger.LogInformation("SSL certificate removed by user {UserId}", userId);
            
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing SSL certificate");
            return StatusCode(500, "Error removing SSL certificate");
        }
    }

    /// <summary>
    /// Update navigation order (admin only)
    /// </summary>
    [HttpPut("navigation/order")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> UpdateNavigationOrder([FromBody] UpdateNavOrderRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            var updateRequest = new UpdateSystemSettingsRequest
            {
                NavOrderConfig = request.NavOrderConfig
            };

            var settings = await _settingsService.UpdateSettingsAsync(updateRequest, userId);
            
            _logger.LogInformation("Navigation order updated by user {UserId}", userId);
            
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating navigation order");
            return StatusCode(500, "Error updating navigation order");
        }
    }

    /// <summary>
    /// Toggle demo database mode
    /// </summary>
    [HttpPost("demo/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> ToggleDemoMode([FromBody] ToggleDemoModeRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            var updateRequest = new UpdateSystemSettingsRequest
            {
                UseDemoDatabase = request.Enabled
            };

            var settings = await _settingsService.UpdateSettingsAsync(updateRequest, userId);
            
            _logger.LogInformation("Demo mode {Action} by user {UserId}", request.Enabled ? "enabled" : "disabled", userId);
            
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling demo mode");
            return StatusCode(500, "Error toggling demo mode");
        }
    }

    /// <summary>
    /// Get demo database status
    /// </summary>
    [HttpGet("demo/status")]
    public async Task<ActionResult<DemoStatusResponse>> GetDemoStatus()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(new DemoStatusResponse
            {
                UseDemoDatabase = settings.UseDemoDatabase,
                DemoDataSeeded = settings.DemoDataSeeded,
                DemoDataLastSeeded = settings.DemoDataLastSeeded
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting demo status");
            return StatusCode(500, "Error getting demo status");
        }
    }
}

/// <summary>
/// Request to toggle HTTPS mode
/// </summary>
public class ToggleHttpsRequest
{
    public bool Enabled { get; set; }
    public bool ForceRedirect { get; set; }
}

/// <summary>
/// Request to update navigation order
/// </summary>
public class UpdateNavOrderRequest
{
    public string NavOrderConfig { get; set; } = string.Empty;
}

/// <summary>
/// Request to toggle demo mode
/// </summary>
public class ToggleDemoModeRequest
{
    public bool Enabled { get; set; }
}

/// <summary>
/// Response for demo status
/// </summary>
public class DemoStatusResponse
{
    public bool UseDemoDatabase { get; set; }
    public bool DemoDataSeeded { get; set; }
    public DateTime? DemoDataLastSeeded { get; set; }
}
