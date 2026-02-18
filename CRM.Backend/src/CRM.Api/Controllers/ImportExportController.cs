// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Infrastructure.Data;
using CRM.Core.Entities;
using CRM.Core.Models;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<object>> GetEntityTypes()
    {
        var entityTypes = new List<object>
        {
            new { name = "contacts", label = "Contacts", canImport = true, canExport = true },
            new { name = "accounts", label = "Accounts", canImport = true, canExport = true },
            new { name = "opportunities", label = "Opportunities", canImport = true, canExport = true },
            new { name = "products", label = "Products", canImport = true, canExport = true },
            new { name = "quotes", label = "Quotes", canImport = false, canExport = true },
            new { name = "tasks", label = "Tasks", canImport = true, canExport = true },
            new { name = "notes", label = "Notes", canImport = false, canExport = true },
            new { name = "activities", label = "Activities", canImport = false, canExport = true },
            new { name = "service-requests", label = "Service Requests", canImport = false, canExport = true },
            new { name = "leads", label = "Leads", canImport = true, canExport = true }
        };

        return Ok(entityTypes);
    }

    /// <summary>
    /// Export entity data as JSON or CSV
    /// </summary>
    [HttpGet("export/{entityType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportData(string entityType, [FromQuery] string format = "json")
    {
        try
        {
            object? data = entityType.ToLowerInvariant() switch
            {
                "contacts" => await _context.Contacts.Include(c => c.SocialMediaLinks).ToListAsync(),
                "accounts" => await _context.Accounts.ToListAsync(),
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Import entity data from a JSON file.
    /// </summary>
    [HttpPost("import/{entityType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportData(string entityType, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded or file is empty." });
        }

        var supportedTypes = new HashSet<string> { "contacts", "accounts", "opportunities", "products", "tasks", "leads" };
        var normalizedType = entityType.ToLowerInvariant();

        if (!supportedTypes.Contains(normalizedType))
        {
            return BadRequest(new { message = $"Import is not supported for entity type: {entityType}" });
        }

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            int importedCount = 0;
            var errors = new List<object>();

            switch (normalizedType)
            {
                case "contacts":
                {
                    var items = JsonSerializer.Deserialize<List<Contact>>(content, jsonOptions);
                    if (items == null || items.Count == 0)
                        return BadRequest(new { message = "File contains no valid records." });

                    foreach (var item in items)
                    {
                        try
                        {
                            item.Id = 0;
                            _context.Contacts.Add(item);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new { row = importedCount + errors.Count + 1, message = ex.Message });
                        }
                    }
                    break;
                }
                case "accounts":
                {
                    var items = JsonSerializer.Deserialize<List<Account>>(content, jsonOptions);
                    if (items == null || items.Count == 0)
                        return BadRequest(new { message = "File contains no valid records." });

                    foreach (var item in items)
                    {
                        try
                        {
                            item.Id = 0;
                            item.CreatedAt = DateTime.UtcNow;
                            item.UpdatedAt = DateTime.UtcNow;
                            _context.Accounts.Add(item);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new { row = importedCount + errors.Count + 1, message = ex.Message });
                        }
                    }
                    break;
                }
                case "opportunities":
                {
                    var items = JsonSerializer.Deserialize<List<Opportunity>>(content, jsonOptions);
                    if (items == null || items.Count == 0)
                        return BadRequest(new { message = "File contains no valid records." });

                    foreach (var item in items)
                    {
                        try
                        {
                            item.Id = 0;
                            item.CreatedAt = DateTime.UtcNow;
                            item.UpdatedAt = DateTime.UtcNow;
                            _context.Opportunities.Add(item);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new { row = importedCount + errors.Count + 1, message = ex.Message });
                        }
                    }
                    break;
                }
                case "products":
                {
                    var items = JsonSerializer.Deserialize<List<Product>>(content, jsonOptions);
                    if (items == null || items.Count == 0)
                        return BadRequest(new { message = "File contains no valid records." });

                    foreach (var item in items)
                    {
                        try
                        {
                            item.Id = 0;
                            item.CreatedAt = DateTime.UtcNow;
                            item.UpdatedAt = DateTime.UtcNow;
                            _context.Products.Add(item);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new { row = importedCount + errors.Count + 1, message = ex.Message });
                        }
                    }
                    break;
                }
                case "tasks":
                {
                    var items = JsonSerializer.Deserialize<List<CrmTask>>(content, jsonOptions);
                    if (items == null || items.Count == 0)
                        return BadRequest(new { message = "File contains no valid records." });

                    foreach (var item in items)
                    {
                        try
                        {
                            item.Id = 0;
                            item.CreatedAt = DateTime.UtcNow;
                            item.UpdatedAt = DateTime.UtcNow;
                            _context.CrmTasks.Add(item);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new { row = importedCount + errors.Count + 1, message = ex.Message });
                        }
                    }
                    break;
                }
                case "leads":
                {
                    var items = JsonSerializer.Deserialize<List<Lead>>(content, jsonOptions);
                    if (items == null || items.Count == 0)
                        return BadRequest(new { message = "File contains no valid records." });

                    foreach (var item in items)
                    {
                        try
                        {
                            item.Id = 0;
                            item.CreatedAt = DateTime.UtcNow;
                            item.UpdatedAt = DateTime.UtcNow;
                            _context.Leads.Add(item);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new { row = importedCount + errors.Count + 1, message = ex.Message });
                        }
                    }
                    break;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Imported {Count} {EntityType} records ({ErrorCount} errors)", importedCount, entityType, errors.Count);

            return Ok(new
            {
                message = $"Successfully imported {importedCount} {entityType} record(s).",
                importedCount,
                errorCount = errors.Count,
                errors = errors.Count > 0 ? errors : null
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in import file for {EntityType}", entityType);
            return BadRequest(new { message = "Invalid JSON format. Please ensure the file contains a valid JSON array.", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing {EntityType}", entityType);
            return StatusCode(500, new { message = $"Error importing data: {ex.Message}" });
        }
    }

    private object? GetTemplateForEntity(string entityType)
    {
        return entityType.ToLowerInvariant() switch
        {
            "contacts" => new
            {
                contactType = "Account",
                firstName = "John",
                lastName = "Doe",
                email = "john.doe@example.com",
                phone = "+1-555-0123",
                company = "Acme Inc",
                jobTitle = "Manager"
            },
            "accounts" => new
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
