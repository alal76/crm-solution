using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for managing field-to-master-data links
/// </summary>
public interface IFieldMasterDataService
{
    /// <summary>
    /// Get all master data links for a field
    /// </summary>
    Task<List<FieldMasterDataLinkDto>> GetLinksForFieldAsync(int fieldConfigurationId);
    
    /// <summary>
    /// Get all master data links for a module
    /// </summary>
    Task<Dictionary<int, List<FieldMasterDataLinkDto>>> GetLinksForModuleAsync(string moduleName);
    
    /// <summary>
    /// Get a single link by ID
    /// </summary>
    Task<FieldMasterDataLinkDto?> GetLinkByIdAsync(int id);
    
    /// <summary>
    /// Create a new master data link
    /// </summary>
    Task<FieldMasterDataLinkDto> CreateLinkAsync(CreateFieldMasterDataLinkDto dto);
    
    /// <summary>
    /// Update an existing link
    /// </summary>
    Task<FieldMasterDataLinkDto> UpdateLinkAsync(int id, CreateFieldMasterDataLinkDto dto);
    
    /// <summary>
    /// Delete a link
    /// </summary>
    Task<bool> DeleteLinkAsync(int id);
    
    /// <summary>
    /// Get all available master data sources
    /// </summary>
    Task<List<MasterDataSourceDto>> GetAvailableSourcesAsync();
    
    /// <summary>
    /// Get master data values for a field (for dropdown population)
    /// </summary>
    Task<List<MasterDataLookupResultDto>> GetMasterDataForFieldAsync(
        int fieldConfigurationId, 
        Dictionary<string, string>? dependentValues = null,
        string? searchTerm = null,
        int limit = 100);
    
    /// <summary>
    /// Validate a value against field's master data configuration
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateValueAsync(
        int fieldConfigurationId, 
        string value,
        Dictionary<string, string>? dependentValues = null);
}
