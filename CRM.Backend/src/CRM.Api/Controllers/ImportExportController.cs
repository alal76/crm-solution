using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using System.Text;
using System.Text.Json;

namespace CRM.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ImportExportController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<ImportExportController> _logger;

    public ImportExportController(CrmDbContext context, ILogger<ImportExportController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get available entity types for import/export
    /// </summary>
    [HttpGet("entity-types")]
    public ActionResult<List<object>> GetEntityTypes()
    {
        var entityTypes = new List<object>
        {
            new { name = "contacts", label = "Contacts", canImport = true, canExport = true },
            new { name = "customers", label = "Customers", canImport = true, canExport = true },
            new { name = "opportunities", label = "Opportunities", canImport = true, canExport = true },
            new { name = "products", label = "Products", canImport = true, canExport = true },
            new { name = "quotes", label = "Quotes", canImport = true, canExport = true },
            new { name = "tasks", label = "Tasks", canImport = true, canExport = true },
            new { name = "notes", label = "Notes", canImport = true, canExport = true },
            new { name = "activities", label = "Activities", canImport = true, canExport = true },
            new { name = "campaigns", label = "Campaigns", canImport = true, canExport = true },
            new { name = "service-requests", label = "Service Requests", canImport = true, canExport = true },
            new { name = "accounts", label = "Accounts", canImport = true, canExport = true }
        };

        return Ok(entityTypes);
    }

    /// <summary>
    /// Export entity data as JSON
    /// </summary>
    [HttpGet("export/{entityType}")]
    public async Task<IActionResult> ExportData(string entityType, [FromQuery] string format = "json")
    {
        try
        {
            object? data = entityType.ToLowerInvariant() switch
            {
                "contacts" => await _context.Contacts.Include(c => c.SocialMediaLinks).ToListAsync(),
                "customers" => await _context.Customers.ToListAsync(),
                "opportunities" => await _context.Opportunities.ToListAsync(),
                "products" => await _context.Products.ToListAsync(),
                "quotes" => await _context.Quotes.Include(q => q.Items).ToListAsync(),
                "tasks" => await _context.Tasks.ToListAsync(),
                "notes" => await _context.Notes.ToListAsync(),
                "activities" => await _context.Activities.ToListAsync(),
                "campaigns" => await _context.Campaigns.ToListAsync(),
                "service-requests" => await _context.ServiceRequests.ToListAsync(),
                "accounts" => await _context.Accounts.ToListAsync(),
                _ => null
            };

            if (data == null)
            {
                return BadRequest(new { message = $"Unknown entity type: {entityType}" });
            }

            if (format.ToLowerInvariant() == "csv")
            {
                var csv = ConvertToCsv(data);
                var csvBytes = Encoding.UTF8.GetBytes(csv);
                return File(csvBytes, "text/csv", $"{entityType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(data, options);
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                return File(jsonBytes, "application/json", $"{entityType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {EntityType}", entityType);
            return StatusCode(500, new { message = $"Error exporting data: {ex.Message}" });
        }
    }

    /// <summary>
    /// Import entity data from JSON or CSV
    /// </summary>
    [HttpPost("import/{entityType}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportData(string entityType, IFormFile file, [FromQuery] bool skipDuplicates = true)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isJson = fileExtension == ".json" || file.ContentType == "application/json";
            var isCsv = fileExtension == ".csv" || file.ContentType == "text/csv";

            if (!isJson && !isCsv)
            {
                return BadRequest(new { message = "Only JSON and CSV files are supported" });
            }

            ImportResult result;
            if (isJson)
            {
                result = await ImportFromJson(entityType, content, skipDuplicates);
            }
            else
            {
                result = await ImportFromCsv(entityType, content, skipDuplicates);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing {EntityType}", entityType);
            return StatusCode(500, new { message = $"Error importing data: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get import template for an entity type
    /// </summary>
    [HttpGet("template/{entityType}")]
    public IActionResult GetImportTemplate(string entityType, [FromQuery] string format = "json")
    {
        try
        {
            var template = GetTemplateForEntity(entityType);
            if (template == null)
            {
                return BadRequest(new { message = $"Unknown entity type: {entityType}" });
            }

            if (format.ToLowerInvariant() == "csv")
            {
                var csv = ConvertToCsv(new List<object> { template });
                var csvBytes = Encoding.UTF8.GetBytes(csv);
                return File(csvBytes, "text/csv", $"{entityType}_template.csv");
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(new List<object> { template }, options);
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                return File(jsonBytes, "application/json", $"{entityType}_template.json");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating template for {EntityType}", entityType);
            return StatusCode(500, new { message = $"Error generating template: {ex.Message}" });
        }
    }

    private object? GetTemplateForEntity(string entityType)
    {
        return entityType.ToLowerInvariant() switch
        {
            "contacts" => new
            {
                contactType = "Customer",
                firstName = "John",
                lastName = "Doe",
                middleName = "",
                emailPrimary = "john.doe@example.com",
                emailSecondary = "",
                phonePrimary = "+1-555-0123",
                phoneSecondary = "",
                address = "123 Main St",
                city = "New York",
                state = "NY",
                country = "USA",
                zipCode = "10001",
                jobTitle = "Manager",
                department = "Sales",
                company = "Acme Inc",
                reportsTo = "",
                notes = "Sample contact",
                dateOfBirth = "1990-01-15"
            },
            "customers" => new
            {
                customerType = "Business",
                businessName = "Acme Corporation",
                firstName = "Jane",
                lastName = "Smith",
                emailPrimary = "jane@acme.com",
                phonePrimary = "+1-555-0456",
                address = "456 Business Ave",
                city = "Los Angeles",
                state = "CA",
                country = "USA",
                zipCode = "90001",
                preferredContactMethod = "Email",
                status = "Active",
                creditLimit = 50000,
                industry = "Technology",
                annualRevenue = 1000000
            },
            "opportunities" => new
            {
                title = "New Business Opportunity",
                amount = 25000.00,
                stage = "Proposal",
                probability = 50,
                expectedCloseDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
                source = "Website",
                customerId = 1,
                description = "Sample opportunity description",
                status = "Open"
            },
            "products" => new
            {
                name = "Sample Product",
                sku = "PROD-001",
                description = "A sample product",
                category = "Electronics",
                unitPrice = 99.99,
                cost = 50.00,
                quantity = 100,
                reorderLevel = 10,
                isActive = true
            },
            "quotes" => new
            {
                quoteNumber = "QT-001",
                customerId = 1,
                opportunityId = 1,
                status = "Draft",
                validUntil = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
                subtotal = 1000.00,
                discountPercent = 10,
                taxPercent = 8.5,
                total = 985.50,
                notes = "Sample quote"
            },
            "tasks" => new
            {
                title = "Sample Task",
                description = "Task description",
                status = "Not Started",
                priority = "Medium",
                dueDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
                assignedTo = "",
                relatedEntityType = "Customer",
                relatedEntityId = 1
            },
            "notes" => new
            {
                title = "Sample Note",
                content = "Note content goes here",
                entityType = "Customer",
                entityId = 1,
                isPinned = false
            },
            "activities" => new
            {
                type = "Call",
                subject = "Follow-up call",
                description = "Discussed project requirements",
                activityDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                duration = 30,
                entityType = "Customer",
                entityId = 1,
                status = "Completed"
            },
            "campaigns" => new
            {
                name = "Summer Sale Campaign",
                type = "Email",
                status = "Planning",
                startDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                endDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
                budget = 5000.00,
                description = "Summer promotional campaign",
                targetAudience = "Existing customers"
            },
            "service-requests" => new
            {
                title = "Technical Support Request",
                description = "Customer needs help with product setup",
                priority = "Medium",
                status = "Open",
                customerId = 1,
                category = "Technical Support",
                channel = "Email"
            },
            "accounts" => new
            {
                name = "Sample Account",
                type = "Customer",
                industry = "Technology",
                website = "https://example.com",
                phone = "+1-555-0789",
                address = "789 Account St",
                city = "Chicago",
                state = "IL",
                country = "USA",
                zipCode = "60601",
                description = "Sample account description",
                annualRevenue = 500000,
                employees = 50
            },
            _ => null
        };
    }

    private async Task<ImportResult> ImportFromJson(string entityType, string jsonContent, bool skipDuplicates)
    {
        var result = new ImportResult { EntityType = entityType };

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            switch (entityType.ToLowerInvariant())
            {
                case "contacts":
                    var contacts = JsonSerializer.Deserialize<List<ContactImportDto>>(jsonContent, options);
                    if (contacts != null)
                    {
                        foreach (var dto in contacts)
                        {
                            try
                            {
                                if (skipDuplicates && !string.IsNullOrEmpty(dto.EmailPrimary))
                                {
                                    var exists = await _context.Contacts.AnyAsync(c => c.EmailPrimary == dto.EmailPrimary);
                                    if (exists)
                                    {
                                        result.Skipped++;
                                        continue;
                                    }
                                }

                                var contact = new CRM.Core.Entities.Contact
                                {
                                    ContactType = dto.ContactType ?? "Other",
                                    FirstName = dto.FirstName ?? "",
                                    LastName = dto.LastName ?? "",
                                    MiddleName = dto.MiddleName,
                                    EmailPrimary = dto.EmailPrimary,
                                    EmailSecondary = dto.EmailSecondary,
                                    PhonePrimary = dto.PhonePrimary,
                                    PhoneSecondary = dto.PhoneSecondary,
                                    Address = dto.Address,
                                    City = dto.City,
                                    State = dto.State,
                                    Country = dto.Country,
                                    ZipCode = dto.ZipCode,
                                    JobTitle = dto.JobTitle,
                                    Department = dto.Department,
                                    Company = dto.Company,
                                    ReportsTo = dto.ReportsTo,
                                    Notes = dto.Notes,
                                    DateOfBirth = dto.DateOfBirth != null ? DateOnly.Parse(dto.DateOfBirth) : null,
                                    DateAdded = DateTime.UtcNow
                                };

                                _context.Contacts.Add(contact);
                                result.Imported++;
                            }
                            catch (Exception ex)
                            {
                                result.Failed++;
                                result.Errors.Add($"Row {result.Total + 1}: {ex.Message}");
                            }
                            result.Total++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "customers":
                    var customers = JsonSerializer.Deserialize<List<CustomerImportDto>>(jsonContent, options);
                    if (customers != null)
                    {
                        foreach (var dto in customers)
                        {
                            try
                            {
                                if (skipDuplicates && !string.IsNullOrEmpty(dto.EmailPrimary))
                                {
                                    var exists = await _context.Customers.AnyAsync(c => c.EmailPrimary == dto.EmailPrimary);
                                    if (exists)
                                    {
                                        result.Skipped++;
                                        continue;
                                    }
                                }

                                var customer = new CRM.Core.Entities.Customer
                                {
                                    CustomerType = dto.CustomerType ?? "Individual",
                                    BusinessName = dto.BusinessName,
                                    FirstName = dto.FirstName,
                                    LastName = dto.LastName,
                                    EmailPrimary = dto.EmailPrimary,
                                    PhonePrimary = dto.PhonePrimary,
                                    Address = dto.Address,
                                    City = dto.City,
                                    State = dto.State,
                                    Country = dto.Country,
                                    ZipCode = dto.ZipCode,
                                    PreferredContactMethod = dto.PreferredContactMethod,
                                    Status = dto.Status ?? "Active",
                                    CreditLimit = dto.CreditLimit,
                                    Industry = dto.Industry,
                                    AnnualRevenue = dto.AnnualRevenue,
                                    DateAdded = DateTime.UtcNow
                                };

                                _context.Customers.Add(customer);
                                result.Imported++;
                            }
                            catch (Exception ex)
                            {
                                result.Failed++;
                                result.Errors.Add($"Row {result.Total + 1}: {ex.Message}");
                            }
                            result.Total++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "products":
                    var products = JsonSerializer.Deserialize<List<ProductImportDto>>(jsonContent, options);
                    if (products != null)
                    {
                        foreach (var dto in products)
                        {
                            try
                            {
                                if (skipDuplicates && !string.IsNullOrEmpty(dto.Sku))
                                {
                                    var exists = await _context.Products.AnyAsync(p => p.Sku == dto.Sku);
                                    if (exists)
                                    {
                                        result.Skipped++;
                                        continue;
                                    }
                                }

                                var product = new CRM.Core.Entities.Product
                                {
                                    Name = dto.Name ?? "",
                                    Sku = dto.Sku,
                                    Description = dto.Description,
                                    Category = dto.Category,
                                    UnitPrice = dto.UnitPrice ?? 0,
                                    Cost = dto.Cost,
                                    Quantity = dto.Quantity ?? 0,
                                    ReorderLevel = dto.ReorderLevel ?? 0,
                                    IsActive = dto.IsActive ?? true,
                                    DateAdded = DateTime.UtcNow
                                };

                                _context.Products.Add(product);
                                result.Imported++;
                            }
                            catch (Exception ex)
                            {
                                result.Failed++;
                                result.Errors.Add($"Row {result.Total + 1}: {ex.Message}");
                            }
                            result.Total++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "tasks":
                    var tasks = JsonSerializer.Deserialize<List<TaskImportDto>>(jsonContent, options);
                    if (tasks != null)
                    {
                        foreach (var dto in tasks)
                        {
                            try
                            {
                                var task = new CRM.Core.Entities.TaskItem
                                {
                                    Title = dto.Title ?? "",
                                    Description = dto.Description,
                                    Status = dto.Status ?? "Not Started",
                                    Priority = dto.Priority ?? "Medium",
                                    DueDate = dto.DueDate != null ? DateTime.Parse(dto.DueDate) : null,
                                    AssignedTo = dto.AssignedTo,
                                    RelatedEntityType = dto.RelatedEntityType,
                                    RelatedEntityId = dto.RelatedEntityId,
                                    CreatedDate = DateTime.UtcNow
                                };

                                _context.Tasks.Add(task);
                                result.Imported++;
                            }
                            catch (Exception ex)
                            {
                                result.Failed++;
                                result.Errors.Add($"Row {result.Total + 1}: {ex.Message}");
                            }
                            result.Total++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "notes":
                    var notes = JsonSerializer.Deserialize<List<NoteImportDto>>(jsonContent, options);
                    if (notes != null)
                    {
                        foreach (var dto in notes)
                        {
                            try
                            {
                                var note = new CRM.Core.Entities.Note
                                {
                                    Title = dto.Title ?? "Untitled",
                                    Content = dto.Content ?? "",
                                    EntityType = dto.EntityType,
                                    EntityId = dto.EntityId,
                                    IsPinned = dto.IsPinned ?? false,
                                    CreatedDate = DateTime.UtcNow
                                };

                                _context.Notes.Add(note);
                                result.Imported++;
                            }
                            catch (Exception ex)
                            {
                                result.Failed++;
                                result.Errors.Add($"Row {result.Total + 1}: {ex.Message}");
                            }
                            result.Total++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "activities":
                    var activities = JsonSerializer.Deserialize<List<ActivityImportDto>>(jsonContent, options);
                    if (activities != null)
                    {
                        foreach (var dto in activities)
                        {
                            try
                            {
                                var activity = new CRM.Core.Entities.Activity
                                {
                                    Type = dto.Type ?? "Other",
                                    Subject = dto.Subject ?? "",
                                    Description = dto.Description,
                                    ActivityDate = dto.ActivityDate != null ? DateTime.Parse(dto.ActivityDate) : DateTime.UtcNow,
                                    Duration = dto.Duration,
                                    EntityType = dto.EntityType,
                                    EntityId = dto.EntityId,
                                    Status = dto.Status ?? "Completed",
                                    CreatedDate = DateTime.UtcNow
                                };

                                _context.Activities.Add(activity);
                                result.Imported++;
                            }
                            catch (Exception ex)
                            {
                                result.Failed++;
                                result.Errors.Add($"Row {result.Total + 1}: {ex.Message}");
                            }
                            result.Total++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;

                default:
                    result.Errors.Add($"Import not yet implemented for entity type: {entityType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse JSON: {ex.Message}");
        }

        return result;
    }

    private async Task<ImportResult> ImportFromCsv(string entityType, string csvContent, bool skipDuplicates)
    {
        var result = new ImportResult { EntityType = entityType };

        try
        {
            var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                result.Errors.Add("CSV file must have a header row and at least one data row");
                return result;
            }

            var headers = ParseCsvLine(lines[0]);
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                headerMap[headers[i].Trim()] = i;
            }

            for (int rowIndex = 1; rowIndex < lines.Length; rowIndex++)
            {
                var values = ParseCsvLine(lines[rowIndex]);
                result.Total++;

                try
                {
                    await ImportCsvRow(entityType, headerMap, values, skipDuplicates, result);
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Row {rowIndex + 1}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse CSV: {ex.Message}");
        }

        return result;
    }

    private async Task ImportCsvRow(string entityType, Dictionary<string, int> headerMap, List<string> values, bool skipDuplicates, ImportResult result)
    {
        string GetValue(string column) => headerMap.TryGetValue(column, out var idx) && idx < values.Count ? values[idx].Trim() : "";

        switch (entityType.ToLowerInvariant())
        {
            case "contacts":
                var email = GetValue("emailPrimary") ?? GetValue("email");
                if (skipDuplicates && !string.IsNullOrEmpty(email))
                {
                    var exists = await _context.Contacts.AnyAsync(c => c.EmailPrimary == email);
                    if (exists)
                    {
                        result.Skipped++;
                        return;
                    }
                }

                var contact = new CRM.Core.Entities.Contact
                {
                    ContactType = GetValue("contactType") ?? "Other",
                    FirstName = GetValue("firstName") ?? "",
                    LastName = GetValue("lastName") ?? "",
                    MiddleName = GetValue("middleName"),
                    EmailPrimary = email,
                    EmailSecondary = GetValue("emailSecondary"),
                    PhonePrimary = GetValue("phonePrimary") ?? GetValue("phone"),
                    PhoneSecondary = GetValue("phoneSecondary"),
                    Address = GetValue("address"),
                    City = GetValue("city"),
                    State = GetValue("state"),
                    Country = GetValue("country"),
                    ZipCode = GetValue("zipCode"),
                    JobTitle = GetValue("jobTitle"),
                    Department = GetValue("department"),
                    Company = GetValue("company"),
                    ReportsTo = GetValue("reportsTo"),
                    Notes = GetValue("notes"),
                    DateAdded = DateTime.UtcNow
                };
                _context.Contacts.Add(contact);
                result.Imported++;
                break;

            case "customers":
                var custEmail = GetValue("emailPrimary") ?? GetValue("email");
                if (skipDuplicates && !string.IsNullOrEmpty(custEmail))
                {
                    var exists = await _context.Customers.AnyAsync(c => c.EmailPrimary == custEmail);
                    if (exists)
                    {
                        result.Skipped++;
                        return;
                    }
                }

                var customer = new CRM.Core.Entities.Customer
                {
                    CustomerType = GetValue("customerType") ?? "Individual",
                    BusinessName = GetValue("businessName"),
                    FirstName = GetValue("firstName"),
                    LastName = GetValue("lastName"),
                    EmailPrimary = custEmail,
                    PhonePrimary = GetValue("phonePrimary") ?? GetValue("phone"),
                    Address = GetValue("address"),
                    City = GetValue("city"),
                    State = GetValue("state"),
                    Country = GetValue("country"),
                    ZipCode = GetValue("zipCode"),
                    Status = GetValue("status") ?? "Active",
                    Industry = GetValue("industry"),
                    DateAdded = DateTime.UtcNow
                };
                _context.Customers.Add(customer);
                result.Imported++;
                break;

            case "products":
                var sku = GetValue("sku");
                if (skipDuplicates && !string.IsNullOrEmpty(sku))
                {
                    var exists = await _context.Products.AnyAsync(p => p.Sku == sku);
                    if (exists)
                    {
                        result.Skipped++;
                        return;
                    }
                }

                var product = new CRM.Core.Entities.Product
                {
                    Name = GetValue("name") ?? "",
                    Sku = sku,
                    Description = GetValue("description"),
                    Category = GetValue("category"),
                    UnitPrice = decimal.TryParse(GetValue("unitPrice") ?? GetValue("price"), out var price) ? price : 0,
                    Cost = decimal.TryParse(GetValue("cost"), out var cost) ? cost : null,
                    Quantity = int.TryParse(GetValue("quantity"), out var qty) ? qty : 0,
                    ReorderLevel = int.TryParse(GetValue("reorderLevel"), out var reorder) ? reorder : 0,
                    IsActive = GetValue("isActive")?.ToLowerInvariant() != "false",
                    DateAdded = DateTime.UtcNow
                };
                _context.Products.Add(product);
                result.Imported++;
                break;

            case "tasks":
                var task = new CRM.Core.Entities.TaskItem
                {
                    Title = GetValue("title") ?? "",
                    Description = GetValue("description"),
                    Status = GetValue("status") ?? "Not Started",
                    Priority = GetValue("priority") ?? "Medium",
                    AssignedTo = GetValue("assignedTo"),
                    RelatedEntityType = GetValue("relatedEntityType"),
                    CreatedDate = DateTime.UtcNow
                };
                _context.Tasks.Add(task);
                result.Imported++;
                break;

            default:
                result.Errors.Add($"CSV import not yet implemented for entity type: {entityType}");
                break;
        }
    }

    private List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }

    private string ConvertToCsv(object data)
    {
        var sb = new StringBuilder();
        var list = data as System.Collections.IEnumerable;
        if (list == null) return "";

        bool headerWritten = false;

        foreach (var item in list)
        {
            var type = item.GetType();
            var properties = type.GetProperties()
                .Where(p => p.PropertyType.IsPrimitive || 
                           p.PropertyType == typeof(string) ||
                           p.PropertyType == typeof(decimal) ||
                           p.PropertyType == typeof(decimal?) ||
                           p.PropertyType == typeof(DateTime) ||
                           p.PropertyType == typeof(DateTime?) ||
                           p.PropertyType == typeof(DateOnly) ||
                           p.PropertyType == typeof(DateOnly?) ||
                           p.PropertyType == typeof(int?) ||
                           p.PropertyType == typeof(bool?))
                .ToList();

            if (!headerWritten)
            {
                sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvValue(p.Name))));
                headerWritten = true;
            }

            var values = properties.Select(p => 
            {
                var value = p.GetValue(item);
                return EscapeCsvValue(value?.ToString() ?? "");
            });
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private string EscapeCsvValue(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}

// DTOs for import
public class ImportResult
{
    public string EntityType { get; set; } = "";
    public int Total { get; set; }
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class ContactImportDto
{
    public string? ContactType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }
    public string? Notes { get; set; }
    public string? DateOfBirth { get; set; }
}

public class CustomerImportDto
{
    public string? CustomerType { get; set; }
    public string? BusinessName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailPrimary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? Status { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Industry { get; set; }
    public decimal? AnnualRevenue { get; set; }
}

public class ProductImportDto
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public int? Quantity { get; set; }
    public int? ReorderLevel { get; set; }
    public bool? IsActive { get; set; }
}

public class TaskImportDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
}

public class NoteImportDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool? IsPinned { get; set; }
}

public class ActivityImportDto
{
    public string? Type { get; set; }
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public string? ActivityDate { get; set; }
    public int? Duration { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Status { get; set; }
}
