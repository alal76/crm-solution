using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing demo database and data seeding.
/// Demo data is seeded to a completely separate database to protect production data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DemoDataController : ControllerBase
{
    private readonly DemoDataSeederService _seederService;
    private readonly IDemoDbContextFactory _demoDbContextFactory;
    private readonly ILogger<DemoDataController> _logger;

    public DemoDataController(
        DemoDataSeederService seederService, 
        IDemoDbContextFactory demoDbContextFactory,
        ILogger<DemoDataController> logger)
    {
        _seederService = seederService;
        _demoDbContextFactory = demoDbContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get demo database status
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetDemoStatus()
    {
        try
        {
            var isAvailable = await _seederService.IsDemoDbAvailableAsync();
            var connectionString = _demoDbContextFactory.GetDemoConnectionString();
            var isConfigured = !string.IsNullOrEmpty(connectionString);
            
            return Ok(new 
            { 
                isConfigured,
                isAvailable,
                message = isAvailable 
                    ? "Demo database is available and connected" 
                    : (isConfigured 
                        ? "Demo database is configured but not reachable" 
                        : "Demo database is not configured. Set ConnectionStrings:DemoConnection")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking demo database status");
            return StatusCode(500, new { message = "Error checking demo database status: " + ex.Message });
        }
    }

    /// <summary>
    /// Initialize demo database schema
    /// </summary>
    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeDemoDatabase()
    {
        try
        {
            _logger.LogInformation("Initializing demo database...");
            await _seederService.InitializeDemoDbAsync();
            return Ok(new { message = "Demo database initialized successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing demo database");
            return StatusCode(500, new { message = "Error initializing demo database: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed all demo data to the demo database (NOT production)
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedDemoData()
    {
        try
        {
            _logger.LogInformation("Starting demo data seeding to DEMO database...");
            await _seederService.SeedAllDemoDataAsync();
            return Ok(new { message = "Demo data seeded successfully to demo database", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding demo data");
            return StatusCode(500, new { message = "Error seeding demo data: " + ex.Message, success = false });
        }
    }

    /// <summary>
    /// Seed only demo users
    /// </summary>
    [HttpPost("seed/users")]
    public async Task<IActionResult> SeedDemoUsers()
    {
        try
        {
            _logger.LogInformation("Seeding demo users...");
            await _seederService.SeedDemoUsersAsync();
            return Ok(new { message = "Demo users seeded successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding demo users");
            return StatusCode(500, new { message = "Error seeding demo users: " + ex.Message, success = false });
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
    /// Seed only service request categories
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
    /// Clear demo data from demo database
    /// </summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearDemoData()
    {
        try
        {
            _logger.LogInformation("Clearing demo data from demo database...");
            await _seederService.ClearDemoDataAsync();
            return Ok(new { message = "Demo data cleared successfully", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing demo data");
            return StatusCode(500, new { message = "Error clearing demo data: " + ex.Message, success = false });
        }
    }
}
