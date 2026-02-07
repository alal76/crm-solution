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

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Quote status enumeration - Full lifecycle
/// </summary>
public enum QuoteStatus
{
    /// <summary>New quote, not yet edited</summary>
    New = 0,
    /// <summary>Quote is being drafted</summary>
    Draft = 1,
    /// <summary>Quote is under internal approval</summary>
    UnderApproval = 2,
    /// <summary>Quote has been approved internally</summary>
    Approved = 3,
    /// <summary>Quote has been shared/sent to customer</summary>
    Shared = 4,
    /// <summary>Customer has viewed the quote</summary>
    Viewed = 5,
    /// <summary>Customer has accepted the quote</summary>
    Accepted = 6,
    /// <summary>Customer has rejected the quote</summary>
    Rejected = 7,
    /// <summary>Quote has expired (past expiration date)</summary>
    Expired = 8,
    /// <summary>Quote has been revised (superseded by new version)</summary>
    Revised = 9,
    /// <summary>Quote is cancelled</summary>
    Cancelled = 10,
    /// <summary>Quote has been converted to order</summary>
    Converted = 11,
    /// <summary>End of life - no longer active or relevant</summary>
    EndOfLife = 12
}

/// <summary>
/// Quote entity for sales quotes and proposals
/// </summary>
public class Quote : BaseEntity
{
    #region Identification

    /// <summary>Unique quote number</summary>
    [Required]
    [MaxLength(50)]
    public string QuoteNumber { get; set; } = string.Empty;

    /// <summary>External system quote ID</summary>
    [MaxLength(100)]
    public string? ExternalQuoteId { get; set; }

    /// <summary>Quote version number</summary>
    [Range(1, 999)]
    public int Version { get; set; } = 1;

    #endregion

    #region Basic Information

    /// <summary>Quote name/title</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Quote description</summary>
    [MaxLength(5000)]
    public string? Description { get; set; }

    /// <summary>Quote status</summary>
    public QuoteStatus Status { get; set; } = QuoteStatus.New;

    #endregion

    #region Dates

    /// <summary>Quote creation date</summary>
    public DateTime QuoteDate { get; set; } = DateTime.UtcNow;

    /// <summary>Expiration date</summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Date sent to customer</summary>
    public DateTime? SentDate { get; set; }

    /// <summary>Date customer viewed</summary>
    public DateTime? ViewedDate { get; set; }

    /// <summary>Date customer accepted</summary>
    public DateTime? AcceptedDate { get; set; }

    /// <summary>Date customer rejected</summary>
    public DateTime? RejectedDate { get; set; }

    #endregion

    #region Pricing

    /// <summary>Subtotal before discounts</summary>
    [Range(0, 999999999999.99)]
    public decimal Subtotal { get; set; } = 0;

    /// <summary>Discount amount</summary>
    [Range(0, 999999999999.99)]
    public decimal Discount { get; set; } = 0;

