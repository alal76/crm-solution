// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Globalization;
using System.Text;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ContactModel = CRM.Core.Models.Contact;
using ImportError = CRM.Core.Interfaces.ImportError;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for import/export operations
/// </summary>
public class ImportExportService : IImportExportService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ImportExportService> _logger;

    private static readonly Dictionary<string, EntityTypeInfo> SupportedEntityTypes = new()
    {
        ["accounts"] = new EntityTypeInfo { Name = "accounts", Label = "Accounts", CanImport = true, CanExport = true },
        ["contacts"] = new EntityTypeInfo { Name = "contacts", Label = "Contacts", CanImport = true, CanExport = true },
        ["leads"] = new EntityTypeInfo { Name = "leads", Label = "Leads", CanImport = true, CanExport = true },
        ["opportunities"] = new EntityTypeInfo { Name = "opportunities", Label = "Opportunities", CanImport = true, CanExport = true },
        ["products"] = new EntityTypeInfo { Name = "products", Label = "Products", CanImport = true, CanExport = true },
        ["interactions"] = new EntityTypeInfo { Name = "interactions", Label = "Interactions", CanImport = false, CanExport = true },
        ["tasks"] = new EntityTypeInfo { Name = "tasks", Label = "Tasks", CanImport = false, CanExport = true }
    };

    public ImportExportService(ICrmDbContext dbContext, ILogger<ImportExportService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IEnumerable<EntityTypeInfo> GetEntityTypes()
    {
        return SupportedEntityTypes.Values;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportToJsonAsync(string entityType)
    {
        _logger.LogDebug("Exporting {EntityType} to JSON", entityType);

        try
        {
            var data = await GetEntityDataAsync(entityType.ToLowerInvariant());
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {EntityType} to JSON", entityType);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportToCsvAsync(string entityType)
    {
        _logger.LogDebug("Exporting {EntityType} to CSV", entityType);

        try
        {
            var csv = entityType.ToLowerInvariant() switch
            {
                "accounts" => await ExportAccountsToCsvAsync(),
                "contacts" => await ExportContactsToCsvAsync(),
                "leads" => await ExportLeadsToCsvAsync(),
                "opportunities" => await ExportOpportunitiesToCsvAsync(),
                "products" => await ExportProductsToCsvAsync(),
                "interactions" => await ExportInteractionsToCsvAsync(),
                "tasks" => await ExportTasksToCsvAsync(),
                _ => throw new ArgumentException($"Unsupported entity type: {entityType}")
            };

            return Encoding.UTF8.GetBytes(csv);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {EntityType} to CSV", entityType);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ImportResult> ImportFromJsonAsync(string entityType, byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        _logger.LogInformation("Importing {EntityType} from JSON", entityType);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new ImportResult { Success = false, Errors = new[] { new ImportError { Message = "JSON content is empty" } } };
        }

        try
        {
            return entityType.ToLowerInvariant() switch
            {
                "accounts" => await ImportAccountsFromJsonAsync(json),
                "contacts" => await ImportContactsFromJsonAsync(json),
                "leads" => await ImportLeadsFromJsonAsync(json),
                "opportunities" => await ImportOpportunitiesFromJsonAsync(json),
                "products" => await ImportProductsFromJsonAsync(json),
                _ => new ImportResult { Success = false, Errors = new[] { new ImportError { Message = $"Import not supported for entity type: {entityType}" } } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing {EntityType} from JSON", entityType);
            return new ImportResult { Success = false, Errors = new[] { new ImportError { Message = ex.Message } } };
        }
    }

    /// <inheritdoc />
    public async Task<ImportResult> ImportFromCsvAsync(string entityType, byte[] data)
    {
        var csv = Encoding.UTF8.GetString(data);
        _logger.LogInformation("Importing {EntityType} from CSV", entityType);

        if (string.IsNullOrWhiteSpace(csv))
        {
            return new ImportResult { Success = false, Errors = new[] { new ImportError { Message = "CSV content is empty" } } };
        }

        try
        {
            return entityType.ToLowerInvariant() switch
            {
                "accounts" => await ImportAccountsFromCsvAsync(csv),
                "contacts" => await ImportContactsFromCsvAsync(csv),
                "leads" => await ImportLeadsFromCsvAsync(csv),
                "opportunities" => await ImportOpportunitiesFromCsvAsync(csv),
                "products" => await ImportProductsFromCsvAsync(csv),
                _ => new ImportResult { Success = false, Errors = new[] { new ImportError { Message = $"Import not supported for entity type: {entityType}" } } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing {EntityType} from CSV", entityType);
            return new ImportResult { Success = false, Errors = new[] { new ImportError { Message = ex.Message } } };
        }
    }

    /// <inheritdoc />
    public byte[] GetTemplateJson(string entityType)
    {
        _logger.LogDebug("Generating JSON template for {EntityType}", entityType);

        var template = entityType.ToLowerInvariant() switch
        {
            "accounts" => new[] { new { FirstName = "", LastName = "", Email = "example@domain.com", Phone = "", Company = "", Industry = "" } },
            "contacts" => new[] { new { FirstName = "", LastName = "", Email = "", Phone = "", JobTitle = "" } },
            "leads" => new[] { new { FirstName = "", LastName = "", Email = "", Phone = "", Company = "", Source = "" } },
            "opportunities" => new[] { new { Name = "", AccountId = 0, Amount = 0, Probability = 0 } },
            "products" => new[] { new { Name = "", SKU = "", Price = 0 } },
            _ => (object)Array.Empty<object>()
        };

        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    /// <inheritdoc />
    public byte[] GetTemplateCsv(string entityType)
    {
        _logger.LogDebug("Generating CSV template for {EntityType}", entityType);

        var header = entityType.ToLowerInvariant() switch
        {
            "accounts" => "FirstName,LastName,Email,Phone,Company,Industry",
            "contacts" => "FirstName,LastName,Email,Phone,JobTitle",
            "leads" => "FirstName,LastName,Email,Phone,Company,Source",
            "opportunities" => "Name,AccountId,Amount,Probability",
            "products" => "Name,SKU,Price",
            _ => ""
        };

        return Encoding.UTF8.GetBytes(header + "\n");
    }

    #region Export Methods

    private async Task<object> GetEntityDataAsync(string entityType)
    {
        return entityType switch
        {
            "accounts" => await _dbContext.Accounts.Where(a => !a.IsDeleted).ToListAsync(),
            "contacts" => await _dbContext.Contacts.Where(c => c.Status == CRM.Core.Models.ContactStatus.Active).ToListAsync(),
            "leads" => await _dbContext.Leads.Where(l => !l.IsDeleted).ToListAsync(),
            "opportunities" => await _dbContext.Opportunities.Where(o => !o.IsDeleted).ToListAsync(),
            "products" => await _dbContext.Products.Where(p => !p.IsDeleted).ToListAsync(),
            "interactions" => await _dbContext.Interactions.Where(i => !i.IsDeleted).ToListAsync(),
            "tasks" => await _dbContext.CrmTasks.Where(t => !t.IsDeleted).ToListAsync(),
            _ => throw new ArgumentException($"Unsupported entity type: {entityType}")
        };
    }

    private async Task<string> ExportAccountsToCsvAsync()
    {
        var accounts = await _dbContext.Accounts.Where(a => !a.IsDeleted).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,FirstName,LastName,Email,Phone,Company,Industry,LifecycleStage,CreatedAt");

        foreach (var account in accounts)
        {
            sb.AppendLine($"{account.Id},{EscapeCsv(account.FirstName)},{EscapeCsv(account.LastName)},{EscapeCsv(account.Email)},{EscapeCsv(account.Phone)},{EscapeCsv(account.Company)},{EscapeCsv(account.Industry)},{account.LifecycleStage},{account.CreatedAt:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    private async Task<string> ExportContactsToCsvAsync()
    {
        var contacts = await _dbContext.Contacts.Where(c => c.Status == CRM.Core.Models.ContactStatus.Active).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,FirstName,LastName,Email,Phone,JobTitle,DateAdded");

        foreach (var contact in contacts)
        {
            sb.AppendLine($"{contact.Id},{EscapeCsv(contact.FirstName)},{EscapeCsv(contact.LastName)},{EscapeCsv(contact.EmailPrimary)},{EscapeCsv(contact.PhonePrimary)},{EscapeCsv(contact.JobTitle)},{contact.DateAdded:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    private async Task<string> ExportLeadsToCsvAsync()
    {
        var leads = await _dbContext.Leads.Where(l => !l.IsDeleted).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,FirstName,LastName,Email,Phone,CompanyName,Status,Source,CreatedAt");

        foreach (var lead in leads)
        {
            sb.AppendLine($"{lead.Id},{EscapeCsv(lead.FirstName)},{EscapeCsv(lead.LastName)},{EscapeCsv(lead.Email)},{EscapeCsv(lead.Phone)},{EscapeCsv(lead.CompanyName)},{lead.Status},{lead.Source},{lead.CreatedAt:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    private async Task<string> ExportOpportunitiesToCsvAsync()
    {
        var opportunities = await _dbContext.Opportunities.Where(o => !o.IsDeleted).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,AccountId,Stage,Amount,Probability,ExpectedCloseDate,CreatedAt");

        foreach (var opp in opportunities)
        {
            sb.AppendLine($"{opp.Id},{EscapeCsv(opp.Name)},{opp.AccountId},{opp.Stage},{opp.Amount},{opp.Probability},{opp.ExpectedCloseDate?.ToString("yyyy-MM-dd")},{opp.CreatedAt:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    private async Task<string> ExportProductsToCsvAsync()
    {
        var products = await _dbContext.Products.Where(p => !p.IsDeleted).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,SKU,Price,IsActive,CreatedAt");

        foreach (var product in products)
        {
            sb.AppendLine($"{product.Id},{EscapeCsv(product.Name)},{EscapeCsv(product.SKU)},{product.Price},{product.IsActive},{product.CreatedAt:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    private async Task<string> ExportInteractionsToCsvAsync()
    {
        var interactions = await _dbContext.Interactions.Where(i => !i.IsDeleted).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,AccountId,InteractionType,Direction,Subject,InteractionDate,Outcome,CreatedAt");

        foreach (var interaction in interactions)
        {
            sb.AppendLine($"{interaction.Id},{interaction.AccountId},{interaction.InteractionType},{interaction.Direction},{EscapeCsv(interaction.Subject)},{interaction.InteractionDate:yyyy-MM-dd},{interaction.Outcome},{interaction.CreatedAt:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    private async Task<string> ExportTasksToCsvAsync()
    {
        var tasks = await _dbContext.CrmTasks.Where(t => !t.IsDeleted).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Id,Subject,Description,Status,Priority,DueDate,CreatedAt");

        foreach (var task in tasks)
        {
            sb.AppendLine($"{task.Id},{EscapeCsv(task.Subject)},{EscapeCsv(task.Description)},{task.Status},{task.Priority},{task.DueDate?.ToString("yyyy-MM-dd")},{task.CreatedAt:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    #endregion

    #region Import Methods

    private async Task<ImportResult> ImportAccountsFromJsonAsync(string json)
    {
        var records = JsonSerializer.Deserialize<List<AccountImportDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<AccountImportDto>();

        return await ImportAccountsAsync(records);
    }

    private async Task<ImportResult> ImportAccountsFromCsvAsync(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            return new ImportResult { Success = true, TotalRecords = 0, ImportedRecords = 0 };
        }

        var records = new List<AccountImportDto>();
        var errors = new List<ImportError>();

        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var values = ParseCsvLine(lines[i]);
                if (values.Length >= 6)
                {
                    records.Add(new AccountImportDto
                    {
                        FirstName = values[0],
                        LastName = values[1],
                        Email = values[2],
                        Phone = values[3],
                        Company = values[4],
                        Industry = values[5]
                    });
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { RowNumber = i, Field = "csv_parse", Message = ex.Message });
            }
        }

        var result = await ImportAccountsAsync(records);
        if (errors.Count > 0)
        {
            result.Errors = (result.Errors ?? Array.Empty<ImportError>()).Concat(errors);
        }
        return result;
    }

    private async Task<ImportResult> ImportAccountsAsync(List<AccountImportDto> records)
    {
        var result = new ImportResult { TotalRecords = records.Count };
        var errors = new List<ImportError>();

        foreach (var (record, index) in records.Select((r, i) => (r, i)))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(record.Email))
                {
                    errors.Add(new ImportError { RowNumber = index + 1, Field = "Email", Message = "Email is required" });
                    continue;
                }

                var account = new Account
                {
                    FirstName = record.FirstName ?? string.Empty,
                    LastName = record.LastName ?? string.Empty,
                    Email = record.Email,
                    Phone = record.Phone ?? string.Empty,
                    Company = record.Company ?? string.Empty,
                    Industry = record.Industry,
                    LifecycleStage = AccountLifecycleStage.Other,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Accounts.Add(account);
                result.ImportedRecords++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { RowNumber = index + 1, Field = "general", Message = ex.Message });
            }
        }

        await _dbContext.SaveChangesAsync();
        result.FailedRecords = result.TotalRecords - result.ImportedRecords;
        result.Success = result.ImportedRecords > 0;
        result.Errors = errors;
        return result;
    }

    private async Task<ImportResult> ImportContactsFromJsonAsync(string json)
    {
        var records = JsonSerializer.Deserialize<List<ContactImportDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<ContactImportDto>();

        return await ImportContactsAsync(records);
    }

    private async Task<ImportResult> ImportContactsFromCsvAsync(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            return new ImportResult { Success = true, TotalRecords = 0, ImportedRecords = 0 };
        }

        var records = new List<ContactImportDto>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            if (values.Length >= 4)
            {
                records.Add(new ContactImportDto
                {
                    FirstName = values[0],
                    LastName = values[1],
                    Email = values[2],
                    Phone = values[3],
                    Title = values.Length > 4 ? values[4] : null
                });
            }
        }

        return await ImportContactsAsync(records);
    }

    private async Task<ImportResult> ImportContactsAsync(List<ContactImportDto> records)
    {
        var result = new ImportResult { TotalRecords = records.Count };
        var errors = new List<ImportError>();

        foreach (var (record, index) in records.Select((r, i) => (r, i)))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(record.FirstName) || string.IsNullOrWhiteSpace(record.LastName))
                {
                    errors.Add(new ImportError { RowNumber = index + 1, Field = "Name", Message = "First and Last name are required" });
                    continue;
                }

                var contact = new ContactModel
                {
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    EmailPrimary = record.Email,
                    PhonePrimary = record.Phone,
                    JobTitle = record.Title,
                    Status = CRM.Core.Models.ContactStatus.Active,
                    DateAdded = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                _dbContext.Contacts.Add(contact);
                result.ImportedRecords++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { RowNumber = index + 1, Field = "general", Message = ex.Message });
            }
        }

        await _dbContext.SaveChangesAsync();
        result.FailedRecords = result.TotalRecords - result.ImportedRecords;
        result.Success = result.ImportedRecords > 0;
        result.Errors = errors;
        return result;
    }

    private async Task<ImportResult> ImportLeadsFromJsonAsync(string json)
    {
        var records = JsonSerializer.Deserialize<List<LeadImportDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<LeadImportDto>();

        return await ImportLeadsAsync(records);
    }

    private async Task<ImportResult> ImportLeadsFromCsvAsync(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            return new ImportResult { Success = true, TotalRecords = 0, ImportedRecords = 0 };
        }

        var records = new List<LeadImportDto>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            if (values.Length >= 4)
            {
                records.Add(new LeadImportDto
                {
                    FirstName = values[0],
                    LastName = values[1],
                    Email = values[2],
                    Phone = values[3],
                    Company = values.Length > 4 ? values[4] : null,
                    Source = values.Length > 5 ? values[5] : null
                });
            }
        }

        return await ImportLeadsAsync(records);
    }

    private async Task<ImportResult> ImportLeadsAsync(List<LeadImportDto> records)
    {
        var result = new ImportResult { TotalRecords = records.Count };
        var errors = new List<ImportError>();

        foreach (var (record, index) in records.Select((r, i) => (r, i)))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(record.FirstName) || string.IsNullOrWhiteSpace(record.LastName))
                {
                    errors.Add(new ImportError { RowNumber = index + 1, Field = "Name", Message = "First and Last name are required" });
                    continue;
                }

                var lead = new Lead
                {
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email ?? string.Empty,
                    Phone = record.Phone,
                    CompanyName = record.Company,
                    Source = ParseLeadSource(record.Source),
                    Status = LeadLifecycleStatus.New,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Leads.Add(lead);
                result.ImportedRecords++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { RowNumber = index + 1, Field = "general", Message = ex.Message });
            }
        }

        await _dbContext.SaveChangesAsync();
        result.FailedRecords = result.TotalRecords - result.ImportedRecords;
        result.Success = result.ImportedRecords > 0;
        result.Errors = errors;
        return result;
    }

    private async Task<ImportResult> ImportOpportunitiesFromJsonAsync(string json)
    {
        var records = JsonSerializer.Deserialize<List<OpportunityImportDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<OpportunityImportDto>();

        return await ImportOpportunitiesAsync(records);
    }

    private async Task<ImportResult> ImportOpportunitiesFromCsvAsync(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            return new ImportResult { Success = true, TotalRecords = 0, ImportedRecords = 0 };
        }

        var records = new List<OpportunityImportDto>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            if (values.Length >= 2)
            {
                records.Add(new OpportunityImportDto
                {
                    Name = values[0],
                    AccountId = int.TryParse(values[1], out var accId) ? accId : null,
                    Amount = values.Length > 2 && decimal.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : null,
                    Probability = values.Length > 3 && int.TryParse(values[3], out var prob) ? prob : null
                });
            }
        }

        return await ImportOpportunitiesAsync(records);
    }

    private async Task<ImportResult> ImportOpportunitiesAsync(List<OpportunityImportDto> records)
    {
        var result = new ImportResult { TotalRecords = records.Count };
        var errors = new List<ImportError>();

        foreach (var (record, index) in records.Select((r, i) => (r, i)))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(record.Name))
                {
                    errors.Add(new ImportError { RowNumber = index + 1, Field = "Name", Message = "Name is required" });
                    continue;
                }

                var opportunity = new Opportunity
                {
                    Name = record.Name,
                    AccountId = record.AccountId ?? 0,
                    Amount = record.Amount ?? 0m,
                    Probability = record.Probability ?? 0,
                    Stage = OpportunityStage.Qualification,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Opportunities.Add(opportunity);
                result.ImportedRecords++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { RowNumber = index + 1, Field = "general", Message = ex.Message });
            }
        }

        await _dbContext.SaveChangesAsync();
        result.FailedRecords = result.TotalRecords - result.ImportedRecords;
        result.Success = result.ImportedRecords > 0;
        result.Errors = errors;
        return result;
    }

    private async Task<ImportResult> ImportProductsFromJsonAsync(string json)
    {
        var records = JsonSerializer.Deserialize<List<ProductImportDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<ProductImportDto>();

        return await ImportProductsAsync(records);
    }

    private async Task<ImportResult> ImportProductsFromCsvAsync(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            return new ImportResult { Success = true, TotalRecords = 0, ImportedRecords = 0 };
        }

        var records = new List<ProductImportDto>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            if (values.Length >= 2)
            {
                records.Add(new ProductImportDto
                {
                    Name = values[0],
                    Sku = values[1],
                    Price = values.Length > 2 && decimal.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0
                });
            }
        }

        return await ImportProductsAsync(records);
    }

    private async Task<ImportResult> ImportProductsAsync(List<ProductImportDto> records)
    {
        var result = new ImportResult { TotalRecords = records.Count };
        var errors = new List<ImportError>();

        foreach (var (record, index) in records.Select((r, i) => (r, i)))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(record.Name))
                {
                    errors.Add(new ImportError { RowNumber = index + 1, Field = "Name", Message = "Name is required" });
                    continue;
                }

                var product = new Product
                {
                    Name = record.Name,
                    SKU = record.Sku ?? string.Empty,
                    Price = record.Price,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Products.Add(product);
                result.ImportedRecords++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { RowNumber = index + 1, Field = "general", Message = ex.Message });
            }
        }

        await _dbContext.SaveChangesAsync();
        result.FailedRecords = result.TotalRecords - result.ImportedRecords;
        result.Success = result.ImportedRecords > 0;
        result.Errors = errors;
        return result;
    }

    #endregion

    #region Helper Methods

    private static LeadSource ParseLeadSource(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return LeadSource.Manual;
        }

        return Enum.TryParse<LeadSource>(source, true, out var result)
            ? result
            : LeadSource.Manual;
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString().Trim());
        return values.ToArray();
    }

    #endregion
}

#region Import DTOs

internal class AccountImportDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Industry { get; set; }
}

internal class ContactImportDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
}

internal class LeadImportDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Source { get; set; }
}

internal class OpportunityImportDto
{
    public string Name { get; set; } = string.Empty;
    public int? AccountId { get; set; }
    public decimal? Amount { get; set; }
    public int? Probability { get; set; }
}

internal class ProductImportDto
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
}

#endregion
