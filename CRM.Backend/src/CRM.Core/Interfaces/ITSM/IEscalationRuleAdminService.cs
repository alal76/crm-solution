// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Admin service interface for managing escalation rules.
/// </summary>
/// <remarks>
/// DEPRECATED: Use <see cref="IEscalationRuleService"/> instead.
/// This interface is kept for backward compatibility only. New code should inject IEscalationRuleService.
/// TODO-SD005-003: Renamed to IEscalationRuleService.
/// </remarks>
[Obsolete("Use IEscalationRuleService. IEscalationRuleAdminService will be removed in a future release.")]
public interface IEscalationRuleAdminService : IEscalationRuleService
{
}
