// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for Sales Configuration
/// </summary>
public class SalesConfigurationDto
{
    public int Id { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public string Description { get; set; }

    public string DataType { get; set; }

    public bool IsSystem { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for admin configuration overview
/// </summary>
public class AdminConfigurationDto
{
    public SalesAdminConfigDto SalesConfig { get; set; }

    public ServiceDeskAdminConfigDto ServiceDeskConfig { get; set; }

    public NotificationAdminConfigDto NotificationConfig { get; set; }

    public List<SalesConfigurationDto> CustomConfigurations { get; set; }
}

/// <summary>
/// Sales module admin configuration
/// </summary>
public class SalesAdminConfigDto
{
    public List<CommissionRuleDto> CommissionRules { get; set; } = new();

    public List<DiscountRuleDto> DiscountRules { get; set; } = new();

    public decimal DefaultCommissionPercentage { get; set; }

    public decimal MaxDiscountPercentage { get; set; }

    public bool RequireApprovalForDiscounts { get; set; }

    public bool RequireApprovalForOrders { get; set; }
}

/// <summary>
/// Service Desk admin configuration
/// </summary>
public class ServiceDeskAdminConfigDto
{
    public List<SLAPolicyDto> SLAPolicies { get; set; } = new();

    public List<EscalationRuleDto> EscalationRules { get; set; } = new();

    public List<ServiceQueueDto> ServiceQueues { get; set; } = new();

    public bool AutoAssignRequests { get; set; }

    public int DefaultPriorityMinutes { get; set; }
}

/// <summary>
/// Notification/Email admin configuration
/// </summary>
public class NotificationAdminConfigDto
{
    public string SmtpHost { get; set; }

    public int SmtpPort { get; set; }

    public bool SmtpUseSsl { get; set; }

    public string FromAddress { get; set; }

    public string FromName { get; set; }

    public bool NotifyOnAssignment { get; set; }

    public bool NotifyOnCompletion { get; set; }

    public bool NotifyOnEscalation { get; set; }

    public string DefaultTemplate { get; set; }
}
