// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces.ITSM;

public interface IEscalationProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken ct = default);
    Task ProcessIncidentAsync(int incidentId, CancellationToken ct = default);
    Task ProcessServiceRequestAsync(int serviceRequestId, CancellationToken ct = default);
}
