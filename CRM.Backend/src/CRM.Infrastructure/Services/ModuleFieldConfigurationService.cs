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

        if (dto.FieldLabel != null) entity.FieldLabel = dto.FieldLabel;
        if (dto.TabIndex.HasValue) entity.TabIndex = dto.TabIndex.Value;
        if (dto.TabName != null) entity.TabName = dto.TabName;
        if (dto.DisplayOrder.HasValue) entity.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsEnabled.HasValue) entity.IsEnabled = dto.IsEnabled.Value;
        if (dto.IsRequired.HasValue) entity.IsRequired = dto.IsRequired.Value;
        if (dto.GridSize.HasValue) entity.GridSize = dto.GridSize.Value;
        if (dto.Placeholder != null) entity.Placeholder = dto.Placeholder;
        if (dto.HelpText != null) entity.HelpText = dto.HelpText;
        if (dto.Options != null) entity.Options = dto.Options;

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

        List<ModuleFieldConfiguration> configs = moduleName switch
        {
            ModuleNames.Customers => GetDefaultCustomerFields(),
            ModuleNames.Contacts => GetDefaultContactFields(),
            ModuleNames.Leads => GetDefaultLeadFields(),
            ModuleNames.Opportunities => GetDefaultOpportunityFields(),
            ModuleNames.Products => GetDefaultProductFields(),
            _ => new List<ModuleFieldConfiguration>()
        };

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
        var allModuleNames = new[] 
        { 
            ModuleNames.Customers, 
            ModuleNames.Contacts, 
            ModuleNames.Leads, 
            ModuleNames.Opportunities,
            ModuleNames.Products
        };

        var results = new Dictionary<string, int>();

        foreach (var moduleName in allModuleNames)
        {
            var existingCount = await _context.ModuleFieldConfigurations
                .CountAsync(c => c.ModuleName == moduleName);

            if (existingCount > 0)
            {
                results[moduleName] = existingCount;
                continue;
            }

            List<ModuleFieldConfiguration> configs = moduleName switch
            {
                ModuleNames.Customers => GetDefaultCustomerFields(),
                ModuleNames.Contacts => GetDefaultContactFields(),
                ModuleNames.Leads => GetDefaultLeadFields(),
                ModuleNames.Opportunities => GetDefaultOpportunityFields(),
                ModuleNames.Products => GetDefaultProductFields(),
                _ => new List<ModuleFieldConfiguration>()
            };

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
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Customers", FieldName = "category", FieldLabel = "Customer Category", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, Options = "Individual,Organization", IsReorderable = false, IsRequiredConfigurable = false, IsHideable = false, CreatedAt = now },
            
            // Individual fields
            new() { ModuleName = "Customers", FieldName = "salutation", FieldLabel = "Salutation", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 10, IsEnabled = true, IsRequired = false, GridSize = 2, Options = "Mr.,Mrs.,Ms.,Dr.,Prof.", ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "firstName", FieldLabel = "First Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 11, IsEnabled = true, IsRequired = true, GridSize = 4, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "lastName", FieldLabel = "Last Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 12, IsEnabled = true, IsRequired = true, GridSize = 4, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "suffix", FieldLabel = "Suffix", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 13, IsEnabled = true, IsRequired = false, GridSize = 2, Placeholder = "Jr., III", ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "dateOfBirth", FieldLabel = "Date of Birth", FieldType = "date", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 14, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "gender", FieldLabel = "Gender", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 15, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Male,Female,Other,Prefer not to say", ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            
            // Organization fields
            new() { ModuleName = "Customers", FieldName = "company", FieldLabel = "Company Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 20, IsEnabled = true, IsRequired = true, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "legalName", FieldLabel = "Legal Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 21, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Full legal entity name", ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "dbaName", FieldLabel = "DBA Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 22, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "Doing Business As", ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "taxId", FieldLabel = "Tax ID / EIN", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 23, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "registrationNumber", FieldLabel = "Registration Number", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 24, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "yearFounded", FieldLabel = "Year Founded", FieldType = "number", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 25, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "1", CreatedAt = now },
            
            // Contact Information (common)
            new() { ModuleName = "Customers", FieldName = "email", FieldLabel = "Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 30, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "secondaryEmail", FieldLabel = "Secondary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 31, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "phone", FieldLabel = "Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 32, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "mobilePhone", FieldLabel = "Mobile Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 33, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 34, IsEnabled = true, IsRequired = false, GridSize = 6, ParentField = "category", ParentFieldValue = "0", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "website", FieldLabel = "Website", FieldType = "url", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 35, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            
            // Address
            new() { ModuleName = "Customers", FieldName = "address", FieldLabel = "Address", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 40, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 41, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "state", FieldLabel = "State", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 42, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "zipCode", FieldLabel = "Zip Code", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 43, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            
            // Business Tab (1)
            new() { ModuleName = "Customers", FieldName = "customerType", FieldLabel = "Customer Type", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Individual,Small Business,Mid-Market,Enterprise,Government,Non-Profit", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "lifecycleStage", FieldLabel = "Lifecycle Stage", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Lead,Prospect,Opportunity,Customer,Churned,Reactivated", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "priority", FieldLabel = "Priority", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Low,Medium,High,Critical", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "industry", FieldLabel = "Industry", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Technology,Healthcare,Finance,Retail,Manufacturing,Education,Real Estate,Consulting,Marketing,Legal,Non-Profit,Government,Other", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "annualRevenue", FieldLabel = "Annual Revenue ($)", FieldType = "currency", TabIndex = 1, TabName = "Business", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "numberOfEmployees", FieldLabel = "Number of Employees", FieldType = "number", TabIndex = 1, TabName = "Business", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "creditLimit", FieldLabel = "Credit Limit ($)", FieldType = "currency", TabIndex = 1, TabName = "Business", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "leadSource", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 1, TabName = "Business", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Website,Referral,Social Media,Cold Call,Trade Show,Advertisement,Email Campaign,Partner,Other", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "leadScore", FieldLabel = "Lead Score", FieldType = "number", TabIndex = 1, TabName = "Business", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 12, HelpText = "Lead score from 0-100", CreatedAt = now },
            
            // Contact Preferences Tab (2)
            new() { ModuleName = "Customers", FieldName = "preferredContactMethod", FieldLabel = "Preferred Contact Method", FieldType = "select", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Email,Phone,SMS,Mail", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "timezone", FieldLabel = "Timezone", FieldType = "text", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g., America/New_York", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "optInEmail", FieldLabel = "Email Communications", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "optInPhone", FieldLabel = "Phone Calls", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "optInSms", FieldLabel = "SMS Messages", FieldType = "checkbox", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "linkedInUrl", FieldLabel = "LinkedIn URL", FieldType = "url", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "twitterHandle", FieldLabel = "Twitter Handle", FieldType = "text", TabIndex = 2, TabName = "Contact Preferences", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Additional Tab (3 for individuals, 4 for organizations)
            new() { ModuleName = "Customers", FieldName = "territory", FieldLabel = "Territory", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "paymentTerms", FieldLabel = "Payment Terms", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Placeholder = "e.g., Net 30", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "tags", FieldLabel = "Tags (comma-separated)", FieldType = "text", TabIndex = 3, TabName = "Additional", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, Placeholder = "vip, enterprise, priority", CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Customers", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
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
            new() { ModuleName = "Contacts", FieldName = "emailPrimary", FieldLabel = "Primary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "emailSecondary", FieldLabel = "Secondary Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 6, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phonePrimary", FieldLabel = "Primary Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 7, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "phoneSecondary", FieldLabel = "Secondary Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 8, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Work Info Tab (1)
            new() { ModuleName = "Contacts", FieldName = "company", FieldLabel = "Company", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "department", FieldLabel = "Department", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "reportsTo", FieldLabel = "Reports To", FieldType = "text", TabIndex = 1, TabName = "Work Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Address Tab (2)
            new() { ModuleName = "Contacts", FieldName = "addressLine1", FieldLabel = "Address Line 1", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "addressLine2", FieldLabel = "Address Line 2", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "state", FieldLabel = "State/Province", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "zipCode", FieldLabel = "Zip/Postal Code", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "country", FieldLabel = "Country", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            
            // Additional Tab (3)
            new() { ModuleName = "Contacts", FieldName = "dateOfBirth", FieldLabel = "Date of Birth", FieldType = "date", TabIndex = 3, TabName = "Additional", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Contacts", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Additional", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
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
            new() { ModuleName = "Leads", FieldName = "email", FieldLabel = "Email", FieldType = "email", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "phone", FieldLabel = "Phone", FieldType = "phone", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "company", FieldLabel = "Company", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "jobTitle", FieldLabel = "Job Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Lead Details Tab (1)
            new() { ModuleName = "Leads", FieldName = "status", FieldLabel = "Status", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "New,Contacted,Qualified,Unqualified,Converted", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "source", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Website,Referral,Social Media,Cold Call,Trade Show,Advertisement,Partner,Other", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "rating", FieldLabel = "Rating", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Hot,Warm,Cold", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "score", FieldLabel = "Lead Score", FieldType = "number", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Score from 0-100", CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "estimatedValue", FieldLabel = "Estimated Value ($)", FieldType = "currency", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "industry", FieldLabel = "Industry", FieldType = "select", TabIndex = 1, TabName = "Lead Details", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Technology,Healthcare,Finance,Retail,Manufacturing,Education,Other", CreatedAt = now },
            
            // Address Tab (2)
            new() { ModuleName = "Leads", FieldName = "address", FieldLabel = "Address", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "city", FieldLabel = "City", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "state", FieldLabel = "State", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "zipCode", FieldLabel = "Zip Code", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "country", FieldLabel = "Country", FieldType = "text", TabIndex = 2, TabName = "Address", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            
            // Notes Tab (3)
            new() { ModuleName = "Leads", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 3, TabName = "Notes", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Leads", FieldName = "notes", FieldLabel = "Notes", FieldType = "textarea", TabIndex = 3, TabName = "Notes", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultOpportunityFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Opportunities", FieldName = "title", FieldLabel = "Opportunity Title", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "customerId", FieldLabel = "Customer", FieldType = "lookup", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "contactId", FieldLabel = "Primary Contact", FieldType = "lookup", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Opportunity Details Tab (1)
            new() { ModuleName = "Opportunities", FieldName = "stage", FieldLabel = "Stage", FieldType = "select", TabIndex = 1, TabName = "Details", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, Options = "Prospecting,Qualification,Proposal,Negotiation,Closed Won,Closed Lost", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "probability", FieldLabel = "Probability (%)", FieldType = "number", TabIndex = 1, TabName = "Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, HelpText = "Win probability 0-100%", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "amount", FieldLabel = "Amount ($)", FieldType = "currency", TabIndex = 1, TabName = "Details", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "expectedCloseDate", FieldLabel = "Expected Close Date", FieldType = "date", TabIndex = 1, TabName = "Details", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "source", FieldLabel = "Lead Source", FieldType = "select", TabIndex = 1, TabName = "Details", DisplayOrder = 4, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Website,Referral,Social Media,Cold Call,Trade Show,Partner,Other", CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "type", FieldLabel = "Type", FieldType = "select", TabIndex = 1, TabName = "Details", DisplayOrder = 5, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "New Business,Existing Business,Upsell,Renewal", CreatedAt = now },
            
            // Notes Tab (2)
            new() { ModuleName = "Opportunities", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 2, TabName = "Notes", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "nextSteps", FieldLabel = "Next Steps", FieldType = "textarea", TabIndex = 2, TabName = "Notes", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Opportunities", FieldName = "competitorInfo", FieldLabel = "Competitor Information", FieldType = "textarea", TabIndex = 2, TabName = "Notes", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }

    private List<ModuleFieldConfiguration> GetDefaultProductFields()
    {
        var now = DateTime.UtcNow;
        return new List<ModuleFieldConfiguration>
        {
            // Basic Info Tab (0)
            new() { ModuleName = "Products", FieldName = "name", FieldLabel = "Product Name", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 8, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "sku", FieldLabel = "SKU", FieldType = "text", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 1, IsEnabled = true, IsRequired = true, GridSize = 4, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "category", FieldLabel = "Category", FieldType = "select", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, Options = "Software,Hardware,Service,Subscription,Other", CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "isActive", FieldLabel = "Active", FieldType = "checkbox", TabIndex = 0, TabName = "Basic Info", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Pricing Tab (1)
            new() { ModuleName = "Products", FieldName = "price", FieldLabel = "Price ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 0, IsEnabled = true, IsRequired = true, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "cost", FieldLabel = "Cost ($)", FieldType = "currency", TabIndex = 1, TabName = "Pricing", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "quantity", FieldLabel = "Quantity in Stock", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 2, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "reorderLevel", FieldLabel = "Reorder Level", FieldType = "number", TabIndex = 1, TabName = "Pricing", DisplayOrder = 3, IsEnabled = true, IsRequired = false, GridSize = 6, CreatedAt = now },
            
            // Details Tab (2)
            new() { ModuleName = "Products", FieldName = "description", FieldLabel = "Description", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 0, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
            new() { ModuleName = "Products", FieldName = "features", FieldLabel = "Features", FieldType = "textarea", TabIndex = 2, TabName = "Details", DisplayOrder = 1, IsEnabled = true, IsRequired = false, GridSize = 12, CreatedAt = now },
        };
    }
}
