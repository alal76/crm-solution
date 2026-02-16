// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

namespace CRM.Core.Interfaces.ITSM;

public interface IEscalationProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken ct = default);
    Task ProcessIncidentAsync(int incidentId, CancellationToken ct = default);
    Task ProcessServiceRequestAsync(int serviceRequestId, CancellationToken ct = default);
}
