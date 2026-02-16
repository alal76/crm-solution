// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

public class EscalationProcessor : IEscalationProcessor
{
    private readonly ILogger<EscalationProcessor> _logger;

    public EscalationProcessor(ILogger<EscalationProcessor> logger)
    {
        _logger = logger;
    }

    public Task<int> ProcessPendingAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Escalation processor invoked (no-op)");
        return Task.FromResult(0);
    }

    public Task ProcessIncidentAsync(int incidentId, CancellationToken ct = default)
    {
        _logger.LogInformation("Escalation processor incident {IncidentId} (no-op)", incidentId);
        return Task.CompletedTask;
    }

    public Task ProcessServiceRequestAsync(int serviceRequestId, CancellationToken ct = default)
    {
        _logger.LogInformation("Escalation processor service request {ServiceRequestId} (no-op)", serviceRequestId);
        return Task.CompletedTask;
    }
}
