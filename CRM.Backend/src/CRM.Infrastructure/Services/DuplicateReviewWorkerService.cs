// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Resolve ambiguity between CRM.Core.Entities.MatchType and System.IO.MatchType
using MatchType = CRM.Core.Entities.MatchType;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that periodically reviews pending duplicate candidates
/// and assigns unassigned ones to the account manager (or first Admin user) for resolution.
///
/// Configuration (appsettings.json):
/// <code>
///   "DuplicateDetection": {
///       "EnableReviewWorker": true,
///       "ReviewIntervalMinutes": 30,
///       "DefaultAssigneeRole": "Admin"
///   }
/// </code>
///
/// Behavior:
///   1. Queries DuplicateCandidates with Status = Pending and no AssignedToUserId.
///   2. Finds the default assignee (configurable role, default Admin).
///   3. Sets AssignedToUserId and AssignedAt on each unassigned candidate.
///   4. Logs a summary of how many candidates were assigned.
/// </summary>
public class DuplicateReviewWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DuplicateReviewWorkerService> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly bool _isEnabled;
    private readonly string _defaultAssigneeRole;

    public DuplicateReviewWorkerService(
        IServiceProvider serviceProvider,
        ILogger<DuplicateReviewWorkerService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Configuration
        var intervalMinutes = configuration.GetValue<int>("DuplicateDetection:ReviewIntervalMinutes", 30);
        _checkInterval = TimeSpan.FromMinutes(intervalMinutes);
        _isEnabled = configuration.GetValue<bool>("DuplicateDetection:EnableReviewWorker", true);
        _defaultAssigneeRole = configuration.GetValue<string>("DuplicateDetection:DefaultAssigneeRole") ?? "Admin";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("Duplicate Review Worker is disabled via configuration");
            return;
        }

        _logger.LogInformation("Duplicate Review Worker starting (interval: {Interval})", _checkInterval);

        // Seed default dedup rules immediately so that dedup checks work from the first API request
        try
        {
            using var seedScope = _serviceProvider.CreateScope();
            var seedContext = seedScope.ServiceProvider.GetRequiredService<CrmDbContext>();
            await EnsureDefaultRulesSeededAsync(seedContext, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed default duplicate rules on startup");
        }

        // Initial delay to allow application startup and first batch of data to settle
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingCandidatesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Duplicate Review Worker");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Duplicate Review Worker stopped");
    }

    private async Task ProcessPendingCandidatesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();

        // Ensure default dedup rules exist on first run
        await EnsureDefaultRulesSeededAsync(context, cancellationToken);

        _logger.LogDebug("Checking for unassigned duplicate candidates...");

        // Find pending candidates that have no reviewer assigned yet
        var unassignedCandidates = await context.DuplicateCandidates
            .Where(c => !c.IsDeleted
                        && c.Status == DuplicateCandidateStatus.Pending
                        && c.AssignedToUserId == null)
            .OrderByDescending(c => c.MatchScore)
            .Take(100) // Process in batches
            .ToListAsync(cancellationToken);

        if (!unassignedCandidates.Any())
        {
            _logger.LogDebug("No unassigned duplicate candidates found");
            return;
        }

        // Find the default assignee — first active user with the configured role
        var assignee = await FindDefaultAssigneeAsync(context, cancellationToken);
        if (assignee == null)
        {
            _logger.LogWarning(
                "No user found with role '{Role}' to assign duplicate candidates to. Skipping assignment.",
                _defaultAssigneeRole);
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var candidate in unassignedCandidates)
        {
            candidate.AssignedToUserId = assignee.Id;
            candidate.AssignedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Assigned {Count} unassigned duplicate candidates to user {UserId} ({UserName})",
            unassignedCandidates.Count, assignee.Id, assignee.Username);

        // Log a summary by entity type
        var byEntityType = unassignedCandidates.GroupBy(c => c.EntityType);
        foreach (var group in byEntityType)
        {
            _logger.LogInformation(
                "  {EntityType}: {Count} candidates (scores: {MinScore}-{MaxScore}%)",
                group.Key, group.Count(), group.Min(c => c.MatchScore), group.Max(c => c.MatchScore));
        }
    }

    private async Task<User?> FindDefaultAssigneeAsync(CrmDbContext context, CancellationToken cancellationToken)
    {
        // Resolve role string to UserRole enum value
        var roleValue = _defaultAssigneeRole.ToLowerInvariant() switch
        {
            "admin" => (int)UserRole.Admin,
            "manager" => (int)UserRole.Manager,
            "sales" => (int)UserRole.Sales,
            _ => (int)UserRole.Admin
        };

        // Find a user by role
        var assignee = await context.Users
            .Where(u => !u.IsDeleted && u.IsActive && u.Role == roleValue)
            .OrderBy(u => u.Id) // Deterministic: pick first matching user by ID
            .FirstOrDefaultAsync(cancellationToken);

        if (assignee != null)
            return assignee;

        // Fallback: find any active admin user
        assignee = await context.Users
            .Where(u => !u.IsDeleted && u.IsActive && u.Role == (int)UserRole.Admin)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return assignee;
    }

    #region Default Rule Seeding

    private static bool _rulesSeeded = false;

    private async Task EnsureDefaultRulesSeededAsync(CrmDbContext context, CancellationToken cancellationToken)
    {
        if (_rulesSeeded)
            return;

        var existingRulesCount = await context.Set<DuplicateRule>()
            .CountAsync(r => !r.IsDeleted, cancellationToken);

        if (existingRulesCount > 0)
        {
            _rulesSeeded = true;
            _logger.LogDebug("Duplicate detection rules already exist ({Count} rules)", existingRulesCount);
            return;
        }

        _logger.LogInformation("Seeding default duplicate detection rules...");

        var rules = GetDefaultDuplicateRules();
        foreach (var rule in rules)
        {
            context.Set<DuplicateRule>().Add(rule);
        }

        await context.SaveChangesAsync(cancellationToken);
        _rulesSeeded = true;

        _logger.LogInformation("Seeded {Count} default duplicate detection rules", rules.Count);
    }

    private static List<DuplicateRule> GetDefaultDuplicateRules()
    {
        var now = DateTime.UtcNow;
        return new List<DuplicateRule>
        {
            // Account duplicate detection
            new DuplicateRule
            {
                Name = "Account Duplicate Detection",
                Description = "Detects duplicate accounts by name, email, and company",
                EntityType = DuplicateEntityType.Account,
                IsActive = true,
                RunOnCreate = true,
                MatchThreshold = 70,
                Priority = 1,
                Action = DuplicateAction.QueueForReview,
                CreatedAt = now,
                MatchFields = new List<DuplicateMatchField>
                {
                    new() { FieldName = "Email", DisplayName = "Email", MatchType = MatchType.Exact, Weight = 100, IsRequired = false, Order = 1 },
                    new() { FieldName = "Name", DisplayName = "Name", MatchType = MatchType.Fuzzy, Weight = 80, FuzzyTolerance = 85, IsRequired = false, Order = 2 },
                    new() { FieldName = "Phone", DisplayName = "Phone", MatchType = MatchType.Normalized, Weight = 60, IsRequired = false, Order = 3 },
                    new() { FieldName = "CompanyName", DisplayName = "Company", MatchType = MatchType.Fuzzy, Weight = 50, FuzzyTolerance = 80, IsRequired = false, Order = 4 }
                }
            },

            // Contact duplicate detection
            new DuplicateRule
            {
                Name = "Contact Duplicate Detection",
                Description = "Detects duplicate contacts by name, email, and phone",
                EntityType = DuplicateEntityType.Contact,
                IsActive = true,
                RunOnCreate = true,
                MatchThreshold = 70,
                Priority = 1,
                Action = DuplicateAction.QueueForReview,
                CreatedAt = now,
                MatchFields = new List<DuplicateMatchField>
                {
                    new() { FieldName = "Email", DisplayName = "Email", MatchType = MatchType.Exact, Weight = 100, IsRequired = false, Order = 1 },
                    new() { FieldName = "FirstName", DisplayName = "First Name", MatchType = MatchType.Fuzzy, Weight = 60, FuzzyTolerance = 85, IsRequired = false, Order = 2 },
                    new() { FieldName = "LastName", DisplayName = "Last Name", MatchType = MatchType.Fuzzy, Weight = 80, FuzzyTolerance = 85, IsRequired = false, Order = 3 },
                    new() { FieldName = "Phone", DisplayName = "Phone", MatchType = MatchType.Normalized, Weight = 60, IsRequired = false, Order = 4 },
                    new() { FieldName = "CompanyName", DisplayName = "Company", MatchType = MatchType.Fuzzy, Weight = 40, FuzzyTolerance = 80, IsRequired = false, Order = 5 }
                }
            },

            // Lead duplicate detection
            new DuplicateRule
            {
                Name = "Lead Duplicate Detection",
                Description = "Detects duplicate leads by name, email, company, and phone",
                EntityType = DuplicateEntityType.Lead,
                IsActive = true,
                RunOnCreate = true,
                MatchThreshold = 70,
                Priority = 1,
                Action = DuplicateAction.QueueForReview,
                CreatedAt = now,
                MatchFields = new List<DuplicateMatchField>
                {
                    new() { FieldName = "Email", DisplayName = "Email", MatchType = MatchType.Exact, Weight = 100, IsRequired = false, Order = 1 },
                    new() { FieldName = "FirstName", DisplayName = "First Name", MatchType = MatchType.Fuzzy, Weight = 60, FuzzyTolerance = 85, IsRequired = false, Order = 2 },
                    new() { FieldName = "LastName", DisplayName = "Last Name", MatchType = MatchType.Fuzzy, Weight = 80, FuzzyTolerance = 85, IsRequired = false, Order = 3 },
                    new() { FieldName = "Phone", DisplayName = "Phone", MatchType = MatchType.Normalized, Weight = 60, IsRequired = false, Order = 4 },
                    new() { FieldName = "CompanyName", DisplayName = "Company", MatchType = MatchType.Fuzzy, Weight = 50, FuzzyTolerance = 80, IsRequired = false, Order = 5 }
                }
            },

            // Opportunity duplicate detection
            new DuplicateRule
            {
                Name = "Opportunity Duplicate Detection",
                Description = "Detects duplicate opportunities by name and account",
                EntityType = DuplicateEntityType.Opportunity,
                IsActive = true,
                RunOnCreate = true,
                MatchThreshold = 75,
                Priority = 1,
                Action = DuplicateAction.QueueForReview,
                CreatedAt = now,
                MatchFields = new List<DuplicateMatchField>
                {
                    new() { FieldName = "Name", DisplayName = "Name", MatchType = MatchType.Fuzzy, Weight = 80, FuzzyTolerance = 85, IsRequired = false, Order = 1 },
                    new() { FieldName = "AccountId", DisplayName = "Account", MatchType = MatchType.Exact, Weight = 100, IsRequired = false, Order = 2 },
                    new() { FieldName = "Amount", DisplayName = "Amount", MatchType = MatchType.Exact, Weight = 40, IsRequired = false, Order = 3 }
                }
            },

            // Product duplicate detection
            new DuplicateRule
            {
                Name = "Product Duplicate Detection",
                Description = "Detects duplicate products by name and SKU",
                EntityType = DuplicateEntityType.Product,
                IsActive = true,
                RunOnCreate = true,
                MatchThreshold = 75,
                Priority = 1,
                Action = DuplicateAction.QueueForReview,
                CreatedAt = now,
                MatchFields = new List<DuplicateMatchField>
                {
                    new() { FieldName = "Name", DisplayName = "Name", MatchType = MatchType.Fuzzy, Weight = 80, FuzzyTolerance = 85, IsRequired = false, Order = 1 },
                    new() { FieldName = "SKU", DisplayName = "SKU", MatchType = MatchType.Exact, Weight = 100, IsRequired = false, Order = 2 },
                    new() { FieldName = "ProductCode", DisplayName = "Product Code", MatchType = MatchType.Exact, Weight = 90, IsRequired = false, Order = 3 }
                }
            },

            // Campaign duplicate detection
            new DuplicateRule
            {
                Name = "Campaign Duplicate Detection",
                Description = "Detects duplicate marketing campaigns by name and type",
                EntityType = DuplicateEntityType.Campaign,
                IsActive = true,
                RunOnCreate = true,
                MatchThreshold = 75,
                Priority = 1,
                Action = DuplicateAction.QueueForReview,
                CreatedAt = now,
                MatchFields = new List<DuplicateMatchField>
                {
                    new() { FieldName = "Name", DisplayName = "Name", MatchType = MatchType.Fuzzy, Weight = 90, FuzzyTolerance = 85, IsRequired = false, Order = 1 },
                    new() { FieldName = "Type", DisplayName = "Type", MatchType = MatchType.Exact, Weight = 60, IsRequired = false, Order = 2 }
                }
            },

            // Interaction duplicate detection
            new DuplicateRule
            {
                Name = "Interaction Duplicate Detection",
                Description = "Detects duplicate interactions by subject, type, account, and date",
                EntityType = DuplicateEntityType.Interaction,
                IsActive = true,
                RunOnCreate = true,
                MatchThreshold = 80,
                Priority = 1,
                Action = DuplicateAction.Warn,
                CreatedAt = now,
                MatchFields = new List<DuplicateMatchField>
                {
                    new() { FieldName = "Subject", DisplayName = "Subject", MatchType = MatchType.Fuzzy, Weight = 70, FuzzyTolerance = 85, IsRequired = false, Order = 1 },
                    new() { FieldName = "Type", DisplayName = "Type", MatchType = MatchType.Exact, Weight = 60, IsRequired = false, Order = 2 },
                    new() { FieldName = "AccountId", DisplayName = "Account", MatchType = MatchType.Exact, Weight = 90, IsRequired = false, Order = 3 },
                    new() { FieldName = "InteractionDate", DisplayName = "Date", MatchType = MatchType.Exact, Weight = 80, IsRequired = false, Order = 4 }
                }
            }
        };
    }

    #endregion
}
