// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Partner portal service implementation. PORTAL-025.
/// Exposes deal pipeline and resource data to partner organisations.
/// </summary>
public sealed class PartnerPortalService : IPartnerPortalService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<PartnerPortalService> _logger;

    public PartnerPortalService(ICrmDbContext db, ILogger<PartnerPortalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<OpportunityDto>> GetPartnerDealsAsync(
        int partnerAccountId,
        CancellationToken ct = default)
    {
        var items = await _db.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.AccountId == partnerAccountId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        _logger.LogDebug("GetPartnerDealsAsync: {Count} deals for account {AccountId}", items.Count, partnerAccountId);
        return items.Select(MapOpportunity);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<OpportunityDto>> GetPartnerOpportunitiesAsync(
        int partnerAccountId,
        CancellationToken ct = default)
    {
        // Return open (non-closed) opportunities for the partner account.
        var closed = new[]
        {
            OpportunityStage.ClosedWon,
            OpportunityStage.ClosedLost,
        };

        var items = await _db.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted
                        && o.AccountId == partnerAccountId
                        && !closed.Contains(o.Stage))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        _logger.LogDebug("GetPartnerOpportunitiesAsync: {Count} open opportunities for account {AccountId}", items.Count, partnerAccountId);
        return items.Select(MapOpportunity);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<PartnerResourceDto>> GetResourcesAsync(CancellationToken ct = default)
    {
        // No PartnerResources table yet — return an empty set.
        // TODO: Create PartnerResources entity + DbSet and populate here. // NOSONAR
        _logger.LogDebug("GetResourcesAsync: no partner resources table yet, returning empty list");
        return Task.FromResult<IEnumerable<PartnerResourceDto>>(Array.Empty<PartnerResourceDto>());
    }

    /// <inheritdoc/>
    public async Task RegisterDealAsync(RegisterPartnerDealDto dto, CancellationToken ct = default)
    {
        // Persist a minimal opportunity as a registered partner deal.
        var opportunity = new Opportunity
        {
            Name = $"{dto.ContactFirstName} {dto.ContactLastName} – {dto.CompanyName}",
            Stage = OpportunityStage.Discovery,
            Amount = dto.DealValue ?? 0m,
            Currency = "USD",
            Probability = 10,
            TermLengthMonths = 12,
            SolutionNotes = dto.Notes,
            AccountId = 0, // Partner-submitted deal — account resolution is a future step.
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _db.Opportunities.Add(opportunity);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("RegisterDealAsync: registered partner deal {Name}", opportunity.Name);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static OpportunityDto MapOpportunity(Opportunity o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        Stage = (int)o.Stage,
        StageName = o.Stage.ToString(),
        StageId = o.StageId,
        Probability = o.Probability,
        Amount = o.Amount,
        Currency = o.Currency,
        ExpectedCloseDate = o.ExpectedCloseDate?.ToString("yyyy-MM-dd"),
        PricingModel = (int)o.PricingModel,
        PricingModelName = o.PricingModel.ToString(),
        TermLengthMonths = o.TermLengthMonths,
        SolutionNotes = o.SolutionNotes,
        QualificationReason = o.QualificationReason.HasValue ? (int?)o.QualificationReason.Value : null,
        QualificationNotes = o.QualificationNotes,
        Region = o.Region,
        AccountId = o.AccountId,
        PrimaryContactId = o.PrimaryContactId,
    };

    /// <inheritdoc/>
    public async Task<PartnerDashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default)
    {
        var closedStages = new[] { OpportunityStage.ClosedWon, OpportunityStage.ClosedLost };

        var deals = await _db.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && (o.SalesOwnerId == userId || o.UserId == userId))
            .ToListAsync(ct);

        var leads = await _db.Leads
            .AsNoTracking()
            .Where(l => !l.IsDeleted && l.OwnerId == userId)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var commissionThisMonth = await _db.Commissions
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.UserId == userId && c.EarnedDate >= monthStart)
            .SumAsync(c => (decimal?)c.FinalCommissionAmount, ct) ?? 0m;

        var activeDeals = deals.Where(o => !closedStages.Contains(o.Stage)).ToList();

        return new PartnerDashboardDto
        {
            PartnerName = string.Empty, // populated from auth context on client
            ActiveDealCount = activeDeals.Count,
            TotalLeadCount = leads.Count,
            CommissionEarnedThisMonth = commissionThisMonth,
            PipelineValue = activeDeals.Sum(o => o.Amount),
            RecentDeals = deals
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new PartnerDealDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Stage = o.Stage.ToString(),
                    Amount = o.Amount,
                    Currency = o.Currency,
                    ExpectedCloseDate = o.ExpectedCloseDate?.ToString("yyyy-MM-dd"),
                    CreatedAt = o.CreatedAt,
                }),
            RecentLeads = leads
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .Select(l => new PartnerLeadDto
                {
                    Id = l.Id,
                    FirstName = l.FirstName,
                    LastName = l.LastName,
                    Email = l.Email,
                    CompanyName = l.CompanyName,
                    Status = l.Status.ToString(),
                    CreatedAt = l.CreatedAt,
                }),
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PartnerLeadDto>> GetLeadsAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var skip = Math.Max(0, (page - 1) * pageSize);
        var items = await _db.Leads
            .AsNoTracking()
            .Where(l => !l.IsDeleted && l.OwnerId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return items.Select(l => new PartnerLeadDto
        {
            Id = l.Id,
            FirstName = l.FirstName,
            LastName = l.LastName,
            Email = l.Email,
            CompanyName = l.CompanyName,
            Status = l.Status.ToString(),
            CreatedAt = l.CreatedAt,
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PartnerCommissionDto>> GetCommissionsAsync(
        int userId,
        CancellationToken ct = default)
    {
        var items = await _db.Commissions
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.UserId == userId)
            .OrderByDescending(c => c.EarnedDate)
            .ToListAsync(ct);

        return items.Select(c => new PartnerCommissionDto
        {
            Id = c.Id,
            CommissionNumber = c.CommissionNumber,
            CommissionPeriod = c.CommissionPeriod,
            CommissionAmount = c.CommissionAmount,
            FinalCommissionAmount = c.FinalCommissionAmount,
            Currency = c.CurrencyCode,
            Status = c.Status.ToString(),
            EarnedDate = c.EarnedDate,
            PaidDate = c.PaidDate,
        });
    }
}
