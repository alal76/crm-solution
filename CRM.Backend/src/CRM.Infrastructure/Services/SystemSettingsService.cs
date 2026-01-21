using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing system settings
/// </summary>
public class SystemSettingsService : ISystemSettingsService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<SystemSettingsService> _logger;

    public SystemSettingsService(ICrmDbContext context, ILogger<SystemSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current system settings (creates default if none exist)
    /// </summary>
    public async Task<SystemSettingsDto> GetSettingsAsync()
    {
        try
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // Create default settings
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            
            return MapToDto(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system settings");
            throw;
        }
    }

    /// <summary>
    /// Gets the module status for frontend permission checking
    /// </summary>
    public async Task<ModuleStatusDto> GetModuleStatusAsync()
    {
        try
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            return new ModuleStatusDto
            {
                CustomersEnabled = settings?.CustomersEnabled ?? true,
                ContactsEnabled = settings?.ContactsEnabled ?? true,
                LeadsEnabled = settings?.LeadsEnabled ?? true,
                OpportunitiesEnabled = settings?.OpportunitiesEnabled ?? true,
                ProductsEnabled = settings?.ProductsEnabled ?? true,
                ServicesEnabled = settings?.ServicesEnabled ?? true,
                CampaignsEnabled = settings?.CampaignsEnabled ?? true,
                QuotesEnabled = settings?.QuotesEnabled ?? true,
                TasksEnabled = settings?.TasksEnabled ?? true,
                ActivitiesEnabled = settings?.ActivitiesEnabled ?? true,
                NotesEnabled = settings?.NotesEnabled ?? true,
                WorkflowsEnabled = settings?.WorkflowsEnabled ?? true,
                ReportsEnabled = settings?.ReportsEnabled ?? true,
                DashboardEnabled = settings?.DashboardEnabled ?? true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module status");
            throw;
        }
    }

    /// <summary>
    /// Updates system settings (partial update supported)
    /// </summary>
    public async Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsRequest request, int? modifiedByUserId = null)
    {
        try
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
            }
            
            // Apply updates (only update non-null values)
            if (request.CustomersEnabled.HasValue) settings.CustomersEnabled = request.CustomersEnabled.Value;
            if (request.ContactsEnabled.HasValue) settings.ContactsEnabled = request.ContactsEnabled.Value;
            if (request.LeadsEnabled.HasValue) settings.LeadsEnabled = request.LeadsEnabled.Value;
            if (request.OpportunitiesEnabled.HasValue) settings.OpportunitiesEnabled = request.OpportunitiesEnabled.Value;
            if (request.ProductsEnabled.HasValue) settings.ProductsEnabled = request.ProductsEnabled.Value;
            if (request.ServicesEnabled.HasValue) settings.ServicesEnabled = request.ServicesEnabled.Value;
            if (request.CampaignsEnabled.HasValue) settings.CampaignsEnabled = request.CampaignsEnabled.Value;
            if (request.QuotesEnabled.HasValue) settings.QuotesEnabled = request.QuotesEnabled.Value;
            if (request.TasksEnabled.HasValue) settings.TasksEnabled = request.TasksEnabled.Value;
            if (request.ActivitiesEnabled.HasValue) settings.ActivitiesEnabled = request.ActivitiesEnabled.Value;
            if (request.NotesEnabled.HasValue) settings.NotesEnabled = request.NotesEnabled.Value;
            if (request.WorkflowsEnabled.HasValue) settings.WorkflowsEnabled = request.WorkflowsEnabled.Value;
            if (request.ReportsEnabled.HasValue) settings.ReportsEnabled = request.ReportsEnabled.Value;
            if (request.DashboardEnabled.HasValue) settings.DashboardEnabled = request.DashboardEnabled.Value;
            
            if (!string.IsNullOrEmpty(request.CompanyName)) settings.CompanyName = request.CompanyName;
            if (request.CompanyLogoUrl != null) settings.CompanyLogoUrl = request.CompanyLogoUrl;
            if (!string.IsNullOrEmpty(request.PrimaryColor)) settings.PrimaryColor = request.PrimaryColor;
            if (!string.IsNullOrEmpty(request.SecondaryColor)) settings.SecondaryColor = request.SecondaryColor;
            
            if (request.RequireTwoFactor.HasValue) settings.RequireTwoFactor = request.RequireTwoFactor.Value;
            if (request.MinPasswordLength.HasValue) settings.MinPasswordLength = request.MinPasswordLength.Value;
            if (request.SessionTimeoutMinutes.HasValue) settings.SessionTimeoutMinutes = request.SessionTimeoutMinutes.Value;
            if (request.AllowUserRegistration.HasValue) settings.AllowUserRegistration = request.AllowUserRegistration.Value;
            if (request.RequireApprovalForNewUsers.HasValue) settings.RequireApprovalForNewUsers = request.RequireApprovalForNewUsers.Value;
            
            if (request.ApiAccessEnabled.HasValue) settings.ApiAccessEnabled = request.ApiAccessEnabled.Value;
            if (request.EmailNotificationsEnabled.HasValue) settings.EmailNotificationsEnabled = request.EmailNotificationsEnabled.Value;
            if (request.AuditLoggingEnabled.HasValue) settings.AuditLoggingEnabled = request.AuditLoggingEnabled.Value;
            
            if (!string.IsNullOrEmpty(request.DateFormat)) settings.DateFormat = request.DateFormat;
            if (!string.IsNullOrEmpty(request.TimeFormat)) settings.TimeFormat = request.TimeFormat;
            if (!string.IsNullOrEmpty(request.DefaultCurrency)) settings.DefaultCurrency = request.DefaultCurrency;
            if (!string.IsNullOrEmpty(request.DefaultTimezone)) settings.DefaultTimezone = request.DefaultTimezone;
            if (!string.IsNullOrEmpty(request.DefaultLanguage)) settings.DefaultLanguage = request.DefaultLanguage;
            
            settings.LastModified = DateTime.UtcNow;
            settings.ModifiedByUserId = modifiedByUserId;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("System settings updated by user {UserId}", modifiedByUserId);
            
            return MapToDto(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system settings");
            throw;
        }
    }

    private static SystemSettingsDto MapToDto(SystemSettings settings)
    {
        return new SystemSettingsDto
        {
            Id = settings.Id,
            
            CustomersEnabled = settings.CustomersEnabled,
            ContactsEnabled = settings.ContactsEnabled,
            LeadsEnabled = settings.LeadsEnabled,
            OpportunitiesEnabled = settings.OpportunitiesEnabled,
            ProductsEnabled = settings.ProductsEnabled,
            ServicesEnabled = settings.ServicesEnabled,
            CampaignsEnabled = settings.CampaignsEnabled,
            QuotesEnabled = settings.QuotesEnabled,
            TasksEnabled = settings.TasksEnabled,
            ActivitiesEnabled = settings.ActivitiesEnabled,
            NotesEnabled = settings.NotesEnabled,
            WorkflowsEnabled = settings.WorkflowsEnabled,
            ReportsEnabled = settings.ReportsEnabled,
            DashboardEnabled = settings.DashboardEnabled,
            
            CompanyName = settings.CompanyName,
            CompanyLogoUrl = settings.CompanyLogoUrl,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            
            RequireTwoFactor = settings.RequireTwoFactor,
            MinPasswordLength = settings.MinPasswordLength,
            SessionTimeoutMinutes = settings.SessionTimeoutMinutes,
            AllowUserRegistration = settings.AllowUserRegistration,
            RequireApprovalForNewUsers = settings.RequireApprovalForNewUsers,
            
            ApiAccessEnabled = settings.ApiAccessEnabled,
            EmailNotificationsEnabled = settings.EmailNotificationsEnabled,
            AuditLoggingEnabled = settings.AuditLoggingEnabled,
            
            DateFormat = settings.DateFormat,
            TimeFormat = settings.TimeFormat,
            DefaultCurrency = settings.DefaultCurrency,
            DefaultTimezone = settings.DefaultTimezone,
            DefaultLanguage = settings.DefaultLanguage,
            
            LastModified = settings.LastModified,
            ModifiedByUserId = settings.ModifiedByUserId
        };
    }
}