    /// <summary>Discount percentage</summary>
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; } = 0;

    /// <summary>Reason for discount</summary>
    [MaxLength(500)]
    public string? DiscountReason { get; set; }

    /// <summary>Tax amount</summary>
    [Range(0, 999999999999.99)]
    public decimal Tax { get; set; } = 0;

    /// <summary>Tax rate percentage</summary>
    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 0;

    /// <summary>Shipping/delivery cost</summary>
    [Range(0, 999999999.99)]
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>Total quote amount</summary>
    [Range(0, 999999999999.99)]
    public decimal Total { get; set; } = 0;

    /// <summary>Currency code (ISO 4217)</summary>
    [MaxLength(3)]
    public string? CurrencyCode { get; set; } = "USD";

    #endregion

    #region Terms

    /// <summary>Payment terms (Net 30, Net 60, etc.)</summary>
    [MaxLength(200)]
    public string? PaymentTerms { get; set; }

    /// <summary>Delivery terms</summary>
    [MaxLength(500)]
    public string? DeliveryTerms { get; set; }

    /// <summary>Terms and conditions</summary>
    [MaxLength(50000)]
    public string? TermsAndConditions { get; set; }

    /// <summary>Warranty information</summary>
    [MaxLength(5000)]
    public string? Warranty { get; set; }

    /// <summary>Quote validity in days</summary>
    [Range(1, 365)]
    public int? ValidityDays { get; set; } = 30;

    #endregion

    #region Line Items

    /// <summary>Line items (Legacy JSON field - prefer QuoteLineItems collection)</summary>
    [MaxLength(50000)]
    public string? LineItems { get; set; }

    #endregion

    #region Billing Address

    /// <summary>Billing contact name</summary>
    [MaxLength(200)]
    public string? BillingName { get; set; }

    /// <summary>Billing street address</summary>
    [MaxLength(500)]
    public string? BillingAddress { get; set; }

    /// <summary>Billing city</summary>
    [MaxLength(100)]
    public string? BillingCity { get; set; }

    /// <summary>Billing state/province</summary>
    [MaxLength(100)]
    public string? BillingState { get; set; }

    /// <summary>Billing ZIP/postal code</summary>
    [MaxLength(20)]
    public string? BillingZipCode { get; set; }

    /// <summary>Billing country</summary>
    [MaxLength(100)]
    public string? BillingCountry { get; set; }

    #endregion

    #region Shipping Address

    /// <summary>Shipping contact name</summary>
    [MaxLength(200)]
    public string? ShippingName { get; set; }

    /// <summary>Shipping street address</summary>
    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    /// <summary>Shipping city</summary>
    [MaxLength(100)]
    public string? ShippingCity { get; set; }

    /// <summary>Shipping state/province</summary>
    [MaxLength(100)]
    public string? ShippingState { get; set; }

    /// <summary>Shipping ZIP/postal code</summary>
    [MaxLength(20)]
    public string? ShippingZipCode { get; set; }

    /// <summary>Shipping country</summary>
    [MaxLength(100)]
    public string? ShippingCountry { get; set; }

    #endregion

    #region Contact Information

    /// <summary>Contact name</summary>
    [MaxLength(200)]
    public string? ContactName { get; set; }

    /// <summary>Contact email address</summary>
    [EmailAddress]
    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    /// <summary>Contact phone number</summary>
    [Phone]
    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    #endregion

    #region Relationships

    /// <summary>Associated customer ID</summary>
    [Column("CustomerId")]
    public int? AccountId { get; set; }

    /// <summary>Associated contact ID</summary>
    public int? ContactId { get; set; }

    /// <summary>Associated opportunity ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Assigned user ID</summary>
    public int? AssignedToUserId { get; set; }

    /// <summary>User who created the quote</summary>
    public int? CreatedByUserId { get; set; }

    /// <summary>User who approved the quote</summary>
    public int? ApprovedByUserId { get; set; }

    /// <summary>Parent quote ID (for revisions)</summary>
    public int? ParentQuoteId { get; set; }

    /// <summary>Relationship manager ID</summary>
    public int? RelationshipManagerId { get; set; }

    #endregion

    #region Approval

    /// <summary>Whether quote requires approval</summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>Whether quote is approved</summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>Approval date</summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>Approval notes</summary>
    [MaxLength(2000)]
    public string? ApprovalNotes { get; set; }

    /// <summary>Date submitted for approval</summary>
    public DateTime? SubmittedForApprovalDate { get; set; }

    #endregion

    #region Signature

    /// <summary>Whether quote is signed</summary>
    public bool IsSigned { get; set; } = false;

    /// <summary>Signature date</summary>
    public DateTime? SignedDate { get; set; }

    /// <summary>Name of signer</summary>
    [MaxLength(200)]
    public string? SignedBy { get; set; }

    /// <summary>Signature image URL</summary>
    [Url]
    [MaxLength(1000)]
    public string? SignatureUrl { get; set; }

    #endregion

    #region Documentation

    /// <summary>External notes (visible to customer)</summary>
    [MaxLength(5000)]
    public string? Notes { get; set; }

    /// <summary>Internal notes (staff only)</summary>
    [MaxLength(5000)]
    public string? InternalNotes { get; set; }

    /// <summary>Attachments (JSON array)</summary>
    [MaxLength(5000)]
    public string? Attachments { get; set; }

    /// <summary>Quote PDF URL</summary>
    [Url]
    [MaxLength(1000)]
    public string? QuotePdfUrl { get; set; }

    #endregion

    #region Classification

    /// <summary>Comma-separated tags</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>Quote category</summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    #endregion

    #region Service/Delivery Tracking

    /// <summary>Expected delivery date</summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>Actual delivery date</summary>
    public DateTime? ActualDeliveryDate { get; set; }

    /// <summary>Warranty period in months</summary>
    [Range(0, 120)]
    public int? WarrantyMonths { get; set; }

    /// <summary>Warranty end date</summary>
    public DateTime? WarrantyEndDate { get; set; }

    /// <summary>Service start date</summary>
    public DateTime? ServiceStartDate { get; set; }

    /// <summary>Service end date</summary>
    public DateTime? ServiceEndDate { get; set; }

    #endregion

    #region Custom Fields

    /// <summary>Custom fields (JSON)</summary>
    [MaxLength(10000)]
    public string? CustomFields { get; set; }

    #endregion

    #region Navigation Properties

    public Account? Account { get; set; }
    public Contact? Contact { get; set; }
    public Opportunity? Opportunity { get; set; }
    public User? AssignedToUser { get; set; }
    public User? CreatedByUser { get; set; }
    public User? ApprovedByUser { get; set; }
    public User? RelationshipManager { get; set; }
    public Quote? ParentQuote { get; set; }
    public ICollection<Quote>? Revisions { get; set; }

    /// <summary>
    /// Quote line items (structured line items)
    /// </summary>
    public ICollection<QuoteLineItem>? QuoteLineItems { get; set; }

    #endregion

    #region Calculation Methods

    /// <summary>
    /// Recalculate quote totals from line items
    /// </summary>
    public void RecalculateFromLineItems()
    {
        if (QuoteLineItems == null || !QuoteLineItems.Any())
        {
            Subtotal = 0;
            Tax = 0;
            Total = ShippingCost;
            return;
        }

        // Only include line items that are marked as included
        var includedItems = QuoteLineItems.Where(li => li.IsIncluded && !li.IsDeleted).ToList();

        foreach (var item in includedItems)
        {
            item.RecalculateTotals();
        }

        Subtotal = includedItems.Sum(li => li.Subtotal);
        var totalLineDiscount = includedItems.Sum(li => li.TotalDiscount);
        Tax = includedItems.Sum(li => li.TaxAmount);

        // Apply quote-level discount
        var afterLineDiscounts = Subtotal - totalLineDiscount;
        var quoteDiscount = DiscountPercent > 0
            ? afterLineDiscounts * (DiscountPercent / 100)
            : Discount;

        Total = afterLineDiscounts - quoteDiscount + Tax + ShippingCost;
        Discount = totalLineDiscount + quoteDiscount;
    }

    /// <summary>
    /// Check if quote is expired
    /// </summary>
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    /// <summary>
    /// Check if quote can be edited
    /// </summary>
    public bool CanEdit => Status == QuoteStatus.New || Status == QuoteStatus.Draft;

    /// <summary>
    /// Check if quote can be submitted for approval
    /// </summary>
    public bool CanSubmitForApproval => Status == QuoteStatus.Draft && RequiresApproval && !IsApproved;

    /// <summary>
    /// Check if quote can be shared with customer
    /// </summary>
    public bool CanShare => (Status == QuoteStatus.Draft && !RequiresApproval) ||
                            (Status == QuoteStatus.Approved);

    #endregion

    #region Aliases

    /// <summary>Alias for Discount for service compatibility</summary>
    public decimal DiscountAmount
    {
        get => Discount;
        set => Discount = value;
    }

    /// <summary>Alias for Tax for service compatibility</summary>
    public decimal TaxAmount
    {
        get => Tax;
        set => Tax = value;
    }

    /// <summary>Alias for Total for service compatibility</summary>
    public decimal TotalAmount
    {
        get => Total;
        set => Total = value;
    }

    #endregion
}
