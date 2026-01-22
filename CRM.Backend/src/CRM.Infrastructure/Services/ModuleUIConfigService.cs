using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

public class ModuleUIConfigService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ModuleUIConfigService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ModuleUIConfigService(ICrmDbContext context, ILogger<ModuleUIConfigService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all module UI configurations
    /// </summary>
    public async Task<List<ModuleUIConfigDto>> GetAllModuleConfigsAsync()
    {
        var configs = await _context.ModuleUIConfigs
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return configs.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get module UI configuration by module name
    /// </summary>
    public async Task<ModuleUIConfigDto?> GetModuleConfigAsync(string moduleName)
    {
        var config = await _context.ModuleUIConfigs
            .FirstOrDefaultAsync(c => c.ModuleName == moduleName);
        
        return config != null ? MapToDto(config) : null;
    }

    /// <summary>
    /// Get complete module configuration including field configurations
    /// </summary>
    public async Task<CompleteModuleConfigDto?> GetCompleteModuleConfigAsync(string moduleName)
    {
        var moduleConfig = await _context.ModuleUIConfigs
            .FirstOrDefaultAsync(c => c.ModuleName == moduleName);

        if (moduleConfig == null)
            return null;

        var fieldConfigs = await _context.ModuleFieldConfigurations
            .Where(f => f.ModuleName == moduleName)
            .OrderBy(f => f.TabIndex)
            .ThenBy(f => f.DisplayOrder)
            .ToListAsync();

        var result = new CompleteModuleConfigDto
        {
            ModuleConfig = MapToDto(moduleConfig),
            FieldConfigurations = fieldConfigs.Select(f => new ModuleFieldConfigurationDto
            {
                Id = f.Id,
                ModuleName = f.ModuleName,
                FieldName = f.FieldName,
                FieldLabel = f.FieldLabel,
                FieldType = f.FieldType,
                TabIndex = f.TabIndex,
                TabName = f.TabName,
                DisplayOrder = f.DisplayOrder,
                IsEnabled = f.IsEnabled,
                IsRequired = f.IsRequired,
                GridSize = f.GridSize,
                Placeholder = f.Placeholder,
                HelpText = f.HelpText,
                Options = f.Options,
                ParentField = f.ParentField,
                ParentFieldValue = f.ParentFieldValue,
                IsReorderable = f.IsReorderable,
                IsRequiredConfigurable = f.IsRequiredConfigurable,
                IsHideable = f.IsHideable
            }).ToList()
        };

        // Parse tabs config
        if (!string.IsNullOrEmpty(moduleConfig.TabsConfig))
        {
            try
            {
                result.Tabs = JsonSerializer.Deserialize<List<TabConfigItem>>(moduleConfig.TabsConfig, _jsonOptions) ?? new();
            }
            catch { result.Tabs = new(); }
        }

        // Parse linked entities config
        if (!string.IsNullOrEmpty(moduleConfig.LinkedEntitiesConfig))
        {
            try
            {
                result.LinkedEntities = JsonSerializer.Deserialize<List<LinkedEntityConfigItem>>(moduleConfig.LinkedEntitiesConfig, _jsonOptions) ?? new();
            }
            catch { result.LinkedEntities = new(); }
        }

        return result;
    }

    /// <summary>
    /// Create a new module UI configuration
    /// </summary>
    public async Task<ModuleUIConfigDto> CreateModuleConfigAsync(CreateModuleUIConfigDto dto)
    {
        var entity = new ModuleUIConfig
        {
            ModuleName = dto.ModuleName,
            IsEnabled = dto.IsEnabled,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            IconName = dto.IconName,
            DisplayOrder = dto.DisplayOrder,
            TabsConfig = dto.TabsConfig,
            LinkedEntitiesConfig = dto.LinkedEntitiesConfig,
            ListViewConfig = dto.ListViewConfig,
            DetailViewConfig = dto.DetailViewConfig,
            QuickCreateConfig = dto.QuickCreateConfig,
            SearchFilterConfig = dto.SearchFilterConfig,
            ModuleSettings = dto.ModuleSettings,
            CreatedAt = DateTime.UtcNow
        };

        _context.ModuleUIConfigs.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created module UI config for {ModuleName}", dto.ModuleName);
        return MapToDto(entity);
    }

    /// <summary>
    /// Update an existing module UI configuration
    /// </summary>
    public async Task<ModuleUIConfigDto?> UpdateModuleConfigAsync(string moduleName, UpdateModuleUIConfigDto dto)
    {
        var entity = await _context.ModuleUIConfigs
            .FirstOrDefaultAsync(c => c.ModuleName == moduleName);

        if (entity == null)
            return null;

        if (dto.IsEnabled.HasValue) entity.IsEnabled = dto.IsEnabled.Value;
        if (dto.DisplayName != null) entity.DisplayName = dto.DisplayName;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.IconName != null) entity.IconName = dto.IconName;
        if (dto.DisplayOrder.HasValue) entity.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.TabsConfig != null) entity.TabsConfig = dto.TabsConfig;
        if (dto.LinkedEntitiesConfig != null) entity.LinkedEntitiesConfig = dto.LinkedEntitiesConfig;
        if (dto.ListViewConfig != null) entity.ListViewConfig = dto.ListViewConfig;
        if (dto.DetailViewConfig != null) entity.DetailViewConfig = dto.DetailViewConfig;
        if (dto.QuickCreateConfig != null) entity.QuickCreateConfig = dto.QuickCreateConfig;
        if (dto.SearchFilterConfig != null) entity.SearchFilterConfig = dto.SearchFilterConfig;
        if (dto.ModuleSettings != null) entity.ModuleSettings = dto.ModuleSettings;

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated module UI config for {ModuleName}", moduleName);
        return MapToDto(entity);
    }

    /// <summary>
    /// Batch update module configurations (for enable/disable and reordering)
    /// </summary>
    public async Task<List<ModuleUIConfigDto>> BatchUpdateModulesAsync(BatchModuleUIConfigUpdateDto dto)
    {
        var moduleNames = dto.Modules.Select(m => m.ModuleName).ToList();
        var configs = await _context.ModuleUIConfigs
            .Where(c => moduleNames.Contains(c.ModuleName))
            .ToListAsync();

        foreach (var config in configs)
        {
            var update = dto.Modules.FirstOrDefault(m => m.ModuleName == config.ModuleName);
            if (update != null)
            {
                if (update.IsEnabled.HasValue) config.IsEnabled = update.IsEnabled.Value;
                if (update.DisplayOrder.HasValue) config.DisplayOrder = update.DisplayOrder.Value;
                config.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Batch updated {Count} module configurations", configs.Count);

        return configs.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Update linked entities configuration for a module
    /// </summary>
    public async Task<ModuleUIConfigDto?> UpdateLinkedEntitiesAsync(string moduleName, List<LinkedEntityConfigItem> linkedEntities)
    {
        var entity = await _context.ModuleUIConfigs
            .FirstOrDefaultAsync(c => c.ModuleName == moduleName);

        if (entity == null)
            return null;

        entity.LinkedEntitiesConfig = JsonSerializer.Serialize(linkedEntities, _jsonOptions);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated linked entities for module {ModuleName}", moduleName);
        return MapToDto(entity);
    }

    /// <summary>
    /// Update tabs configuration for a module
    /// </summary>
    public async Task<ModuleUIConfigDto?> UpdateTabsConfigAsync(string moduleName, List<TabConfigItem> tabs)
    {
        var entity = await _context.ModuleUIConfigs
            .FirstOrDefaultAsync(c => c.ModuleName == moduleName);

        if (entity == null)
            return null;

        entity.TabsConfig = JsonSerializer.Serialize(tabs, _jsonOptions);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated tabs config for module {ModuleName}", moduleName);
        return MapToDto(entity);
    }

    /// <summary>
    /// Initialize default configurations for all modules
    /// </summary>
    public async Task InitializeDefaultConfigsAsync()
    {
        var existingModules = await _context.ModuleUIConfigs
            .Select(c => c.ModuleName)
            .ToListAsync();

        var modulesToCreate = new List<ModuleUIConfig>();
        var order = 0;

        var defaultModules = new[]
        {
            ("Dashboard", "Analytics and overview", "Dashboard", true),
            ("Customers", "Customer management", "People", true),
            ("Contacts", "Contact information", "Contacts", true),
            ("Leads", "Lead management", "PersonAdd", true),
            ("Opportunities", "Sales opportunities", "TrendingUp", true),
            ("Products", "Product catalog", "Inventory2", true),
            ("Services", "Service offerings", "Build", true),
            ("Campaigns", "Marketing campaigns", "Campaign", true),
            ("Quotes", "Quotations", "Description", true),
            ("Tasks", "Task management", "Assignment", true),
            ("Activities", "Activity logging", "Timeline", true),
            ("Notes", "Notes and docs", "Note", true),
            ("Workflows", "Automation rules", "AutoAwesome", true),
            ("Reports", "Reports and analytics", "Assessment", true)
        };

        foreach (var (moduleName, description, icon, enabled) in defaultModules)
        {
            if (!existingModules.Contains(moduleName))
            {
                var linkedEntities = DefaultModuleConfigs.DefaultLinkedEntities
                    .GetValueOrDefault(moduleName, Array.Empty<string>())
                    .Select((e, i) => new LinkedEntityConfigItem
                    {
                        EntityName = e,
                        RelationshipType = "one-to-many",
                        Enabled = true,
                        TabName = e,
                        DisplayOrder = i
                    })
                    .ToList();

                modulesToCreate.Add(new ModuleUIConfig
                {
                    ModuleName = moduleName,
                    DisplayName = moduleName,
                    Description = description,
                    IconName = icon,
                    IsEnabled = enabled,
                    DisplayOrder = order++,
                    LinkedEntitiesConfig = JsonSerializer.Serialize(linkedEntities, _jsonOptions),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (modulesToCreate.Any())
        {
            _context.ModuleUIConfigs.AddRange(modulesToCreate);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Initialized {Count} default module configurations", modulesToCreate.Count);
        }
    }

    /// <summary>
    /// Toggle module enabled status
    /// </summary>
    public async Task<ModuleUIConfigDto?> ToggleModuleAsync(string moduleName, bool enabled)
    {
        var entity = await _context.ModuleUIConfigs
            .FirstOrDefaultAsync(c => c.ModuleName == moduleName);

        if (entity == null)
            return null;

        entity.IsEnabled = enabled;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Toggled module {ModuleName} to {Enabled}", moduleName, enabled);
        return MapToDto(entity);
    }

    private ModuleUIConfigDto MapToDto(ModuleUIConfig entity)
    {
        return new ModuleUIConfigDto
        {
            Id = entity.Id,
            ModuleName = entity.ModuleName,
            IsEnabled = entity.IsEnabled,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            IconName = entity.IconName,
            DisplayOrder = entity.DisplayOrder,
            TabsConfig = entity.TabsConfig,
            LinkedEntitiesConfig = entity.LinkedEntitiesConfig,
            ListViewConfig = entity.ListViewConfig,
            DetailViewConfig = entity.DetailViewConfig,
            QuickCreateConfig = entity.QuickCreateConfig,
            SearchFilterConfig = entity.SearchFilterConfig,
            ModuleSettings = entity.ModuleSettings
        };
    }
}
