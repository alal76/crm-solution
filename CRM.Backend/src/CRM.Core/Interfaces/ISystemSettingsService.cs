using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing system settings
/// </summary>
public interface ISystemSettingsService
{
    /// <summary>
    /// Gets the current system settings
    /// </summary>
    Task<SystemSettingsDto> GetSettingsAsync();
    
    /// <summary>
    /// Gets the module status for frontend permission checking
    /// </summary>
    Task<ModuleStatusDto> GetModuleStatusAsync();
    
    /// <summary>
    /// Updates system settings (partial update supported)
    /// </summary>
    Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsRequest request, int? modifiedByUserId = null);
}
