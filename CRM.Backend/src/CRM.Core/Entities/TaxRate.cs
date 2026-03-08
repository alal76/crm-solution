using System;

namespace CRM.Core.Entities;

/// <summary>
/// Tax rate configuration for invoicing and quoting.
/// </summary>
public class TaxRate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? TaxType { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
