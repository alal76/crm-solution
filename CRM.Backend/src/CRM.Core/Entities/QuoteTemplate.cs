using System;

namespace CRM.Core.Entities;

/// <summary>
/// Quote template for standardized quote generation with predefined sections.
/// </summary>
public class QuoteTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HeaderHtml { get; set; }
    public string? FooterHtml { get; set; }
    public string? BodyHtml { get; set; }
    public string? TermsAndConditions { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CssStyles { get; set; }
}
