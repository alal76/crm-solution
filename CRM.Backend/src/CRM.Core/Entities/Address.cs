namespace CRM.Core.Entities;

/// <summary>
/// Address entity extracted from Customer/Contact records
/// Reusable across multiple owner types via linking table
/// </summary>
public class Address : BaseEntity
{
    public string Label { get; set; } = "Primary"; // e.g., Billing, Shipping, Home
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
}
