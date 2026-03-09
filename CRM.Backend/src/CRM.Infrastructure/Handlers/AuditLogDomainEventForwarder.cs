// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities.Events;
using CRM.Core.Ports.Output.Events;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Handlers;

/// <summary>
/// AP-059: Phase 1 audit log forwarder.
/// Implements <see cref="IDomainEventHandler{TEvent}"/> for all 22 domain event types
/// defined in the AP-059 domain enrichment phase. Each handler writes a structured
/// log entry; real side-effects (audit records, notifications, etc.) will be layered
/// on top in later phases without modifying this forwarder.
/// </summary>
public sealed class AuditLogDomainEventForwarder :
    IDomainEventHandler<ServiceRequestResolvedEvent>,
    IDomainEventHandler<ServiceRequestClosedEvent>,
    IDomainEventHandler<ServiceRequestEscalatedEvent>,
    IDomainEventHandler<ServiceRequestAssignedEvent>,
    IDomainEventHandler<ServiceRequestReopenedEvent>,
    IDomainEventHandler<OpportunityStageChangedEvent>,
    IDomainEventHandler<OpportunityClosedEvent>,
    IDomainEventHandler<OpportunityRevenueUpdatedEvent>,
    IDomainEventHandler<LeadConvertedEvent>,
    IDomainEventHandler<LeadDisqualifiedEvent>,
    IDomainEventHandler<LeadQualifiedEvent>,
    IDomainEventHandler<LeadAssignedEvent>,
    IDomainEventHandler<AccountLifecycleChangedEvent>,
    IDomainEventHandler<AccountPrimaryContactSetEvent>,
    IDomainEventHandler<AccountDeactivatedEvent>,
    IDomainEventHandler<ContractApprovedEvent>,
    IDomainEventHandler<ContractRenewedEvent>,
    IDomainEventHandler<ContractTerminatedEvent>,
    IDomainEventHandler<ContractExpiredEvent>,
    IDomainEventHandler<IncidentResolvedEvent>,
    IDomainEventHandler<IncidentClosedEvent>,
    IDomainEventHandler<IncidentEscalatedEvent>
{
    private readonly ILogger<AuditLogDomainEventForwarder> _logger;

    public AuditLogDomainEventForwarder(ILogger<AuditLogDomainEventForwarder> logger)
    {
        _logger = logger;
    }

    // ── Service Request ──────────────────────────────────────────────────────

    public Task HandleAsync(ServiceRequestResolvedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ServiceRequestId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ServiceRequestClosedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ServiceRequestId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ServiceRequestEscalatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ServiceRequestId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ServiceRequestAssignedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ServiceRequestId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ServiceRequestReopenedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ServiceRequestId);
        return Task.CompletedTask;
    }

    // ── Opportunity ──────────────────────────────────────────────────────────

    public Task HandleAsync(OpportunityStageChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.OpportunityId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(OpportunityClosedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.OpportunityId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(OpportunityRevenueUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.OpportunityId);
        return Task.CompletedTask;
    }

    // ── Lead ─────────────────────────────────────────────────────────────────

    public Task HandleAsync(LeadConvertedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.LeadId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(LeadDisqualifiedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.LeadId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(LeadQualifiedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.LeadId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(LeadAssignedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.LeadId);
        return Task.CompletedTask;
    }

    // ── Account ──────────────────────────────────────────────────────────────

    public Task HandleAsync(AccountLifecycleChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.AccountId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(AccountPrimaryContactSetEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.AccountId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(AccountDeactivatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.AccountId);
        return Task.CompletedTask;
    }

    // ── Contract ─────────────────────────────────────────────────────────────

    public Task HandleAsync(ContractApprovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ContractId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ContractRenewedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ContractId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ContractTerminatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ContractId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ContractExpiredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.ContractId);
        return Task.CompletedTask;
    }

    // ── Incident ─────────────────────────────────────────────────────────────

    public Task HandleAsync(IncidentResolvedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.IncidentId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(IncidentClosedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.IncidentId);
        return Task.CompletedTask;
    }

    public Task HandleAsync(IncidentEscalatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event raised: {EventType} for entity {EntityId}",
            domainEvent.EventType, domainEvent.IncidentId);
        return Task.CompletedTask;
    }
}
