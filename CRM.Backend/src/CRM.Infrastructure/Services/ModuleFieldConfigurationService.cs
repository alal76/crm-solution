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
        {
            return null;
        }

        if (dto.FieldLabel != null)
        {
            entity.FieldLabel = dto.FieldLabel;
        }
        if (dto.TabIndex.HasValue)
        {
            entity.TabIndex = dto.TabIndex.Value;
        }
        if (dto.TabName != null)
        {
            entity.TabName = dto.TabName;
        }
        if (dto.DisplayOrder.HasValue)
        {
            entity.DisplayOrder = dto.DisplayOrder.Value;
        }
        if (dto.IsEnabled.HasValue)
        {
            entity.IsEnabled = dto.IsEnabled.Value;
        }
        if (dto.IsRequired.HasValue)
        {
            entity.IsRequired = dto.IsRequired.Value;
        }
        if (dto.GridSize.HasValue)
        {
            entity.GridSize = dto.GridSize.Value;
        }
        if (dto.Placeholder != null)
        {
            entity.Placeholder = dto.Placeholder;
        }
        if (dto.HelpText != null)
        {
            entity.HelpText = dto.HelpText;
        }
        if (dto.Options != null)
        {
            entity.Options = dto.Options;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated field configuration {Id}", id);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteFieldConfigurationAsync(int id)
    {
        var entity = await _context.ModuleFieldConfigurations.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

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
            ModuleNames.Orders => GetDefaultOrderFields(),
            ModuleNames.Invoices => GetDefaultInvoiceFields(),
            ModuleNames.Contracts => GetDefaultContractFields(),
            ModuleNames.ServiceRequests => GetDefaultServiceRequestFields(),
            ModuleNames.Payments => GetDefaultPaymentFields(),
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
        var m = "Campaigns";
        return new List<ModuleFieldConfiguration>
        {
            // ── Tab 0: Basic Info ──────────────────────────────────────
            new() { ModuleName = m, FieldName = "name", FieldLabel = "Campaign Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "campaignType", FieldLabel = "Campaign Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:Email,1:Social Media,2:Event,3:Webinar,4:Content Marketing,5:Advertising,6:Referral,7:Direct Mail,8:Telemarketing,9:Partner,10:Trade Show,11:Other", CreatedAt = now },
            new() { ModuleName = m, FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "0:Planning,1:Active,2:Paused,3:Completed,4:Cancelled,5:Draft,6:Scheduled,7:Archived", CreatedAt = now },
            new() { ModuleName = m, FieldName = "priority", FieldLabel = "Priority", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:Low,1:Medium,2:High,3:Critical", CreatedAt = now },
            new() { ModuleName = m, FieldName = "startDate", FieldLabel = "Start Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "endDate", FieldLabel = "End Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            // Accordion: Additional Information
            new() { ModuleName = m, FieldName = "budget", FieldLabel = "Budget ($)", FieldType = "currency", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "actualSpend", FieldLabel = "Actual Spend ($)", FieldType = "currency", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "targetAudience", FieldLabel = "Target Audience", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 12, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "tags", FieldLabel = "Tags (comma-separated)", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 13, IsEnabled = true, IsRequired = false, GridSize = 12, Placeholder = "seasonal, promotion, q1", CreatedAt = now },
            new() { ModuleName = m, FieldName = "isABTest", FieldLabel = "A/B Test Campaign", FieldType = "checkbox", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 14, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "abTestVariants", FieldLabel = "A/B Test Variants", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 15, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "isABTest", ParentFieldValue = "true", CreatedAt = now },
            new() { ModuleName = m, FieldName = "winningVariant", FieldLabel = "Winning Variant", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 16, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "isABTest", ParentFieldValue = "true", CreatedAt = now },
            // Accordion: Budget & Performance
            new() { ModuleName = m, FieldName = "dailyBudget", FieldLabel = "Daily Budget ($)", FieldType = "currency", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 20, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "monthlyBudget", FieldLabel = "Monthly Budget ($)", FieldType = "currency", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 21, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "expectedRevenue", FieldLabel = "Expected Revenue ($)", FieldType = "currency", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 22, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "costPerLead", FieldLabel = "Cost Per Lead ($)", FieldType = "currency", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 23, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "costPerAcquisition", FieldLabel = "Cost Per Acquisition ($)", FieldType = "currency", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 24, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            // Accordion: Scheduling
            new() { ModuleName = m, FieldName = "actualStartDate", FieldLabel = "Actual Start Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 30, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "actualEndDate", FieldLabel = "Actual End Date", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 31, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "objectiveType", FieldLabel = "Objective Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 32, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:Awareness,1:Lead Generation,2:Conversion,3:Retention,4:Revenue", CreatedAt = now },
            // Accordion: Audience
            new() { ModuleName = m, FieldName = "audienceType", FieldLabel = "Audience Type", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 33, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:All,1:New Prospects,2:Existing Customers,3:Churned Customers,4:Segment", CreatedAt = now },
            // Accordion: Event Details
            new() { ModuleName = m, FieldName = "attendance", FieldLabel = "Attendance", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 40, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "noShows", FieldLabel = "No-Shows", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 41, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "eventCapacity", FieldLabel = "Event Capacity", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 42, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "eventDateTime", FieldLabel = "Event Date & Time", FieldType = "datetime", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 43, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "eventLocation", FieldLabel = "Event Location", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 44, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            // Accordion: Admin
            new() { ModuleName = m, FieldName = "costCenter", FieldLabel = "Cost Center", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 50, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "parentCampaignId", FieldLabel = "Parent Campaign ID", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 51, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "externalId", FieldLabel = "External ID", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 52, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "abTestMetric", FieldLabel = "A/B Test Metric", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 53, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g. open_rate", CreatedAt = now },
            // Read-only system metrics in Tab 0 accordions
            new() { ModuleName = m, FieldName = "mqlsGenerated", FieldLabel = "MQLs Generated", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 60, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "System-calculated metric", CreatedAt = now },
            new() { ModuleName = m, FieldName = "sqlsGenerated", FieldLabel = "SQLs Generated", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 61, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "System-calculated metric", CreatedAt = now },
            new() { ModuleName = m, FieldName = "opportunitiesCreated", FieldLabel = "Opportunities Created", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 62, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "System-calculated metric", CreatedAt = now },
            new() { ModuleName = m, FieldName = "dealsWon", FieldLabel = "Deals Won", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 63, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "System-calculated metric", CreatedAt = now },

            // ── Tab 1: Performance ─────────────────────────────────────
            new() { ModuleName = m, FieldName = "impressions", FieldLabel = "Impressions", FieldType = "number", TabIndex = 1, TabName = "Performance", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "clicks", FieldLabel = "Clicks", FieldType = "number", TabIndex = 1, TabName = "Performance", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "conversions", FieldLabel = "Conversions", FieldType = "number", TabIndex = 1, TabName = "Performance", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "leadsGenerated", FieldLabel = "Leads Generated", FieldType = "number", TabIndex = 1, TabName = "Performance", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "revenue", FieldLabel = "Revenue ($)", FieldType = "currency", TabIndex = 1, TabName = "Performance", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "reach", FieldLabel = "Reach", FieldType = "number", TabIndex = 1, TabName = "Performance", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "landingPageVisits", FieldLabel = "Landing Page Visits", FieldType = "number", TabIndex = 1, TabName = "Performance", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // ── Tab 2: Email Metrics ───────────────────────────────────
            new() { ModuleName = m, FieldName = "emailsSent", FieldLabel = "Emails Sent", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "emailsDelivered", FieldLabel = "Emails Delivered", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "emailsOpened", FieldLabel = "Emails Opened", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "emailClicks", FieldLabel = "Email Clicks", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "unsubscribes", FieldLabel = "Unsubscribes", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "bounces", FieldLabel = "Bounces", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "bounceRate", FieldLabel = "Bounce Rate (%)", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Percentage", CreatedAt = now },
            new() { ModuleName = m, FieldName = "deliveryRate", FieldLabel = "Delivery Rate (%)", FieldType = "number", TabIndex = 2, TabName = "Email Metrics", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Percentage", CreatedAt = now },

            // ── Tab 3: Social & A/B ───────────────────────────────────
            new() { ModuleName = m, FieldName = "socialReach", FieldLabel = "Social Reach", FieldType = "number", TabIndex = 3, TabName = "Social & A/B", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "socialEngagement", FieldLabel = "Social Engagement", FieldType = "number", TabIndex = 3, TabName = "Social & A/B", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "socialShares", FieldLabel = "Social Shares", FieldType = "number", TabIndex = 3, TabName = "Social & A/B", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "clickThroughRate", FieldLabel = "Click-Through Rate (%)", FieldType = "number", TabIndex = 3, TabName = "Social & A/B", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "CTR percentage", CreatedAt = now },

            // ── Tab 4: Tracking ────────────────────────────────────────
            new() { ModuleName = m, FieldName = "utmSource", FieldLabel = "UTM Source", FieldType = "text", TabIndex = 4, TabName = "Tracking", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "google", CreatedAt = now },
            new() { ModuleName = m, FieldName = "utmMedium", FieldLabel = "UTM Medium", FieldType = "text", TabIndex = 4, TabName = "Tracking", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "cpc", CreatedAt = now },
            new() { ModuleName = m, FieldName = "utmCampaign", FieldLabel = "UTM Campaign", FieldType = "text", TabIndex = 4, TabName = "Tracking", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "spring_sale", CreatedAt = now },
            new() { ModuleName = m, FieldName = "utmContent", FieldLabel = "UTM Content", FieldType = "text", TabIndex = 4, TabName = "Tracking", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "banner_ad", CreatedAt = now },
            new() { ModuleName = m, FieldName = "utmTerm", FieldLabel = "UTM Term", FieldType = "text", TabIndex = 4, TabName = "Tracking", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "running+shoes", CreatedAt = now },
            new() { ModuleName = m, FieldName = "objectives", FieldLabel = "Objectives", FieldType = "textarea", TabIndex = 4, TabName = "Tracking", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultQuoteFields()
    {
        var now = DateTime.UtcNow;
        var m = "Quotes";
        return new List<ModuleFieldConfiguration>
        {
            // ── Tab 0: Details ─────────────────────────────────────────
            new() { ModuleName = m, FieldName = "quoteNumber", FieldLabel = "Quote Number", FieldType = "text", TabIndex = 0, TabName = "Details", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "title", FieldLabel = "Title", FieldType = "text", TabIndex = 0, TabName = "Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountId", FieldLabel = "Account", FieldType = "lookup", TabIndex = 0, TabName = "Details", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "contactId", FieldLabel = "Contact", FieldType = "lookup", TabIndex = 0, TabName = "Details", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "opportunityId", FieldLabel = "Opportunity", FieldType = "lookup", TabIndex = 0, TabName = "Details", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Details", DisplayOrder = 5, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "0:Draft,1:Pending Approval,2:Approved,3:Sent,4:Accepted,5:Rejected,6:Expired,7:Revised,8:Cancelled", CreatedAt = now },
            new() { ModuleName = m, FieldName = "quoteDate", FieldLabel = "Quote Date", FieldType = "date", TabIndex = 0, TabName = "Details", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "validUntil", FieldLabel = "Valid Until", FieldType = "date", TabIndex = 0, TabName = "Details", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 0, TabName = "Details", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // ── Tab 1: Pricing ─────────────────────────────────────────
            new() { ModuleName = m, FieldName = "subtotal", FieldLabel = "Subtotal", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "discountType", FieldLabel = "Discount Type", FieldType = "select", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, Options = "percentage,fixed", CreatedAt = now },
            new() { ModuleName = m, FieldName = "discountValue", FieldLabel = "Discount Value", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "discountAmount", FieldLabel = "Discount Amount", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "taxRate", FieldLabel = "Tax Rate (%)", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "taxAmount", FieldLabel = "Tax Amount", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCost", FieldLabel = "Shipping Cost", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "total", FieldLabel = "Total", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "currency", FieldLabel = "Currency", FieldType = "select", TabIndex = 1, TabName = "Pricing", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "USD,EUR,GBP,CAD,AUD,JPY,CNY,INR", CreatedAt = now },

            // ── Tab 2: Addresses ───────────────────────────────────────
            new() { ModuleName = m, FieldName = "billingAddress", FieldLabel = "Billing Address", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingCity", FieldLabel = "Billing City", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingState", FieldLabel = "Billing State", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingZipCode", FieldLabel = "Billing Zip Code", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingCountry", FieldLabel = "Billing Country", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingAddress", FieldLabel = "Shipping Address", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCity", FieldLabel = "Shipping City", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingState", FieldLabel = "Shipping State", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingZipCode", FieldLabel = "Shipping Zip Code", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCountry", FieldLabel = "Shipping Country", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // ── Tab 3: Terms & Approval ────────────────────────────────
            new() { ModuleName = m, FieldName = "paymentTerms", FieldLabel = "Payment Terms", FieldType = "select", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Net 15,Net 30,Net 45,Net 60,Due on Receipt,COD,50/50,Custom", CreatedAt = now },
            new() { ModuleName = m, FieldName = "deliveryTerms", FieldLabel = "Delivery Terms", FieldType = "text", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "terms", FieldLabel = "Terms & Conditions", FieldType = "textarea", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "internalNotes", FieldLabel = "Internal Notes", FieldType = "textarea", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "approvalStatus", FieldLabel = "Approval Status", FieldType = "select", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Pending,Approved,Rejected", CreatedAt = now },
            new() { ModuleName = m, FieldName = "approvedByUserId", FieldLabel = "Approved By", FieldType = "number", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "approvedDate", FieldLabel = "Approved Date", FieldType = "date", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "rejectionReason", FieldLabel = "Rejection Reason", FieldType = "text", TabIndex = 3, TabName = "Terms & Approval", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultOrderFields()
    {
        var now = DateTime.UtcNow;
        var m = "Orders";
        return new List<ModuleFieldConfiguration>
        {
            // ── Tab 0: Order Info ──────────────────────────────────────
            new() { ModuleName = m, FieldName = "orderNumber", FieldLabel = "Order Number", FieldType = "text", TabIndex = 0, TabName = "Order Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountId", FieldLabel = "Account", FieldType = "lookup", TabIndex = 0, TabName = "Order Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "contactId", FieldLabel = "Contact", FieldType = "lookup", TabIndex = 0, TabName = "Order Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "quoteId", FieldLabel = "Quote", FieldType = "lookup", TabIndex = 0, TabName = "Order Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Order Info", DisplayOrder = 4, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "0:Draft,1:Submitted,2:Approved,3:In Progress,4:Shipped,5:Delivered,6:Completed,7:Cancelled,8:Returned,9:Refunded", CreatedAt = now },
            new() { ModuleName = m, FieldName = "orderDate", FieldLabel = "Order Date", FieldType = "date", TabIndex = 0, TabName = "Order Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "requestedDeliveryDate", FieldLabel = "Requested Delivery Date", FieldType = "date", TabIndex = 0, TabName = "Order Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 0, TabName = "Order Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            // Accordion: Additional
            new() { ModuleName = m, FieldName = "priority", FieldLabel = "Priority", FieldType = "select", TabIndex = 0, TabName = "Order Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Low,Medium,High,Critical", CreatedAt = now },
            new() { ModuleName = m, FieldName = "salesOwnerId", FieldLabel = "Sales Owner", FieldType = "lookup", TabIndex = 0, TabName = "Order Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "poNumber", FieldLabel = "PO Number", FieldType = "text", TabIndex = 0, TabName = "Order Info", DisplayOrder = 12, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "externalOrderId", FieldLabel = "External Order ID", FieldType = "text", TabIndex = 0, TabName = "Order Info", DisplayOrder = 13, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // ── Tab 1: Pricing ─────────────────────────────────────────
            new() { ModuleName = m, FieldName = "subtotal", FieldLabel = "Subtotal", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "discountAmount", FieldLabel = "Discount", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "taxAmount", FieldLabel = "Tax", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCost", FieldLabel = "Shipping Cost", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "total", FieldLabel = "Total", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "currency", FieldLabel = "Currency", FieldType = "select", TabIndex = 1, TabName = "Pricing", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "USD,EUR,GBP,CAD,AUD,JPY,CNY,INR", CreatedAt = now },

            // ── Tab 2: Shipping ────────────────────────────────────────
            new() { ModuleName = m, FieldName = "shippingAddress", FieldLabel = "Shipping Address", FieldType = "text", TabIndex = 2, TabName = "Shipping", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCity", FieldLabel = "City", FieldType = "text", TabIndex = 2, TabName = "Shipping", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingState", FieldLabel = "State", FieldType = "text", TabIndex = 2, TabName = "Shipping", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingZipCode", FieldLabel = "Zip Code", FieldType = "text", TabIndex = 2, TabName = "Shipping", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCountry", FieldLabel = "Country", FieldType = "text", TabIndex = 2, TabName = "Shipping", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingMethod", FieldLabel = "Shipping Method", FieldType = "select", TabIndex = 2, TabName = "Shipping", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Standard,Express,Overnight,Pickup,Digital", CreatedAt = now },
            new() { ModuleName = m, FieldName = "trackingNumber", FieldLabel = "Tracking Number", FieldType = "text", TabIndex = 2, TabName = "Shipping", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // ── Tab 3: Terms ───────────────────────────────────────────
            new() { ModuleName = m, FieldName = "paymentTerms", FieldLabel = "Payment Terms", FieldType = "select", TabIndex = 3, TabName = "Terms", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Net 15,Net 30,Net 45,Net 60,Due on Receipt,COD", CreatedAt = now },
            new() { ModuleName = m, FieldName = "terms", FieldLabel = "Terms & Conditions", FieldType = "textarea", TabIndex = 3, TabName = "Terms", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Terms", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "internalNotes", FieldLabel = "Internal Notes", FieldType = "textarea", TabIndex = 3, TabName = "Terms", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultInvoiceFields()
    {
        var now = DateTime.UtcNow;
        var m = "Invoices";
        return new List<ModuleFieldConfiguration>
        {
            // ── Tab 0: Invoice Info ────────────────────────────────────
            new() { ModuleName = m, FieldName = "invoiceNumber", FieldLabel = "Invoice Number", FieldType = "text", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountId", FieldLabel = "Account", FieldType = "lookup", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "contactId", FieldLabel = "Contact", FieldType = "lookup", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "orderId", FieldLabel = "Order", FieldType = "lookup", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 4, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "0:Draft,1:Sent,2:Viewed,3:Partially Paid,4:Paid,5:Overdue,6:Void,7:Cancelled,8:Written Off,9:Disputed", CreatedAt = now },
            new() { ModuleName = m, FieldName = "invoiceDate", FieldLabel = "Invoice Date", FieldType = "date", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "dueDate", FieldLabel = "Due Date", FieldType = "date", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            // Accordion
            new() { ModuleName = m, FieldName = "poNumber", FieldLabel = "PO Number", FieldType = "text", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "reference", FieldLabel = "Reference", FieldType = "text", TabIndex = 0, TabName = "Invoice Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // ── Tab 1: Pricing ─────────────────────────────────────────
            new() { ModuleName = m, FieldName = "subtotal", FieldLabel = "Subtotal", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "discountAmount", FieldLabel = "Discount", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "taxRate", FieldLabel = "Tax Rate (%)", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "taxAmount", FieldLabel = "Tax Amount", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCost", FieldLabel = "Shipping Cost", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "total", FieldLabel = "Total", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "amountPaid", FieldLabel = "Amount Paid", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "balanceDue", FieldLabel = "Balance Due", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "currency", FieldLabel = "Currency", FieldType = "select", TabIndex = 1, TabName = "Pricing", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "USD,EUR,GBP,CAD,AUD,JPY,CNY,INR", CreatedAt = now },

            // ── Tab 2: Addresses ───────────────────────────────────────
            new() { ModuleName = m, FieldName = "billingAddress", FieldLabel = "Billing Address", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingCity", FieldLabel = "Billing City", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingState", FieldLabel = "Billing State", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingZipCode", FieldLabel = "Billing Zip Code", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingCountry", FieldLabel = "Billing Country", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingAddress", FieldLabel = "Shipping Address", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCity", FieldLabel = "Shipping City", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingState", FieldLabel = "Shipping State", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingZipCode", FieldLabel = "Shipping Zip Code", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "shippingCountry", FieldLabel = "Shipping Country", FieldType = "text", TabIndex = 2, TabName = "Addresses", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // ── Tab 3: Payment & Terms ─────────────────────────────────
            new() { ModuleName = m, FieldName = "paymentTerms", FieldLabel = "Payment Terms", FieldType = "select", TabIndex = 3, TabName = "Payment & Terms", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Net 15,Net 30,Net 45,Net 60,Due on Receipt", CreatedAt = now },
            new() { ModuleName = m, FieldName = "paymentMethod", FieldLabel = "Payment Method", FieldType = "select", TabIndex = 3, TabName = "Payment & Terms", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Credit Card,Bank Transfer,Check,Cash,PayPal,Other", CreatedAt = now },
            new() { ModuleName = m, FieldName = "lateFeePercentage", FieldLabel = "Late Fee (%)", FieldType = "number", TabIndex = 3, TabName = "Payment & Terms", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "terms", FieldLabel = "Terms & Conditions", FieldType = "textarea", TabIndex = 3, TabName = "Payment & Terms", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Payment & Terms", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "internalNotes", FieldLabel = "Internal Notes", FieldType = "textarea", TabIndex = 3, TabName = "Payment & Terms", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // ── Tab 4: Dates & Tracking ────────────────────────────────
            new() { ModuleName = m, FieldName = "sentDate", FieldLabel = "Sent Date", FieldType = "date", TabIndex = 4, TabName = "Dates & Tracking", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "paidDate", FieldLabel = "Paid Date", FieldType = "date", TabIndex = 4, TabName = "Dates & Tracking", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "lastReminderDate", FieldLabel = "Last Reminder", FieldType = "date", TabIndex = 4, TabName = "Dates & Tracking", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "reminderCount", FieldLabel = "Reminder Count", FieldType = "number", TabIndex = 4, TabName = "Dates & Tracking", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultContractFields()
    {
        var now = DateTime.UtcNow;
        var m = "Contracts";
        return new List<ModuleFieldConfiguration>
        {
            // ── Tab 0: Contract Info ───────────────────────────────────
            new() { ModuleName = m, FieldName = "contractNumber", FieldLabel = "Contract Number", FieldType = "text", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "title", FieldLabel = "Title", FieldType = "text", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountId", FieldLabel = "Account", FieldType = "lookup", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "contactId", FieldLabel = "Contact", FieldType = "lookup", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "contractType", FieldLabel = "Contract Type", FieldType = "select", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:Service Agreement,1:License Agreement,2:NDA,3:SLA,4:Subscription,5:Maintenance,6:Support,7:Consulting,8:Custom", CreatedAt = now },
            new() { ModuleName = m, FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 5, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "0:Draft,1:Pending Approval,2:Active,3:Expired,4:Terminated,5:Suspended,6:Renewed,7:Cancelled", CreatedAt = now },
            new() { ModuleName = m, FieldName = "startDate", FieldLabel = "Start Date", FieldType = "date", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 6, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "endDate", FieldLabel = "End Date", FieldType = "date", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 7, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "contractValue", FieldLabel = "Contract Value ($)", FieldType = "currency", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "billingFrequency", FieldLabel = "Billing Frequency", FieldType = "select", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:Monthly,1:Quarterly,2:Semi-Annual,3:Annual,4:One-Time,5:Custom", CreatedAt = now },
            new() { ModuleName = m, FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            // Accordion: Additional
            new() { ModuleName = m, FieldName = "autoRenew", FieldLabel = "Auto-Renew", FieldType = "checkbox", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 15, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "renewalTermMonths", FieldLabel = "Renewal Term (months)", FieldType = "number", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 16, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "noticePeriodDays", FieldLabel = "Notice Period (days)", FieldType = "number", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 17, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "parentContractId", FieldLabel = "Parent Contract ID", FieldType = "number", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 18, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "specialConditions", FieldLabel = "Special Conditions", FieldType = "textarea", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 19, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            // Documents & approval
            new() { ModuleName = m, FieldName = "contractFileUrl", FieldLabel = "Contract File URL", FieldType = "url", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 20, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "signedContractFileUrl", FieldLabel = "Signed Contract URL", FieldType = "url", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 21, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "approvedByUserId", FieldLabel = "Approved By", FieldType = "number", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 22, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "approvedDate", FieldLabel = "Approved Date", FieldType = "date", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 23, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "rejectionReason", FieldLabel = "Rejection Reason", FieldType = "text", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 24, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "currencyCode", FieldLabel = "Currency Code", FieldType = "text", TabIndex = 0, TabName = "Contract Info", DisplayOrder = 25, IsEnabled = true, IsRequired = false, GridSize = 4, Placeholder = "USD", CreatedAt = now },

            // ── Tab 1: Terms & Conditions ──────────────────────────────
            new() { ModuleName = m, FieldName = "terms", FieldLabel = "Terms & Conditions", FieldType = "textarea", TabIndex = 1, TabName = "Terms & Conditions", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "terminationClause", FieldLabel = "Termination Clause", FieldType = "textarea", TabIndex = 1, TabName = "Terms & Conditions", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "suspensionReason", FieldLabel = "Suspension Reason", FieldType = "text", TabIndex = 1, TabName = "Terms & Conditions", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "suspendedDate", FieldLabel = "Suspended Date", FieldType = "date", TabIndex = 1, TabName = "Terms & Conditions", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // ── Tab 2: Related Records ─────────────────────────────────
            new() { ModuleName = m, FieldName = "opportunityId", FieldLabel = "Related Opportunity", FieldType = "lookup", TabIndex = 2, TabName = "Related Records", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "quoteId", FieldLabel = "Related Quote", FieldType = "number", TabIndex = 2, TabName = "Related Records", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "renewalNoticeSent", FieldLabel = "Renewal Notice Sent", FieldType = "checkbox", TabIndex = 2, TabName = "Related Records", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "renewalNoticeSentDate", FieldLabel = "Notice Sent Date", FieldType = "date", TabIndex = 2, TabName = "Related Records", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultServiceRequestFields()
    {
        var now = DateTime.UtcNow;
        var m = "ServiceRequests";
        return new List<ModuleFieldConfiguration>
        {
            // ── Tab 0: Request Info ────────────────────────────────────
            new() { ModuleName = m, FieldName = "subject", FieldLabel = "Subject", FieldType = "text", TabIndex = 0, TabName = "Request Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 0, TabName = "Request Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "channel", FieldLabel = "Channel", FieldType = "select", TabIndex = 0, TabName = "Request Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "0:WhatsApp,1:Email,2:Phone,3:In Person,4:Self Service Portal,5:Social Media,6:Live Chat,7:API", CreatedAt = now },
            new() { ModuleName = m, FieldName = "priority", FieldLabel = "Priority", FieldType = "select", TabIndex = 0, TabName = "Request Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:Low,1:Medium,2:High,3:Critical,4:Urgent", CreatedAt = now },
            new() { ModuleName = m, FieldName = "categoryId", FieldLabel = "Category", FieldType = "select", TabIndex = 0, TabName = "Request Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Loaded from categories endpoint", CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountId", FieldLabel = "Account", FieldType = "lookup", TabIndex = 0, TabName = "Request Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "contactId", FieldLabel = "Contact", FieldType = "lookup", TabIndex = 0, TabName = "Request Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "assignedToUserId", FieldLabel = "Assign to User", FieldType = "lookup", TabIndex = 0, TabName = "Request Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "assignedToGroupId", FieldLabel = "Assign to Group", FieldType = "select", TabIndex = 0, TabName = "Request Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Loaded from groups endpoint", CreatedAt = now },
            // Accordion: Additional Information
            new() { ModuleName = m, FieldName = "subcategoryId", FieldLabel = "Subcategory", FieldType = "select", TabIndex = 0, TabName = "Request Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Loaded from categories endpoint", CreatedAt = now },
            new() { ModuleName = m, FieldName = "workflowId", FieldLabel = "Workflow", FieldType = "select", TabIndex = 0, TabName = "Request Info", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 12, HelpText = "Loaded from workflows endpoint", CreatedAt = now },

            // ── Tab 1: Resolution & SLA ────────────────────────────────
            new() { ModuleName = m, FieldName = "slaStatus", FieldLabel = "SLA Status", FieldType = "select", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "on_track,at_risk,breached", CreatedAt = now },
            new() { ModuleName = m, FieldName = "isVipAccount", FieldLabel = "VIP Account", FieldType = "checkbox", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "estimatedEffortHours", FieldLabel = "Estimated Effort (hrs)", FieldType = "number", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "actualEffortHours", FieldLabel = "Actual Effort (hrs)", FieldType = "number", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "resolutionCode", FieldLabel = "Resolution Code", FieldType = "text", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "rootCause", FieldLabel = "Root Cause", FieldType = "text", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "resolutionSummary", FieldLabel = "Resolution Summary", FieldType = "textarea", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "internalNotes", FieldLabel = "Internal Notes", FieldType = "textarea", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            // Accordion: Expedite
            new() { ModuleName = m, FieldName = "isExpedited", FieldLabel = "Expedited", FieldType = "checkbox", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "expediteReason", FieldLabel = "Expedite Reason", FieldType = "text", TabIndex = 1, TabName = "Resolution & SLA", DisplayOrder = 11, IsEnabled = true, IsRequired = false, GridSize = 12, ParentField = "isExpedited", ParentFieldValue = "true", CreatedAt = now },

            // ── Tab 2: Feedback & Reference ────────────────────────────
            new() { ModuleName = m, FieldName = "satisfactionRating", FieldLabel = "Satisfaction Rating", FieldType = "select", TabIndex = 2, TabName = "Feedback & Reference", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "1:1 - Very Unsatisfied,2:2 - Unsatisfied,3:3 - Neutral,4:4 - Satisfied,5:5 - Very Satisfied", CreatedAt = now },
            new() { ModuleName = m, FieldName = "customerFeedback", FieldLabel = "Customer Feedback", FieldType = "textarea", TabIndex = 2, TabName = "Feedback & Reference", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "externalReferenceId", FieldLabel = "External Reference ID", FieldType = "text", TabIndex = 2, TabName = "Feedback & Reference", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultPaymentFields()
    {
        var now = DateTime.UtcNow;
        var m = "Payments";
        return new List<ModuleFieldConfiguration>
        {
            // ── Tab 0: Payment Info ────────────────────────────────────
            new() { ModuleName = m, FieldName = "invoiceId", FieldLabel = "Invoice ID", FieldType = "number", TabIndex = 0, TabName = "Payment Info", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "amount", FieldLabel = "Amount ($)", FieldType = "currency", TabIndex = 0, TabName = "Payment Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "paymentMethod", FieldLabel = "Payment Method", FieldType = "select", TabIndex = 0, TabName = "Payment Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "0:Credit Card,1:Debit Card,2:Bank Transfer,3:Wire Transfer,4:Check,5:Cash,6:PayPal,7:Stripe,8:Apple Pay,9:Google Pay,10:Venmo,11:Crypto,12:Store Credit,13:Gift Card,14:Financing,15:Purchase Order,16:Other", CreatedAt = now },
            // Accordion: Additional
            new() { ModuleName = m, FieldName = "reference", FieldLabel = "Reference / Transaction ID", FieldType = "text", TabIndex = 0, TabName = "Payment Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = m, FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 0, TabName = "Payment Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },

            // ── Tab 1: Identifiers ─────────────────────────────────────
            new() { ModuleName = m, FieldName = "paymentNumber", FieldLabel = "Payment Number", FieldType = "text", TabIndex = 1, TabName = "Identifiers", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Auto-generated", CreatedAt = now },
            new() { ModuleName = m, FieldName = "externalPaymentId", FieldLabel = "External Payment ID", FieldType = "text", TabIndex = 1, TabName = "Identifiers", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "gatewayTransactionId", FieldLabel = "Gateway Transaction ID", FieldType = "text", TabIndex = 1, TabName = "Identifiers", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "gatewayReference", FieldLabel = "Gateway Reference", FieldType = "text", TabIndex = 1, TabName = "Identifiers", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "checkNumber", FieldLabel = "Check Number", FieldType = "text", TabIndex = 1, TabName = "Identifiers", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // ── Tab 2: Financial & Dates ───────────────────────────────
            new() { ModuleName = m, FieldName = "amountApplied", FieldLabel = "Amount Applied", FieldType = "currency", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "processingFee", FieldLabel = "Processing Fee", FieldType = "currency", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "exchangeRate", FieldLabel = "Exchange Rate", FieldType = "number", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "processedDate", FieldLabel = "Processed Date", FieldType = "date", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "settledDate", FieldLabel = "Settled Date", FieldType = "date", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "depositDate", FieldLabel = "Deposit Date", FieldType = "date", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "scheduledDate", FieldLabel = "Scheduled Date", FieldType = "date", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountId", FieldLabel = "Account ID", FieldType = "number", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "originalPaymentId", FieldLabel = "Original Payment ID", FieldType = "number", TabIndex = 2, TabName = "Financial & Dates", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },

            // ── Tab 3: Payment Details ─────────────────────────────────
            new() { ModuleName = m, FieldName = "cardBrand", FieldLabel = "Card Brand", FieldType = "text", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "cardLast4", FieldLabel = "Card Last 4", FieldType = "text", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "cardExpMonth", FieldLabel = "Exp Month", FieldType = "number", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "cardExpYear", FieldLabel = "Exp Year", FieldType = "number", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "cardholderName", FieldLabel = "Cardholder Name", FieldType = "text", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "bankName", FieldLabel = "Bank Name", FieldType = "text", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountLast4", FieldLabel = "Account Last 4", FieldType = "text", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = m, FieldName = "accountType", FieldLabel = "Account Type", FieldType = "select", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 4, Options = "checking,savings,business", CreatedAt = now },
            new() { ModuleName = m, FieldName = "gateway", FieldLabel = "Gateway", FieldType = "text", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "gatewayResponseCode", FieldLabel = "Response Code", FieldType = "text", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 9, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = m, FieldName = "internalNotes", FieldLabel = "Internal Notes", FieldType = "textarea", TabIndex = 3, TabName = "Payment Details", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }
}
