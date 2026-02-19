// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.


// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for Sales Configuration
/// </summary>
public class SalesConfigurationDto
{
    public int Id { get; set; }

    /// <summary>
    /// Configuration key.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Configuration value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Description of the configuration.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Data type of the configuration.
    /// </summary>
    public string? DataType { get; set; }

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
    public SalesAdminConfigDto? SalesConfig { get; set; }
    public ServiceDeskAdminConfigDto? ServiceDeskConfig { get; set; }
    public NotificationAdminConfigDto? NotificationConfig { get; set; }
    public List<SalesConfigurationDto>? CustomConfigurations { get; set; }
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
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; }
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public bool NotifyOnAssignment { get; set; }
    public bool NotifyOnCompletion { get; set; }
    public bool NotifyOnEscalation { get; set; }
    public string? DefaultTemplate { get; set; }
}
