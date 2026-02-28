// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Credit Note entity — issued when a refund or return credit is granted.
/// CreditNoteNumber format: CN-{year}-{id:D5}  e.g. CN-2026-00042
/// BACK-007: Order Returns + Credit Notes
/// </summary>
[Table("CreditNotes")]
public class CreditNote : BaseEntity
{
    /// <summary>Human-readable credit note reference (CN-{year}-{id:D5}).</summary>
    [Required]
    [MaxLength(50)]
    public string CreditNoteNumber { get; set; } = string.Empty;

    /// <summary>Optional linked order ID.</summary>
    public int? OrderId { get; set; }

    /// <summary>Navigation to linked order.</summary>
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    /// <summary>Optional linked invoice ID.</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Navigation to linked invoice.</summary>
    [ForeignKey("InvoiceId")]
    public Invoice? Invoice { get; set; }

    /// <summary>Credit amount in the order currency.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Amount { get; set; }

    /// <summary>Reason for issuing the credit note.</summary>
    [MaxLength(1000)]
    public string? Reason { get; set; }

    /// <summary>UTC timestamp when the credit note was issued.</summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Whether the credit note has been applied to a future purchase or payout.</summary>
    public bool IsApplied { get; set; }

    /// <summary>UTC timestamp when the credit note was applied (null if pending).</summary>
    public DateTime? AppliedAt { get; set; }
}
