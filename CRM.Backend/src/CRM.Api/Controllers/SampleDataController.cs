// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing sample data seeding and clearing.
/// Sample data (Products, Accounts, Contacts, Leads, etc.) can be seeded to production
/// and cleared while preserving master data (ZipCodes, ColorPalettes).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SampleDataController : CrmControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
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
                accounts = stats.AccountCount,
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

    /// <summary>
    /// Seed all sample data to the production database
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedSampleData()
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
                accounts = stats.AccountCount,
                contacts = stats.ContactCount,
                leads = stats.LeadCount,
                opportunities = stats.OpportunityCount,
                sampleUsers = stats.SampleUserCount
            }
        });
    }

    /// <summary>
    /// Seed only sample users
    /// </summary>
    [HttpPost("seed/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedSampleUsers()
    {
                _logger.LogInformation("Seeding sample users...");
        await _seederService.SeedSampleUsersAsync();
        return Ok(new { message = "Sample users seeded successfully", success = true });
    }

    /// <summary>
    /// Seed only products and services
    /// </summary>
    [HttpPost("seed/products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedProducts()
    {
                _logger.LogInformation("Seeding products and services...");
        await _seederService.SeedProductsAsync();
        return Ok(new { message = "Products and services seeded successfully", success = true });
    }

    /// <summary>
    /// Seed only service request categories and types
    /// </summary>
    [HttpPost("seed/servicerequests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedServiceRequestCategories()
    {
                _logger.LogInformation("Seeding service request categories...");
        await _seederService.SeedServiceRequestCategoriesAsync();
        return Ok(new { message = "Service request categories seeded successfully", success = true });
    }

    /// <summary>
    /// Seed only accounts
    /// </summary>
    [HttpPost("seed/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedAccounts()
    {
                _logger.LogInformation("Seeding accounts...");
        await _seederService.SeedAccountsAsync();
        return Ok(new { message = "Accounts seeded successfully", success = true });
    }

    /// <summary>
    /// Seed only contacts
    /// </summary>
    [HttpPost("seed/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedContacts()
    {
                _logger.LogInformation("Seeding contacts...");
        await _seederService.SeedContactsAsync();
        return Ok(new { message = "Contacts seeded successfully", success = true });
    }

    /// <summary>
    /// Seed only leads
    /// </summary>
    [HttpPost("seed/leads")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedLeads()
    {
                _logger.LogInformation("Seeding leads...");
        await _seederService.SeedLeadsAsync();
        return Ok(new { message = "Leads seeded successfully", success = true });
    }

    /// <summary>
    /// Seed only opportunities
    /// </summary>
    [HttpPost("seed/opportunities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedOpportunities()
    {
                _logger.LogInformation("Seeding opportunities...");
        await _seederService.SeedOpportunitiesAsync();
        return Ok(new { message = "Opportunities seeded successfully", success = true });
    }

    /// <summary>
    /// Clear all sample data while preserving master data (ZipCodes, ColorPalettes)
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearSampleData()
    {
                _logger.LogInformation("Clearing sample data while preserving master data...");
        await _seederService.ClearSampleDataAsync();
        return Ok(new
        {
            message = "Sample data cleared successfully. Master data (ZipCodes, ColorPalettes) preserved.",
            success = true
        });
    }
}
