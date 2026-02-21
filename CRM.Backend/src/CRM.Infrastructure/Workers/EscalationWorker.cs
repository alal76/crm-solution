// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Workers;

public class EscalationWorker
{
    private readonly IEscalationProcessor _processor;
    private readonly ILogger<EscalationWorker> _logger;

    public EscalationWorker(IEscalationProcessor processor, ILogger<EscalationWorker> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task<int> ProcessAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Escalation worker run started");
        var count = await _processor.ProcessPendingAsync(ct);
        _logger.LogInformation("Escalation worker run completed with {Count} items", count);
        return count;
    }
}
