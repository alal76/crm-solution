// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Core.DTOs;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Opportunity service implementation.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IOpportunityInputPort (primary/driving port)
/// - Implements IOpportunityService (backward compatibility)
/// - Uses IRepository for data access (secondary/driven port)
/// </summary>
public class OpportunityService : IOpportunityService, IOpportunityInputPort
{
    private readonly IRepository<Opportunity> _repository;
    private readonly IRepository<CRM.Core.Entities.EntityTag> _entityTagRepository;
    private readonly IRepository<CRM.Core.Entities.CustomField> _customFieldRepository;
    private readonly NormalizationService _normalizationService;
    private readonly IEntityEventDispatcher _eventDispatcher;

    public OpportunityService(IRepository<Opportunity> repository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService,
        IEntityEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Opportunity?> GetOpportunityByIdAsync(int id)
    {
        var opp = await _repository.GetByIdAsync(id);
        return opp;
    }

    public async Task<IEnumerable<Opportunity>> GetOpportunitiesByAccountAsync(int accountId)
    {
        var items = await _repository.FindAsync(o => !o.IsDeleted && o.AccountId == accountId);
        return items;
    }

    /// <summary>
    /// Get opportunities by customer ID (alias for GetOpportunitiesByAccountAsync for backward compatibility)
    /// </summary>
    public async Task<List<Opportunity>> GetOpportunitiesByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var opportunities = await _repository.FindAsync(o => !o.IsDeleted && o.AccountId == customerId);
        return opportunities?.ToList() ?? new List<Opportunity>();
    }

    public async Task<IEnumerable<Opportunity>> GetOpenOpportunitiesAsync()
    {
        var items = await _repository.FindAsync(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost);
        return items;
    }

    public async Task<int> CreateOpportunityAsync(Opportunity opportunity)
    {
        await _repository.AddAsync(opportunity);
        await _repository.SaveAsync();

        // Fire workflow triggers for entity creation
        _eventDispatcher.DispatchEntityEvent("Opportunity", opportunity.Id, WorkflowTriggerType.OnCreate);

        return opportunity.Id;
    }

    public async Task UpdateOpportunityAsync(Opportunity opportunity)
    {
        opportunity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(opportunity);
        await _repository.SaveAsync();

        // Fire workflow triggers for entity update
        _eventDispatcher.DispatchEntityEvent("Opportunity", opportunity.Id, WorkflowTriggerType.OnUpdate);
    }

    public async Task DeleteOpportunityAsync(int id)
    {
        var opportunity = await _repository.GetByIdAsync(id);
        if (opportunity != null)
        {
            await _repository.DeleteAsync(opportunity);
            await _repository.SaveAsync();

            // Fire workflow triggers for entity deletion
            _eventDispatcher.DispatchEntityEvent("Opportunity", id, WorkflowTriggerType.OnDelete);
        }
    }

    public async Task<decimal> GetTotalPipelineAsync()
    {
        var opportunities = await GetOpenOpportunitiesAsync();
        return opportunities.Sum(o => o.Amount);
    }
}
