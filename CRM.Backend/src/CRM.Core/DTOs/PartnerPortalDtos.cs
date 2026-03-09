// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>Partner dashboard summary returned by GET /api/partner-portal/dashboard. FLAG-002.</summary>
public class PartnerDashboardDto
{
    public string PartnerName { get; set; } = string.Empty;

    public int ActiveDealCount { get; set; }

    public int TotalLeadCount { get; set; }

    public decimal CommissionEarnedThisMonth { get; set; }

    public decimal PipelineValue { get; set; }

    public IEnumerable<PartnerDealDto> RecentDeals { get; set; } = [];

    public IEnumerable<PartnerLeadDto> RecentLeads { get; set; } = [];
}

/// <summary>Partner-facing deal (opportunity) summary. FLAG-002.</summary>
public class PartnerDealDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Stage { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    public string? ExpectedCloseDate { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>Partner-facing lead summary. FLAG-002.</summary>
public class PartnerLeadDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

/// <summary>Partner commission record returned by GET /api/partner-portal/commissions. FLAG-002.</summary>
public class PartnerCommissionDto
{
    public int Id { get; set; }

    public string CommissionNumber { get; set; } = string.Empty;

    public string CommissionPeriod { get; set; } = string.Empty;

    public decimal CommissionAmount { get; set; }

    public decimal FinalCommissionAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = string.Empty;

    public DateTime EarnedDate { get; set; }

    public DateTime? PaidDate { get; set; }
}

/// <summary>A partner-facing resource (document, guide, link). PORTAL-025.</summary>
public class PartnerResourceDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>Payload for a partner registering a new deal. PORTAL-025.</summary>
public class RegisterPartnerDealDto
{
    public string ContactFirstName { get; set; } = string.Empty;

    public string ContactLastName { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public decimal? DealValue { get; set; }

    public string? Notes { get; set; }
}
