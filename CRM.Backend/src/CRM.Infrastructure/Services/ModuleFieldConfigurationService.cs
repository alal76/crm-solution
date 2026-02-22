// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

public class ModuleFieldConfigurationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ModuleFieldConfigurationService> _logger;

    public ModuleFieldConfigurationService(ICrmDbContext context, ILogger<ModuleFieldConfigurationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ModuleFieldConfigurationDto>> GetFieldConfigurationsAsync(string moduleName)
    {
        var configs = await _context.ModuleFieldConfigurations
            .Where(c => c.ModuleName == moduleName)
            .OrderBy(c => c.TabIndex)
            .ThenBy(c => c.DisplayOrder)
            .ToListAsync();

        return configs.Select(MapToDto).ToList();
    }

    public async Task<ModuleFieldConfigurationDto?> GetFieldConfigurationAsync(int id)
    {
        var config = await _context.ModuleFieldConfigurations.FindAsync(id);
        return config != null ? MapToDto(config) : null;
    }

    public async Task<ModuleFieldConfigurationDto> CreateFieldConfigurationAsync(CreateModuleFieldConfigurationDto dto)
    {
        var entity = new ModuleFieldConfiguration
        {
            ModuleName = dto.ModuleName,
            FieldName = dto.FieldName,
            FieldLabel = dto.FieldLabel,
            FieldType = dto.FieldType,
            TabIndex = dto.TabIndex,
            TabName = dto.TabName,
            DisplayOrder = dto.DisplayOrder,
            IsEnabled = dto.IsEnabled,
            IsRequired = dto.IsRequired,
            GridSize = dto.GridSize,
            Placeholder = dto.Placeholder,
            HelpText = dto.HelpText,
            Options = dto.Options,
            ParentField = dto.ParentField,
            ParentFieldValue = dto.ParentFieldValue,
            IsReorderable = dto.IsReorderable,
            IsRequiredConfigurable = dto.IsRequiredConfigurable,
            IsHideable = dto.IsHideable,
            CreatedAt = DateTime.UtcNow
        };

        _context.ModuleFieldConfigurations.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created field configuration {FieldName} for module {ModuleName}", dto.FieldName, dto.ModuleName);
        return MapToDto(entity);
    }

    public async Task<ModuleFieldConfigurationDto?> UpdateFieldConfigurationAsync(int id, UpdateModuleFieldConfigurationDto dto)
    {
        var entity = await _context.ModuleFieldConfigurations.FindAsync(id);
        if (entity == null)
            return null;

        if (dto.FieldLabel != null)
            entity.FieldLabel = dto.FieldLabel;
        if (dto.TabIndex.HasValue)
            entity.TabIndex = dto.TabIndex.Value;
        if (dto.TabName != null)
            entity.TabName = dto.TabName;
        if (dto.DisplayOrder.HasValue)
            entity.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsEnabled.HasValue)
            entity.IsEnabled = dto.IsEnabled.Value;
        if (dto.IsRequired.HasValue)
            entity.IsRequired = dto.IsRequired.Value;
        if (dto.GridSize.HasValue)
            entity.GridSize = dto.GridSize.Value;
        if (dto.Placeholder != null)
            entity.Placeholder = dto.Placeholder;
        if (dto.HelpText != null)
            entity.HelpText = dto.HelpText;
        if (dto.Options != null)
            entity.Options = dto.Options;

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated field configuration {Id}", id);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteFieldConfigurationAsync(int id)
    {
        var entity = await _context.ModuleFieldConfigurations.FindAsync(id);
        if (entity == null)
            return false;

        _context.ModuleFieldConfigurations.Remove(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted field configuration {Id}", id);
        return true;
    }

    public async Task<bool> BulkUpdateFieldOrderAsync(BulkUpdateFieldOrderDto dto)
    {
        var fieldIds = dto.Fields.Select(f => f.Id).ToList();
        var fields = await _context.ModuleFieldConfigurations
            .Where(f => f.ModuleName == dto.ModuleName && f.TabIndex == dto.TabIndex && fieldIds.Contains(f.Id))
            .ToListAsync();

        foreach (var field in fields)
        {
            var orderItem = dto.Fields.FirstOrDefault(f => f.Id == field.Id);
            if (orderItem != null)
            {
                field.DisplayOrder = orderItem.DisplayOrder;
                field.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Bulk updated field order for module {ModuleName} tab {TabIndex}", dto.ModuleName, dto.TabIndex);
        return true;
    }

    public async Task InitializeDefaultConfigurationsAsync(string moduleName)
    {
        var existingCount = await _context.ModuleFieldConfigurations
            .CountAsync(c => c.ModuleName == moduleName);

        if (existingCount > 0)
        {
            _logger.LogInformation("Module {ModuleName} already has field configurations, skipping initialization", moduleName);
            return;
        }

        var configs = GetDefaultFieldsForModule(moduleName);

        if (configs.Any())
        {
            _context.ModuleFieldConfigurations.AddRange(configs);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Initialized {Count} default field configurations for module {ModuleName}", configs.Count, moduleName);
        }
    }

    /// <summary>
    /// Initialize field configurations for all modules that don't have any yet.
    /// This ensures fields are available without requiring users to visit each entity first.
    /// </summary>
    public async Task<Dictionary<string, int>> InitializeAllModulesAsync()
    {
        var results = new Dictionary<string, int>();

        foreach (var moduleName in ModuleNames.All)
        {
            var existingCount = await _context.ModuleFieldConfigurations
                .CountAsync(c => c.ModuleName == moduleName);

            if (existingCount > 0)
            {
                results[moduleName] = existingCount;
                continue;
            }

            var configs = GetDefaultFieldsForModule(moduleName);

            if (configs.Any())
            {
                _context.ModuleFieldConfigurations.AddRange(configs);
                await _context.SaveChangesAsync();
                results[moduleName] = configs.Count;
                _logger.LogInformation("Initialized {Count} default field configurations for module {ModuleName}", configs.Count, moduleName);
            }
            else
            {
                results[moduleName] = 0;
            }
        }

        return results;
    }

    /// <summary>
    /// Force reinitialize all module field configurations.
    /// Deletes ALL existing configs and reseeds with current defaults.
    /// Used during deployments and when field definitions are updated.
    /// </summary>
    public async Task<Dictionary<string, int>> ForceReinitializeAllAsync()
    {
        // Delete all existing field configurations
        var existing = await _context.ModuleFieldConfigurations.ToListAsync();
        if (existing.Any())
        {
            _context.ModuleFieldConfigurations.RemoveRange(existing);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleared {Count} existing module field configurations for force reinitialize", existing.Count);
        }

        // Reseed all modules with current defaults
        var results = new Dictionary<string, int>();
        foreach (var moduleName in ModuleNames.All)
        {
            var configs = GetDefaultFieldsForModule(moduleName);
            if (configs.Any())
            {
                _context.ModuleFieldConfigurations.AddRange(configs);
                await _context.SaveChangesAsync();
                results[moduleName] = configs.Count;
                _logger.LogInformation("Force reinitialized {Count} field configurations for module {ModuleName}", configs.Count, moduleName);
            }
            else
            {
                results[moduleName] = 0;
            }
        }

        return results;
    }

    /// <summary>
    /// Get all default field configurations for the specified module.
    /// This is the single source of truth for default field definitions.
    /// </summary>
    public List<ModuleFieldConfiguration> GetDefaultFieldsForModule(string moduleName)
    {
        return moduleName switch
        {
            ModuleNames.Accounts => GetDefaultCustomerFields(),
            ModuleNames.Contacts => GetDefaultContactFields(),
            ModuleNames.Leads => GetDefaultLeadFields(),
            ModuleNames.Opportunities => GetDefaultOpportunityFields(),
            ModuleNames.Products => GetDefaultProductFields(),
            ModuleNames.Campaigns => GetDefaultCampaignFields(),
            ModuleNames.Quotes => GetDefaultQuoteFields(),
            _ => new List<ModuleFieldConfiguration>()
        };
    }

    private ModuleFieldConfigurationDto MapToDto(ModuleFieldConfiguration entity)
    {
        var help = entity.HelpText;
        // Mark tags/customFields as normalized so UI can prefer normalized rows
        if (!string.IsNullOrWhiteSpace(entity.FieldName))
        {
            var fn = entity.FieldName.Trim().ToLower();
            if (fn == "tags" || fn == "customfields")
            {
                help = string.IsNullOrWhiteSpace(help) ? "(populated from normalized table)" : help + " (populated from normalized table)";
            }
        }

        return new ModuleFieldConfigurationDto
        {
            Id = entity.Id,
            ModuleName = entity.ModuleName,
            FieldName = entity.FieldName,
            FieldLabel = entity.FieldLabel,
            FieldType = entity.FieldType,
            TabIndex = entity.TabIndex,
            TabName = entity.TabName,
            DisplayOrder = entity.DisplayOrder,
            IsEnabled = entity.IsEnabled,
            IsRequired = entity.IsRequired,
            GridSize = entity.GridSize,
            Placeholder = entity.Placeholder,
            HelpText = help,
            Options = entity.Options,
            ParentField = entity.ParentField,
            ParentFieldValue = entity.ParentFieldValue,
            IsReorderable = entity.IsReorderable,
            IsRequiredConfigurable = entity.IsRequiredConfigurable,
            IsHideable = entity.IsHideable
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultCustomerFields()
    {
        var now = DateTime.UtcNow;
        var moduleName = ModuleNames.Accounts;
        var configs = new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Accounts", FieldName = "category", FieldLabel = "Customer Category", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, Options = "Individual,Organization", IsReorderable = false, IsRequiredConfigurable = false, IsHideable = false, CreatedAt = now },

            // All fields shown regardless of category (Individual/Organization)
            new() { ModuleName = "Accounts", FieldName = "salutation", FieldLabel = "Salutation", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 2, Options = "Mr.,Mrs.,Ms.,Dr.,Prof.", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "firstName", FieldLabel = "First Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "lastName", FieldLabel = "Last Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 12, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "suffix", FieldLabel = "Suffix", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 13, IsEnabled = true, IsRequired = false, GridSize = 2, Placeholder = "Jr., III", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "dateOfBirth", FieldLabel = "Date of Birth", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 14, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "gender", FieldLabel = "Gender", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 15, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Male,Female,Other,Prefer not to say", CreatedAt = now },

            // Organization fields (shown for all accounts)
            new() { ModuleName = "Accounts", FieldName = "company", FieldLabel = "Company Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 20, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "legalName", FieldLabel = "Legal Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 21, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Full legal entity name", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "dbaName", FieldLabel = "DBA Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 22, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Doing Business As", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "taxId", FieldLabel = "Tax ID / EIN", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 23, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "registrationNumber", FieldLabel = "Registration Number", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 24, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "yearFounded", FieldLabel = "Year Founded", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 25, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Contact Information (common)
            new() { ModuleName = "Accounts", FieldName = "email", FieldLabel = "Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 30, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "secondaryEmail", FieldLabel = "Secondary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 31, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "phone", FieldLabel = "Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 32, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "mobilePhone", FieldLabel = "Mobile Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 33, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 34, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "website", FieldLabel = "Website", FieldType = "url", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 35, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // Address
            new() { ModuleName = "Accounts", FieldName = "address", FieldLabel = "Address", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 40, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 41, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "state", FieldLabel = "State", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 42, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "zipCode", FieldLabel = "Zip Code", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 43, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },

            // Business Tab (1)
            new() { ModuleName = "Accounts", FieldName = "customerType", FieldLabel = "Customer Type", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Individual,Small Business,Mid-Market,Enterprise,Government,Non-Profit", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "lifecycleStage", FieldLabel = "Lifecycle Stage", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Lead,Prospect,Opportunity,Customer,Churned,Reactivated", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "priority", FieldLabel = "Priority", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Low,Medium,High,Critical", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "industry", FieldLabel = "Industry", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Technology,Healthcare,Finance,Retail,Manufacturing,Education,Real Estate,Consulting,Marketing,Legal,Non-Profit,Government,Other", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "annualRevenue", FieldLabel = "Annual Revenue ($)", FieldType = "currency", TabIndex = 1, TabName = "Business", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "numberOfEmployees", FieldLabel = "Number of Employees", FieldType = "number", TabIndex = 1, TabName = "Business", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "creditLimit", FieldLabel = "Credit Limit ($)", FieldType = "currency", TabIndex = 1, TabName = "Business", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "leadSource", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Website,Referral,Social Media,Cold Call,Trade Show,Advertisement,Email Campaign,Partner,Other", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "leadScore", FieldLabel = "Lead Score", FieldType = "number", TabIndex = 1, TabName = "Business", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 12, HelpText = "Lead score from 0-100", CreatedAt = now },

            // Contact Preferences Tab (2)
            new() { ModuleName = "Accounts", FieldName = "preferredContactMethod", FieldLabel = "Preferred Contact Method", FieldType = "select", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Email,Phone,SMS,Mail", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "timezone", FieldLabel = "Timezone", FieldType = "text", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g., America/New_York", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "optInEmail", FieldLabel = "Email Communications", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "optInPhone", FieldLabel = "Phone Calls", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "optInSms", FieldLabel = "SMS Messages", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "linkedInUrl", FieldLabel = "LinkedIn URL", FieldType = "url", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "twitterHandle", FieldLabel = "Twitter Handle", FieldType = "text", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Additional Tab (3)
            new() { ModuleName = "Accounts", FieldName = "territory", FieldLabel = "Territory", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "paymentTerms", FieldLabel = "Payment Terms", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g., Net 30", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "tags", FieldLabel = "Tags (comma-separated)", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, Placeholder = "vip, enterprise, priority", CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Accounts", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };

        foreach (var config in configs)
        {
            config.ModuleName = moduleName;
        }

        return configs;
    }

    private List<ModuleFieldConfiguration> GetDefaultContactFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Contacts", FieldName = "contactType", FieldLabel = "Contact Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Employee,Customer,Partner,Lead,Vendor,Other", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "salutation", FieldLabel = "Salutation", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 2, Options = "Mr.,Mrs.,Ms.,Dr.,Prof.", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "firstName", FieldLabel = "First Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "middleName", FieldLabel = "Middle Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "lastName", FieldLabel = "Last Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = true, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "suffix", FieldLabel = "Suffix", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 2, Placeholder = "Jr., Sr., III", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "nickname", FieldLabel = "Nickname", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 4, Placeholder = "Preferred name", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "gender", FieldLabel = "Gender", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Male,Female,NonBinary,PreferNotToSay,Other", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "emailPrimary", FieldLabel = "Primary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 8, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "emailSecondary", FieldLabel = "Secondary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phonePrimary", FieldLabel = "Primary Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phoneSecondary", FieldLabel = "Secondary Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phoneMobile", FieldLabel = "Mobile Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 12, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Mobile / cell number", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phoneFax", FieldLabel = "Fax Number", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 13, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "dateOfBirth", FieldLabel = "Date of Birth", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 14, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Work Info Tab (1)
            new() { ModuleName = "Contacts", FieldName = "company", FieldLabel = "Company", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "department", FieldLabel = "Department", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "reportsTo", FieldLabel = "Reports To", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "accountId", FieldLabel = "Owner Account", FieldType = "lookup", TabIndex = 1, TabName = "Work Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // Address & Social Tab (2)
            new() { ModuleName = "Contacts", FieldName = "address", FieldLabel = "Address", FieldType = "text", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Street address", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "address2", FieldLabel = "Address Line 2", FieldType = "text", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Suite, Apt, Floor", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "state", FieldLabel = "State/Province", FieldType = "text", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "zipCode", FieldLabel = "Zip/Postal Code", FieldType = "text", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "country", FieldLabel = "Country", FieldType = "text", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "website", FieldLabel = "Website", FieldType = "url", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 12, Placeholder = "https://", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "linkedInUrl", FieldLabel = "LinkedIn URL", FieldType = "url", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "https://linkedin.com/in/...", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "twitterHandle", FieldLabel = "Twitter Handle", FieldType = "text", TabIndex = 2, TabName = "Address & Social", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "@username", CreatedAt = now },

            // Additional Tab (3)
            new() { ModuleName = "Contacts", FieldName = "leadStatus", FieldLabel = "Lead Status", FieldType = "select", TabIndex = 3, TabName = "Additional", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "new,contacted,qualified,unqualified,converted", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "preferredContactMethod", FieldLabel = "Preferred Contact Method", FieldType = "select", TabIndex = 3, TabName = "Additional", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "email,phone,sms,linkedin", CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "doNotContact", FieldLabel = "Do Not Contact", FieldType = "checkbox", TabIndex = 3, TabName = "Additional", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultLeadFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Leads", FieldName = "firstName", FieldLabel = "First Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "lastName", FieldLabel = "Last Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "emailPrimary", FieldLabel = "Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "phonePrimary", FieldLabel = "Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "company", FieldLabel = "Company", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "source", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "website,referral,event,cold_call,social,other", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 7, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "new,contacted,qualified,converted,lost", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // Additional Info Tab (1)
            new() { ModuleName = "Leads", FieldName = "website", FieldLabel = "Website", FieldType = "url", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "https://", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "phoneSecondary", FieldLabel = "Secondary Phone", FieldType = "phone", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "linkedInUrl", FieldLabel = "LinkedIn URL", FieldType = "url", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "https://linkedin.com/in/...", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "twitterHandle", FieldLabel = "Twitter Handle", FieldType = "text", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "@username", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "preferredContactMethod", FieldLabel = "Preferred Contact Method", FieldType = "select", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Email,Phone,SMS,Mail", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "doNotContact", FieldLabel = "Do Not Contact", FieldType = "checkbox", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "region", FieldLabel = "Region", FieldType = "text", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "campaignId", FieldLabel = "Campaign ID", FieldType = "number", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "mqlDate", FieldLabel = "MQL Date", FieldType = "date", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "sqlDate", FieldLabel = "SQL Date", FieldType = "date", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "qualificationNotes", FieldLabel = "Qualification Notes", FieldType = "textarea", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "tags", FieldLabel = "Tags", FieldType = "text", TabIndex = 1, TabName = "Additional Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 12, Placeholder = "comma-separated", CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultOpportunityFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Opportunity Info Tab (0)
            new() { ModuleName = "Opportunities", FieldName = "name", FieldLabel = "Opportunity Name", FieldType = "text", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "accountId", FieldLabel = "Account", FieldType = "lookup", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "primaryContactId", FieldLabel = "Primary Contact", FieldType = "lookup", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "salesOwnerId", FieldLabel = "Sales Owner", FieldType = "lookup", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "stage", FieldLabel = "Stage", FieldType = "select", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 4, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Prospecting,Qualification,Proposal,Negotiation,Closed Won,Closed Lost", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "probability", FieldLabel = "Probability (%)", FieldType = "number", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Win probability 0-100%", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "amount", FieldLabel = "Amount", FieldType = "currency", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "currency", FieldLabel = "Currency", FieldType = "select", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "USD,EUR,GBP,CAD,AUD,JPY,CNY,INR", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "expectedCloseDate", FieldLabel = "Expected Close Date", FieldType = "date", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "pricingModel", FieldLabel = "Pricing Model", FieldType = "select", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Subscription,OneTime,UsageBased,Tiered", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "termLengthMonths", FieldLabel = "Term Length (months)", FieldType = "number", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "1-120 months", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "region", FieldLabel = "Region", FieldType = "text", TabIndex = 0, TabName = "Opportunity Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Qualification Tab (1)
            new() { ModuleName = "Opportunities", FieldName = "qualificationReason", FieldLabel = "Qualification Reason", FieldType = "select", TabIndex = 1, TabName = "Qualification", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Budget,Authority,Need,Timeline,Competition", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "qualificationNotes", FieldLabel = "Qualification Notes", FieldType = "textarea", TabIndex = 1, TabName = "Qualification", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "solutionNotes", FieldLabel = "Solution Notes", FieldType = "textarea", TabIndex = 1, TabName = "Qualification", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 1, TabName = "Qualification", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultProductFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Products", FieldName = "name", FieldLabel = "Product Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "sku", FieldLabel = "SKU", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "barcode", FieldLabel = "Barcode", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "productType", FieldLabel = "Product Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, Options = "Physical,Digital,Service,Subscription,Bundle", CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, Options = "Draft,Active,Discontinued,Archived", CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "category", FieldLabel = "Category", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 4, Options = "Software,Hardware,Service,Subscription,Other", CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "subcategory", FieldLabel = "Subcategory", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "isActive", FieldLabel = "Active", FieldType = "checkbox", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "isFeatured", FieldLabel = "Featured", FieldType = "checkbox", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 3, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "shortDescription", FieldLabel = "Short Description", FieldType = "textarea", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "features", FieldLabel = "Features", FieldType = "textarea", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "tags", FieldLabel = "Tags", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 12, IsEnabled = true, IsRequired = false, GridSize = 12, Placeholder = "comma-separated", CreatedAt = now },

            // Pricing Tab (1)
            new() { ModuleName = "Products", FieldName = "price", FieldLabel = "Price ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "listPrice", FieldLabel = "List Price ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "minimumPrice", FieldLabel = "Minimum Price ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "costPrice", FieldLabel = "Cost Price ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "isTaxable", FieldLabel = "Taxable", FieldType = "checkbox", TabIndex = 1, TabName = "Pricing", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "taxRate", FieldLabel = "Tax Rate (%)", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "e.g. 8.25", CreatedAt = now },

            // Subscription Tab (2)
            new() { ModuleName = "Products", FieldName = "isSubscription", FieldLabel = "Is Subscription", FieldType = "checkbox", TabIndex = 2, TabName = "Subscription", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "billingFrequency", FieldLabel = "Billing Frequency", FieldType = "select", TabIndex = 2, TabName = "Subscription", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Monthly,Quarterly,SemiAnnual,Annual", CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "recurringPrice", FieldLabel = "Recurring Price ($)", FieldType = "currency", TabIndex = 2, TabName = "Subscription", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "setupFee", FieldLabel = "Setup Fee ($)", FieldType = "currency", TabIndex = 2, TabName = "Subscription", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "trialPeriodDays", FieldLabel = "Trial Period (days)", FieldType = "number", TabIndex = 2, TabName = "Subscription", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "contractLengthMonths", FieldLabel = "Contract Length (months)", FieldType = "number", TabIndex = 2, TabName = "Subscription", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Inventory Tab (3)
            new() { ModuleName = "Products", FieldName = "trackInventory", FieldLabel = "Track Inventory", FieldType = "checkbox", TabIndex = 3, TabName = "Inventory", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "stock", FieldLabel = "Current Stock", FieldType = "number", TabIndex = 3, TabName = "Inventory", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "reorderLevel", FieldLabel = "Reorder Level", FieldType = "number", TabIndex = 3, TabName = "Inventory", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "reorderQuantity", FieldLabel = "Reorder Quantity", FieldType = "number", TabIndex = 3, TabName = "Inventory", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "warehouseLocation", FieldLabel = "Warehouse Location", FieldType = "text", TabIndex = 3, TabName = "Inventory", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "weight", FieldLabel = "Weight", FieldType = "text", TabIndex = 3, TabName = "Inventory", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g. 1.5 kg", CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "dimensions", FieldLabel = "Dimensions", FieldType = "text", TabIndex = 3, TabName = "Inventory", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "L x W x H", CreatedAt = now },

            // SEO & Media Tab (4)
            new() { ModuleName = "Products", FieldName = "thumbnailUrl", FieldLabel = "Thumbnail URL", FieldType = "url", TabIndex = 4, TabName = "SEO & Media", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "videoUrl", FieldLabel = "Video URL", FieldType = "url", TabIndex = 4, TabName = "SEO & Media", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "slug", FieldLabel = "URL Slug", FieldType = "text", TabIndex = 4, TabName = "SEO & Media", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "metaTitle", FieldLabel = "Meta Title", FieldType = "text", TabIndex = 4, TabName = "SEO & Media", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "metaDescription", FieldLabel = "Meta Description", FieldType = "textarea", TabIndex = 4, TabName = "SEO & Media", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultCampaignFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Campaigns", FieldName = "name", FieldLabel = "Campaign Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "type", FieldLabel = "Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Email,Social Media,Event,Webinar,Advertising,Referral,Other", CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Planning,Active,Paused,Completed,Cancelled", CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "startDate", FieldLabel = "Start Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "endDate", FieldLabel = "End Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Budget Tab (1)
            new() { ModuleName = "Campaigns", FieldName = "budget", FieldLabel = "Budget", FieldType = "currency", TabIndex = 1, TabName = "Budget", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "actualCost", FieldLabel = "Actual Cost", FieldType = "currency", TabIndex = 1, TabName = "Budget", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "expectedRevenue", FieldLabel = "Expected Revenue", FieldType = "currency", TabIndex = 1, TabName = "Budget", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "expectedResponse", FieldLabel = "Expected Response", FieldType = "number", TabIndex = 1, TabName = "Budget", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Details Tab (2)
            new() { ModuleName = "Campaigns", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Campaigns", FieldName = "objectives", FieldLabel = "Objectives", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultQuoteFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Quotes", FieldName = "quoteNumber", FieldLabel = "Quote Number", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "accountId", FieldLabel = "Account", FieldType = "lookup", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Draft,Sent,Accepted,Rejected,Expired", CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "validUntil", FieldLabel = "Valid Until", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // Pricing Tab (1)
            new() { ModuleName = "Quotes", FieldName = "subtotal", FieldLabel = "Subtotal", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "discount", FieldLabel = "Discount", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "tax", FieldLabel = "Tax", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "total", FieldLabel = "Total", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // Terms Tab (2)
            new() { ModuleName = "Quotes", FieldName = "terms", FieldLabel = "Terms & Conditions", FieldType = "textarea", TabIndex = 2, TabName = "Terms", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Quotes", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 2, TabName = "Terms", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }
}
