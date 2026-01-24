using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
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
    private readonly IDbContextResolver? _contextResolver;
    private readonly IDemoModeState _demoModeState;
    private readonly IDatabaseSyncService? _databaseSyncService;

    public SystemSettingsController(
        ISystemSettingsService settingsService, 
        ILogger<SystemSettingsController> logger,
        IDemoModeState demoModeState,
        IDbContextResolver? contextResolver = null,
        IDatabaseSyncService? databaseSyncService = null)
    {
        _settingsService = settingsService;
        _logger = logger;
        _demoModeState = demoModeState;
        _contextResolver = contextResolver;
        _databaseSyncService = databaseSyncService;
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
    /// Generate a self-signed SSL certificate (admin only)
    /// </summary>
    [HttpPost("ssl/generate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GenerateSelfSignedCertificate([FromBody] GenerateCertificateRequest? request)
    {
        try
        {
            var certDir = Path.Combine(Directory.GetCurrentDirectory(), "ssl");
            Directory.CreateDirectory(certDir);

            var commonName = request?.CommonName ?? "localhost";
            var validityDays = request?.ValidityDays ?? 365;
            var password = request?.Password ?? "CrmSslCert2024";
            var pfxPath = Path.Combine(certDir, "server.pfx");
            var crtPath = Path.Combine(certDir, "server.crt");
            var keyPath = Path.Combine(certDir, "server.key");

            // Generate certificate using System.Security.Cryptography
            using var rsa = System.Security.Cryptography.RSA.Create(2048);
            var distinguishedName = new X500DistinguishedName($"CN={commonName}, O=CRM, C=US");
            
            var sanBuilder = new System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddDnsName(commonName);
            sanBuilder.AddDnsName("crm-api");
            sanBuilder.AddDnsName("crm-api.crm-app.svc.cluster.local");
            sanBuilder.AddIpAddress(System.Net.IPAddress.Parse("127.0.0.1"));

            var certRequest = new System.Security.Cryptography.X509Certificates.CertificateRequest(
                distinguishedName,
                rsa,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                System.Security.Cryptography.RSASignaturePadding.Pkcs1);

            certRequest.CertificateExtensions.Add(
                new System.Security.Cryptography.X509Certificates.X509KeyUsageExtension(
                    System.Security.Cryptography.X509Certificates.X509KeyUsageFlags.DigitalSignature |
                    System.Security.Cryptography.X509Certificates.X509KeyUsageFlags.KeyEncipherment,
                    critical: true));

            certRequest.CertificateExtensions.Add(
                new System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension(
                    new System.Security.Cryptography.OidCollection
                    {
                        new System.Security.Cryptography.Oid("1.3.6.1.5.5.7.3.1") // Server Authentication
                    }, critical: true));

            certRequest.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = certRequest.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(validityDays));

            // Export PFX
            var pfxBytes = certificate.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx, password);
            await System.IO.File.WriteAllBytesAsync(pfxPath, pfxBytes);

            // Export CRT (public certificate)
            var crtBytes = certificate.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Cert);
            await System.IO.File.WriteAllBytesAsync(crtPath, crtBytes);

            // Export private key (PEM format)
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
            await System.IO.File.WriteAllTextAsync(keyPath, privateKeyPem);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            // Update settings
            var updateRequest = new UpdateSystemSettingsRequest
            {
                HttpsEnabled = true,
                SslCertificatePath = pfxPath,
                SslPrivateKeyPath = keyPath,
                SslCertificateExpiry = certificate.NotAfter,
                SslCertificateSubject = certificate.Subject
            };

            await _settingsService.UpdateSettingsAsync(updateRequest, userId);
            
            _logger.LogInformation("Self-signed certificate generated by user {UserId} for CN={CommonName}", userId, commonName);

            return Ok(new
            {
                message = "Self-signed certificate generated successfully. Restart the server to apply.",
                commonName = commonName,
                validityDays = validityDays,
                expiresOn = certificate.NotAfter,
                subject = certificate.Subject,
                pfxPath = pfxPath,
                requiresRestart = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating self-signed certificate");
            return StatusCode(500, new { message = $"Error generating certificate: {ex.Message}" });
        }
    }

    /// <summary>
    /// Upload SSL certificate (supports PFX, CRT+KEY) (admin only)
    /// </summary>
    [HttpPost("ssl/upload")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> UploadSslCertificate(
        [FromForm] IFormFile certificate,
        [FromForm] IFormFile? privateKey,
        [FromForm] string? password)
    {
        try
        {
            if (certificate == null || certificate.Length == 0)
                return BadRequest(new { message = "Certificate file is required" });

            var certDir = Path.Combine(Directory.GetCurrentDirectory(), "ssl");
            Directory.CreateDirectory(certDir);

            var fileName = certificate.FileName.ToLower();
            var isPfx = fileName.EndsWith(".pfx") || fileName.EndsWith(".p12");
            
            string pfxPath;
            string? keyPath = null;
            DateTime? expiry = null;
            string? subject = null;
            
            if (isPfx)
            {
                // Handle PFX file directly
                pfxPath = Path.Combine(certDir, "server.pfx");
                using (var stream = new FileStream(pfxPath, FileMode.Create))
                {
                    await certificate.CopyToAsync(stream);
                }

                try
                {
                    var certPassword = password ?? "";
                    using var x509 = new X509Certificate2(pfxPath, certPassword, X509KeyStorageFlags.Exportable);
                    expiry = x509.NotAfter;
                    subject = x509.Subject;
                    
                    // Re-export with standard password if different
                    if (password != "CrmSslCert2024")
                    {
                        var pfxBytes = x509.Export(X509ContentType.Pfx, "CrmSslCert2024");
                        await System.IO.File.WriteAllBytesAsync(pfxPath, pfxBytes);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not parse PFX certificate - may need password");
                    return BadRequest(new { message = "Could not load PFX certificate. Please check the password." });
                }
            }
            else
            {
                // Handle CRT + KEY files
                var crtPath = Path.Combine(certDir, "server.crt");
                using (var stream = new FileStream(crtPath, FileMode.Create))
                {
                    await certificate.CopyToAsync(stream);
                }

                if (privateKey == null || privateKey.Length == 0)
                {
                    return BadRequest(new { message = "Private key file is required for CRT/PEM certificates" });
                }

                keyPath = Path.Combine(certDir, "server.key");
                using (var keyStream = new FileStream(keyPath, FileMode.Create))
                {
                    await privateKey.CopyToAsync(keyStream);
                }

                // Create PFX from CRT + KEY
                try
                {
                    var certPem = await System.IO.File.ReadAllTextAsync(crtPath);
                    var keyPem = await System.IO.File.ReadAllTextAsync(keyPath);
                    
                    using var x509 = X509Certificate2.CreateFromPem(certPem, keyPem);
                    expiry = x509.NotAfter;
                    subject = x509.Subject;
                    
                    // Export as PFX
                    pfxPath = Path.Combine(certDir, "server.pfx");
                    var pfxBytes = x509.Export(X509ContentType.Pfx, "CrmSslCert2024");
                    await System.IO.File.WriteAllBytesAsync(pfxPath, pfxBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not convert CRT+KEY to PFX");
                    return BadRequest(new { message = $"Could not process certificate: {ex.Message}" });
                }
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            var request = new UpdateSystemSettingsRequest
            {
                HttpsEnabled = true,
                SslCertificatePath = pfxPath,
                SslPrivateKeyPath = keyPath,
                SslCertificateExpiry = expiry,
                SslCertificateSubject = subject
            };

            var settings = await _settingsService.UpdateSettingsAsync(request, userId);
            
            _logger.LogInformation("SSL certificate uploaded by user {UserId}", userId);
            
            return Ok(new { 
                message = "SSL certificate uploaded successfully. Restart the server to apply.",
                expiry = expiry,
                subject = subject,
                requiresRestart = true,
                settings 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading SSL certificate");
            return StatusCode(500, new { message = "Error uploading SSL certificate" });
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
    public ActionResult<DemoStatusResponse> ToggleDemoMode([FromBody] ToggleDemoModeRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;

            // Update the API-layer singleton state (immediate effect)
            _demoModeState.IsDemoMode = request.Enabled;
            
            _logger.LogInformation("Demo mode {Action} by user {UserId}. Current state: {State}", 
                request.Enabled ? "ENABLED" : "DISABLED", userId, _demoModeState.IsDemoMode);
            
            return Ok(new DemoStatusResponse
            {
                UseDemoDatabase = _demoModeState.IsDemoMode,
                DemoDataSeeded = true, // Demo data is always seeded
                DemoDataLastSeeded = DateTime.UtcNow
            });
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
    public ActionResult<DemoStatusResponse> GetDemoStatus()
    {
        try
        {
            // Return the current API-layer state
            return Ok(new DemoStatusResponse
            {
                UseDemoDatabase = _demoModeState.IsDemoMode,
                DemoDataSeeded = true,
                DemoDataLastSeeded = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting demo status");
            return StatusCode(500, "Error getting demo status");
        }
    }

    /// <summary>
    /// Run database sync check (admin only)
    /// </summary>
    [HttpPost("database/sync")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DatabaseSyncResult>> RunDatabaseSync()
    {
        try
        {
            if (_databaseSyncService == null)
            {
                return StatusCode(503, "Database sync service not available");
            }

            var result = await _databaseSyncService.RunSyncCheckAsync();
            _logger.LogInformation("Database sync completed. Success: {Success}, Fields synced: {FieldsSynced}", 
                result.Success, result.FieldsSynced);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running database sync");
            return StatusCode(500, "Error running database sync");
        }
    }

    /// <summary>
    /// Get database sync status (admin only)
    /// </summary>
    [HttpGet("database/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetDatabaseStatus()
    {
        try
        {
            if (_databaseSyncService == null)
            {
                return StatusCode(503, "Database sync service not available");
            }

            var result = await _databaseSyncService.RunSyncCheckAsync();
            
            return Ok(new
            {
                productionDatabase = new
                {
                    name = "crm_db",
                    isActive = !_demoModeState.IsDemoMode,
                    modules = result.ProductionFieldCounts
                },
                demoDatabase = new
                {
                    name = "crm_demodb",
                    isActive = _demoModeState.IsDemoMode,
                    modules = result.DemoFieldCounts
                },
                inSync = result.ProductionFieldCounts.Count == result.DemoFieldCounts.Count &&
                    result.ProductionFieldCounts.All(kvp => result.DemoFieldCounts.TryGetValue(kvp.Key, out var v) && v == kvp.Value),
                lastChecked = result.CheckedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database status");
            return StatusCode(500, "Error getting database status");
        }
    }

    /// <summary>
    /// Get comprehensive feature configuration for admin UI
    /// </summary>
    [HttpGet("features")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetFeatureConfiguration()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            
            return Ok(new
            {
                coreModules = new
                {
                    customers = new { enabled = settings.CustomersEnabled, name = "Customers", description = "Manage customer records and relationships" },
                    contacts = new { enabled = settings.ContactsEnabled, name = "Contacts", description = "Manage contact information" },
                    leads = new { enabled = settings.LeadsEnabled, name = "Leads", description = "Track and manage sales leads" },
                    opportunities = new { enabled = settings.OpportunitiesEnabled, name = "Opportunities", description = "Track sales opportunities and pipeline" },
                    products = new { enabled = settings.ProductsEnabled, name = "Products", description = "Manage product catalog" },
                    services = new { enabled = settings.ServicesEnabled, name = "Services", description = "Manage service catalog" }
                },
                salesModules = new
                {
                    campaigns = new { enabled = settings.CampaignsEnabled, name = "Campaigns", description = "Marketing campaign management" },
                    quotes = new { enabled = settings.QuotesEnabled, name = "Quotes", description = "Create and manage quotes" }
                },
                productivityModules = new
                {
                    tasks = new { enabled = settings.TasksEnabled, name = "Tasks", description = "Task management" },
                    activities = new { enabled = settings.ActivitiesEnabled, name = "Activities", description = "Track activities and interactions" },
                    notes = new { enabled = settings.NotesEnabled, name = "Notes", description = "Add notes to records" }
                },
                automationModules = new
                {
                    workflows = new { enabled = settings.WorkflowsEnabled, name = "Workflows", description = "Automate business processes" }
                },
                analyticsModules = new
                {
                    reports = new { enabled = settings.ReportsEnabled, name = "Reports", description = "Generate reports" },
                    dashboard = new { enabled = settings.DashboardEnabled, name = "Dashboard", description = "Dashboard and analytics" }
                },
                communicationModules = new
                {
                    email = new { enabled = settings.EmailEnabled, name = "Email", description = "Email integration" },
                    whatsapp = new { enabled = settings.WhatsAppEnabled, name = "WhatsApp", description = "WhatsApp integration" },
                    socialMedia = new { enabled = settings.SocialMediaEnabled, name = "Social Media", description = "Social media integration" }
                },
                systemSettings = new
                {
                    demoModeEnabled = _demoModeState.IsDemoMode,
                    useDemoDatabase = settings.UseDemoDatabase
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature configuration");
            return StatusCode(500, "Error getting feature configuration");
        }
    }

    /// <summary>
    /// Bulk update feature toggles (admin only)
    /// </summary>
    [HttpPut("features")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingsDto>> UpdateFeatures([FromBody] UpdateFeaturesRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : null;
            
            var updateRequest = new UpdateSystemSettingsRequest
            {
                CustomersEnabled = request.CustomersEnabled,
                ContactsEnabled = request.ContactsEnabled,
                LeadsEnabled = request.LeadsEnabled,
                OpportunitiesEnabled = request.OpportunitiesEnabled,
                ProductsEnabled = request.ProductsEnabled,
                ServicesEnabled = request.ServicesEnabled,
                CampaignsEnabled = request.CampaignsEnabled,
                QuotesEnabled = request.QuotesEnabled,
                TasksEnabled = request.TasksEnabled,
                ActivitiesEnabled = request.ActivitiesEnabled,
                NotesEnabled = request.NotesEnabled,
                WorkflowsEnabled = request.WorkflowsEnabled,
                ReportsEnabled = request.ReportsEnabled,
                DashboardEnabled = request.DashboardEnabled,
                EmailEnabled = request.EmailEnabled,
                WhatsAppEnabled = request.WhatsAppEnabled,
                SocialMediaEnabled = request.SocialMediaEnabled,
                UseDemoDatabase = request.UseDemoDatabase
            };
            
            var settings = await _settingsService.UpdateSettingsAsync(updateRequest, userId);
            
            // Update demo mode state if changed
            if (request.UseDemoDatabase.HasValue)
            {
                _demoModeState.IsDemoMode = request.UseDemoDatabase.Value;
            }
            
            _logger.LogInformation("Features updated by user {UserId}", userId);
            
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating features");
            return StatusCode(500, "Error updating features");
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
/// Request to generate a self-signed SSL certificate
/// </summary>
public class GenerateCertificateRequest
{
    public string? CommonName { get; set; }
    public int? ValidityDays { get; set; }
    public string? Password { get; set; }
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

/// <summary>
/// Request to update features configuration
/// </summary>
public class UpdateFeaturesRequest
{
    // Core Modules
    public bool? CustomersEnabled { get; set; }
    public bool? ContactsEnabled { get; set; }
    public bool? LeadsEnabled { get; set; }
    public bool? OpportunitiesEnabled { get; set; }
    public bool? ProductsEnabled { get; set; }
    public bool? ServicesEnabled { get; set; }
    
    // Sales Modules
    public bool? CampaignsEnabled { get; set; }
    public bool? QuotesEnabled { get; set; }
    
    // Productivity Modules
    public bool? TasksEnabled { get; set; }
    public bool? ActivitiesEnabled { get; set; }
    public bool? NotesEnabled { get; set; }
    
    // Automation Modules
    public bool? WorkflowsEnabled { get; set; }
    
    // Analytics Modules
    public bool? ReportsEnabled { get; set; }
    public bool? DashboardEnabled { get; set; }
    
    // Communication Modules
    public bool? EmailEnabled { get; set; }
    public bool? WhatsAppEnabled { get; set; }
    public bool? SocialMediaEnabled { get; set; }
    
    // System Settings
    public bool? UseDemoDatabase { get; set; }
}
