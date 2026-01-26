using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing sample data seeding and clearing.
/// Sample data (Products, Customers, Contacts, Leads, etc.) can be seeded to production
/// and cleared while preserving master data (ZipCodes, ColorPalettes).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SampleDataController : ControllerBase
{
    private readonly SampleDataSeederService _seederService;
    private readonly ILogger<SampleDataController> _logger;

    public SampleDataController(
        SampleDataSeederService seederService,
        ILogger<SampleDataController> logger)
    {
        _seederService = seederService;
        _logger = logger;
    }

    /// <summary>
    /// Get sample data status and statistics
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var isSeeded = await _seederService.IsSampleDataSeededAsync();
            var stats = await _seederService.GetSampleDataStatsAsync();
            
            return Ok(new 
            { 
                isSeeded,
                statistics = new
                {
                    products = stats.ProductCount,
                    serviceRequestCategories = stats.ServiceRequestCategoryCount,
                    serviceRequestSubcategories = stats.ServiceRequestSubcategoryCount,
                    serviceRequestTypes = stats.ServiceRequestTypeCount,
                    customers = stats.CustomerCount,
                    contacts = stats.ContactCount,
                    leads = stats.LeadCount,
                    opportunities = stats.OpportunityCount,
                    sampleUsers = stats.SampleUserCount
                },
                message = isSeeded 
                    ? "Sample data is seeded in the database" 
                    : "No sample data has been seeded yet"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking sample data status");
            return StatusCode(500, new { message = "Error checking sample data status: " + ex.Message });
        }
    }

    /// <summary>
    /// Seed all sample data to the production database
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedSampleData()
    {
        try
        {
            _logger.LogInformation("Starting sample data seeding...");
            await _seederService.SeedAllSampleDataAsync();
            var stats = await _seederService.GetSampleDataStatsAsync();
            
            return Ok(new 
            { 
                message = "Sample data seeded successfully",
                success = true,
                statistics = new
                {
                    products = stats.ProductCount,
                    serviceRequestCategories = stats.ServiceRequestCategoryCount,
                    serviceRequestSubcategories = stats.ServiceRequestSubcategoryCount,
                    serviceRequestTypes = stats.ServiceRequestTypeCount,
                    customers = stats.CustomerCount,
                    contacts = stats.ContactCount,
                    leads = stats.LeadCount,
                    opportunities = stats.OpportunityCount,
                    sampleUsers = stats.SampleUserCount
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding sample data");
            return StatusCode(500, new { message = "Error seeding sample data: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only sample users
    /// </summary>
    [HttpPost("seed/users")]
    public async Task<IActionResult> SeedSampleUsers()
    {
        try
        {
            _logger.LogInformation("Seeding sample users...");
            await _seederService.SeedSampleUsersAsync();
            return Ok(new { message = "Sample users seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding sample users");
            return StatusCode(500, new { message = "Error seeding sample users: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only products and services
    /// </summary>
    [HttpPost("seed/products")]
    public async Task<IActionResult> SeedProducts()
    {
        try
        {
            _logger.LogInformation("Seeding products and services...");
            await _seederService.SeedProductsAsync();
            return Ok(new { message = "Products and services seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding products");
            return StatusCode(500, new { message = "Error seeding products: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only service request categories and types
    /// </summary>
    [HttpPost("seed/servicerequests")]
    public async Task<IActionResult> SeedServiceRequestCategories()
    {
        try
        {
            _logger.LogInformation("Seeding service request categories...");
            await _seederService.SeedServiceRequestCategoriesAsync();
            return Ok(new { message = "Service request categories seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding service request categories");
            return StatusCode(500, new { message = "Error seeding service request categories: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only customers
    /// </summary>
    [HttpPost("seed/customers")]
    public async Task<IActionResult> SeedCustomers()
    {
        try
        {
            _logger.LogInformation("Seeding customers...");
            await _seederService.SeedCustomersAsync();
            return Ok(new { message = "Customers seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding customers");
            return StatusCode(500, new { message = "Error seeding customers: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only contacts
    /// </summary>
    [HttpPost("seed/contacts")]
    public async Task<IActionResult> SeedContacts()
    {
        try
        {
            _logger.LogInformation("Seeding contacts...");
            await _seederService.SeedContactsAsync();
            return Ok(new { message = "Contacts seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding contacts");
            return StatusCode(500, new { message = "Error seeding contacts: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only leads
    /// </summary>
    [HttpPost("seed/leads")]
    public async Task<IActionResult> SeedLeads()
    {
        try
        {
            _logger.LogInformation("Seeding leads...");
            await _seederService.SeedLeadsAsync();
            return Ok(new { message = "Leads seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding leads");
            return StatusCode(500, new { message = "Error seeding leads: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only opportunities
    /// </summary>
    [HttpPost("seed/opportunities")]
    public async Task<IActionResult> SeedOpportunities()
    {
        try
        {
            _logger.LogInformation("Seeding opportunities...");
            await _seederService.SeedOpportunitiesAsync();
            return Ok(new { message = "Opportunities seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding opportunities");
            return StatusCode(500, new { message = "Error seeding opportunities: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Clear all sample data while preserving master data (ZipCodes, ColorPalettes)
    /// </summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearSampleData()
    {
        try
        {
            _logger.LogInformation("Clearing sample data while preserving master data...");
            await _seederService.ClearSampleDataAsync();
            return Ok(new 
            { 
                message = "Sample data cleared successfully. Master data (ZipCodes, ColorPalettes) preserved.", 
                success = true 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing sample data");
            return StatusCode(500, new { message = "Error clearing sample data: " + ex.Message, success = false });
        }
    }
}
