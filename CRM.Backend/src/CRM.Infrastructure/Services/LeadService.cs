// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

public class LeadService : ILeadService
{
    private readonly ICrmDbContext _context;
    private readonly IEntityEventDispatcher _eventDispatcher;
    private readonly ILogger<LeadService> _logger;

    public LeadService(ICrmDbContext context, IEntityEventDispatcher eventDispatcher, ILogger<LeadService> logger)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<(IEnumerable<LeadSummaryDto> Items, int TotalCount, int Page, int PageSize, int TotalPages)> GetAllAsync(int page = 1, int pageSize = 25)
    {
        var query = _context.Set<Lead>().Where(l => !l.IsDeleted).OrderByDescending(l => l.CreatedAt);
        var totalCount = await query.CountAsync();
        var leads = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeadSummaryDto
            {
                Id = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Title = l.Title,
                Status = l.Status.ToString(),
                Source = l.Source.ToString(),
                Score = l.Score,
                FitScore = l.FitScore,
                EngagementScore = l.EngagementScore,
                OwnerId = l.OwnerId,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return (leads, totalCount, page, pageSize, totalPages);
    }

    public async Task<LeadDto?> GetByIdAsync(int id)
    {
        var lead = await _context.Set<Lead>()
            .Where(l => l.Id == id && !l.IsDeleted)
            .Select(l => new LeadDto
            {
                Id = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Title = l.Title,
                Status = l.Status.ToString(),
                Source = l.Source.ToString(),
                Score = l.Score,
                FitScore = l.FitScore,
                EngagementScore = l.EngagementScore,
                QualificationNotes = l.QualificationNotes,
                Region = l.Region,
                Website = l.Website,
                Tags = l.Tags,
                OwnerId = l.OwnerId,
                AccountId = l.AccountId,
                ContactId = l.ContactId,
                CampaignId = l.CampaignId,
                MqlDate = l.MqlDate,
                SqlDate = l.SqlDate,
                LastActivityDate = l.LastActivityDate,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return lead;
    }

    public async Task<int> CreateAsync(Lead lead)
    {
        lead.Status = LeadLifecycleStatus.New;
        lead.CreatedAt = DateTime.UtcNow;
        _context.Set<Lead>().Add(lead);
        await _context.SaveChangesAsync();

        // Fire workflow triggers for entity creation
        _eventDispatcher.DispatchEntityEvent("Lead", lead.Id, WorkflowTriggerType.OnCreate);

        return lead.Id;
    }

    public async Task<bool> UpdateAsync(int id, Action<Lead> applyChanges)
    {
        var lead = await _context.Set<Lead>().FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        if (lead == null)
            return false;

        applyChanges(lead);
        lead.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _eventDispatcher.DispatchEntityEvent("Lead", lead.Id, WorkflowTriggerType.OnUpdate);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var lead = await _context.Set<Lead>().FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        if (lead == null)
            return false;

        lead.IsDeleted = true;
        lead.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _eventDispatcher.DispatchEntityEvent("Lead", lead.Id, WorkflowTriggerType.OnDelete);
        return true;
    }

    public async Task<(int OpportunityId, int LeadId)> ConvertAsync(int id, string? opportunityName, int? accountId, decimal? estimatedValue, DateTime? expectedCloseDate)
    {
        var lead = await _context.Set<Lead>().FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        if (lead == null)
            throw new InvalidOperationException($"Lead with ID {id} not found");
        if (lead.Status == LeadLifecycleStatus.Converted)
            throw new InvalidOperationException("Lead has already been converted");

        var opportunity = new Opportunity
        {
            Name = opportunityName ?? $"{lead.CompanyName} - Opportunity",
            AccountId = accountId ?? lead.AccountId ?? 0,
            PrimaryContactId = lead.ContactId,
            Amount = estimatedValue ?? 0,
            Stage = OpportunityStage.Discovery,
            Probability = 10,
            ExpectedCloseDate = expectedCloseDate ?? DateTime.UtcNow.AddMonths(3),
            SalesOwnerId = lead.OwnerId,
            LeadId = lead.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Opportunity>().Add(opportunity);

        lead.Status = LeadLifecycleStatus.Converted;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Fire events
        _eventDispatcher.DispatchEntityEvent("Opportunity", opportunity.Id, WorkflowTriggerType.OnCreate);
        _eventDispatcher.DispatchEntityEvent("Lead", lead.Id, WorkflowTriggerType.OnUpdate);

        return (opportunity.Id, lead.Id);
    }

    public async Task<IEnumerable<LeadSummaryDto>> GetByStatusAsync(LeadLifecycleStatus status)
    {
        var leads = await _context.Set<Lead>()
            .Where(l => l.Status == status && !l.IsDeleted)
            .OrderByDescending(l => l.Score)
            .Select(l => new LeadSummaryDto
            {
                Id = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Title = l.Title,
                Status = l.Status.ToString(),
                Source = l.Source.ToString(),
                Score = l.Score,
                FitScore = l.FitScore,
                EngagementScore = l.EngagementScore,
                OwnerId = l.OwnerId,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        return leads;
    }

    public async Task<object> GetStatsAsync()
    {
        var total = await _context.Set<Lead>().CountAsync(l => !l.IsDeleted);
        var newLeads = await _context.Set<Lead>().CountAsync(l => l.Status == LeadLifecycleStatus.New && !l.IsDeleted);
        var working = await _context.Set<Lead>().CountAsync(l => l.Status == LeadLifecycleStatus.Working && !l.IsDeleted);
        var qualified = await _context.Set<Lead>().CountAsync(l => l.Status == LeadLifecycleStatus.Qualified && !l.IsDeleted);
        var converted = await _context.Set<Lead>().CountAsync(l => l.Status == LeadLifecycleStatus.Converted && !l.IsDeleted);
        var disqualified = await _context.Set<Lead>().CountAsync(l => l.Status == LeadLifecycleStatus.Disqualified && !l.IsDeleted);
        var avgScore = await _context.Set<Lead>().Where(l => !l.IsDeleted).AverageAsync(l => (double?)l.Score) ?? 0;

        return new
        {
            total,
            newLeads,
            working,
            qualified,
            converted,
            disqualified,
            avgScore
        };
    }

    public async Task<IEnumerable<LeadSummaryDto>> SearchAsync(string searchTerm)
    {
        var q = searchTerm ?? string.Empty;
        var leads = await _context.Set<Lead>()
            .Where(l => !l.IsDeleted && (
                l.FirstName.Contains(q) ||
                l.LastName.Contains(q) ||
                l.Email.Contains(q) ||
                (l.CompanyName ?? "").Contains(q)
            ))
            .Select(l => new LeadSummaryDto
            {
                Id = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Email = l.Email,
                CompanyName = l.CompanyName,
                Status = l.Status.ToString(),
                Source = l.Source.ToString(),
                Score = l.Score,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return leads;
    }

    public async Task<bool> AssignOwnerAsync(int leadId, int ownerId)
    {
        var lead = await _context.Set<Lead>().FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted);
        if (lead == null)
            return false;

        lead.OwnerId = ownerId;
        lead.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _eventDispatcher.DispatchEntityEvent("Lead", lead.Id, WorkflowTriggerType.OnUpdate);
        return true;
    }
}
