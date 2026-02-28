// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
namespace CRM.Core.Dtos;

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
