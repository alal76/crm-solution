// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Admin-only controller for triggering core data seeding operations per ADR-002.
/// All endpoints require the Admin role.
/// </summary>
[ApiController]
[Route("api/admin/seed")]
[Authorize(Roles = "Admin")]
public class AdminSeedController : CrmControllerBase
{
    private readonly ICoreDataSeederService _seeder;
    private readonly ILogger<AdminSeedController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSeedController"/> class.
    /// </summary>
    /// <param name="seeder">The core data seeder service.</param>
    /// <param name="logger">The logger instance.</param>
    public AdminSeedController(ICoreDataSeederService seeder, ILogger<AdminSeedController> logger)
    {
        _seeder = seeder ?? throw new ArgumentNullException(nameof(seeder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs ALL seed methods in order: departments, accounts, products, lookups, contacts,
    /// system settings, module field configurations, additional master data, and ensure lookups.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("core")]
    public async Task<IActionResult> SeedAll(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting full core data seed...");

        await _seeder.SeedDepartmentsAsync(cancellationToken);
        await _seeder.SeedSampleAccountsAsync(cancellationToken);
        await _seeder.SeedSampleProductsAsync(cancellationToken);
        await _seeder.SeedLookupsAsync(cancellationToken);
        await _seeder.SeedSampleContactsAsync(cancellationToken);
        await _seeder.SeedSystemSettingsAsync(cancellationToken);
        await _seeder.SeedModuleFieldConfigurationsAsync(cancellationToken);
        await _seeder.SeedAdditionalMasterDataAsync(cancellationToken);
        await _seeder.SeedEnsureLookupsAsync(cancellationToken);
        await _seeder.SeedWorkflowTriggersAsync(cancellationToken);

        _logger.LogInformation("Full core data seed completed successfully.");
        return Ok(new { message = "All core data seeded successfully." });
    }

    /// <summary>
    /// Seeds departments only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("departments")]
    public async Task<IActionResult> SeedDepartments(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting departments seed...");
        await _seeder.SeedDepartmentsAsync(cancellationToken);
        _logger.LogInformation("Departments seed completed successfully.");
        return Ok(new { message = "Departments seeded successfully." });
    }

    /// <summary>
    /// Seeds sample accounts only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("accounts")]
    public async Task<IActionResult> SeedAccounts(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting sample accounts seed...");
        await _seeder.SeedSampleAccountsAsync(cancellationToken);
        _logger.LogInformation("Sample accounts seed completed successfully.");
        return Ok(new { message = "Sample accounts seeded successfully." });
    }

    /// <summary>
    /// Seeds sample products only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("products")]
    public async Task<IActionResult> SeedProducts(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting sample products seed...");
        await _seeder.SeedSampleProductsAsync(cancellationToken);
        _logger.LogInformation("Sample products seed completed successfully.");
        return Ok(new { message = "Sample products seeded successfully." });
    }

    /// <summary>
    /// Seeds sample contacts only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("contacts")]
    public async Task<IActionResult> SeedContacts(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting sample contacts seed...");
        await _seeder.SeedSampleContactsAsync(cancellationToken);
        _logger.LogInformation("Sample contacts seed completed successfully.");
        return Ok(new { message = "Sample contacts seeded successfully." });
    }

    /// <summary>
    /// Seeds lookups only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("lookups")]
    public async Task<IActionResult> SeedLookups(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting lookups seed...");
        await _seeder.SeedLookupsAsync(cancellationToken);
        _logger.LogInformation("Lookups seed completed successfully.");
        return Ok(new { message = "Lookups seeded successfully." });
    }

    /// <summary>
    /// Seeds system settings only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("settings")]
    public async Task<IActionResult> SeedSettings(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting system settings seed...");
        await _seeder.SeedSystemSettingsAsync(cancellationToken);
        _logger.LogInformation("System settings seed completed successfully.");
        return Ok(new { message = "System settings seeded successfully." });
    }

    /// <summary>
    /// Seeds module field configurations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("module-fields")]
    public async Task<IActionResult> SeedModuleFields(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting module field configurations seed...");
        await _seeder.SeedModuleFieldConfigurationsAsync(cancellationToken);
        _logger.LogInformation("Module field configurations seed completed successfully.");
        return Ok(new { message = "Module field configurations seeded successfully." });
    }

    /// <summary>
    /// Seeds workflow triggers only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("workflow-triggers")]
    public async Task<IActionResult> SeedWorkflowTriggers(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting workflow triggers seed...");
        await _seeder.SeedWorkflowTriggersAsync(cancellationToken);
        _logger.LogInformation("Workflow triggers seed completed successfully.");
        return Ok(new { message = "Workflow triggers seeded successfully." });
    }

    /// <summary>
    /// Force reseeds module field configurations, clearing existing data first.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message or a 500 error on failure.</returns>
    [HttpPost("module-fields/force")]
    public async Task<IActionResult> ForceReseedModuleFields(CancellationToken cancellationToken)
    {
                _logger.LogInformation("Starting force reseed of module field configurations...");
        await _seeder.ForceReseedModuleFieldConfigurationsAsync(cancellationToken);
        _logger.LogInformation("Force reseed of module field configurations completed successfully.");
        return Ok(new { message = "Module field configurations force-reseeded successfully." });
    }
}
