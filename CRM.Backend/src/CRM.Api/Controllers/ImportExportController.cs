using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Infrastructure.Data;
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
            new { name = "contacts", label = "Contacts", canImport = false, canExport = true },
            new { name = "customers", label = "Customers", canImport = false, canExport = true },
            new { name = "opportunities", label = "Opportunities", canImport = false, canExport = true },
            new { name = "products", label = "Products", canImport = false, canExport = true },
            new { name = "quotes", label = "Quotes", canImport = false, canExport = true },
            new { name = "tasks", label = "Tasks", canImport = false, canExport = true },
            new { name = "notes", label = "Notes", canImport = false, canExport = true },
            new { name = "activities", label = "Activities", canImport = false, canExport = true },
            new { name = "service-requests", label = "Service Requests", canImport = false, canExport = true },
            new { name = "leads", label = "Leads", canImport = false, canExport = true }
        };

        return Ok(entityTypes);
    }

    /// <summary>
    /// Export entity data as JSON or CSV
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
                "quotes" => await _context.Quotes.ToListAsync(),
                "tasks" => await _context.CrmTasks.ToListAsync(),
                "notes" => await _context.Notes.ToListAsync(),
                "activities" => await _context.Activities.ToListAsync(),
                "service-requests" => await _context.ServiceRequests.ToListAsync(),
                "leads" => await _context.Leads.ToListAsync(),
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
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
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
                email = "john.doe@example.com",
                phone = "+1-555-0123",
                company = "Acme Inc",
                jobTitle = "Manager"
            },
            "customers" => new
            {
                category = "Individual",
                firstName = "Jane",
                lastName = "Smith",
                company = "Acme Corporation",
                email = "jane@acme.com",
                phone = "+1-555-0456"
            },
            "opportunities" => new
            {
                name = "New Business Opportunity",
                estimatedValue = 25000.00,
                stage = "Proposal",
                probability = 50,
                expectedCloseDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")
            },
            "products" => new
            {
                name = "Sample Product",
                description = "A sample product",
                price = 99.99,
                category = "Electronics"
            },
            "tasks" => new
            {
                subject = "Sample Task",
                description = "Task description",
                status = "NotStarted",
                priority = "Medium",
                dueDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd")
            },
            "notes" => new
            {
                title = "Sample Note",
                content = "Note content goes here"
            },
            _ => null
        };
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
                           p.PropertyType == typeof(bool?) ||
                           p.PropertyType.IsEnum)
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
