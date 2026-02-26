// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Admin contract for managing the Customer Portal configuration and users.
/// Consumed by CRM staff (requires CRM JWT authentication).
/// </summary>
public interface IPortalAdminService
{
    Task<PortalConfigDto> GetConfigAsync(CancellationToken ct = default);
    Task<PortalConfigDto> UpdateConfigAsync(UpdatePortalConfigDto dto, CancellationToken ct = default);
    Task<PagedResultDto<PortalUserDto>> GetPortalUsersAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ActivatePortalUserAsync(int portalUserId, CancellationToken ct = default);
    Task<bool> DeactivatePortalUserAsync(int portalUserId, CancellationToken ct = default);
}
