// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.DTOs
{
    /// <summary>
    /// Data Transfer Object for Order entity. Represents all fields exposed via API.
    /// </summary>
    public class OrderDto
    {
        /// <summary>Order unique identifier</summary>
        public int Id { get; set; }

        /// <summary>System-generated unique order number</summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>External reference (customer PO number, etc.)</summary>
        public string? ExternalOrderId { get; set; }

        /// <summary>Customer's purchase order number</summary>
        public string? CustomerPONumber { get; set; }

        /// <summary>Reference number for internal tracking</summary>
        public string? ReferenceNumber { get; set; }

        /// <summary>Order name/description</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Detailed order description</summary>
        public string? Description { get; set; }

        /// <summary>Current order status (enum as int)</summary>
        public int Status { get; set; }

        /// <summary>Type of order transaction (enum as int)</summary>
        public int OrderType { get; set; }

        /// <summary>How the order will be fulfilled (enum as int)</summary>
        public int FulfillmentMethod { get; set; }

        /// <summary>Processing priority (enum as int)</summary>
        public int Priority { get; set; }

        /// <summary>Date order was placed (ISO 8601)</summary>
        public string OrderDate { get; set; } = string.Empty;

        /// <summary>Date order was approved (ISO 8601)</summary>
        public string? ApprovedDate { get; set; }

        /// <summary>Requested delivery date (ISO 8601)</summary>
        public string? RequestedDeliveryDate { get; set; }

        /// <summary>Promised delivery date (ISO 8601)</summary>
        public string? PromisedDeliveryDate { get; set; }

        /// <summary>Actual shipment date (ISO 8601)</summary>
        public string? ShippedDate { get; set; }

        /// <summary>Actual delivery date (ISO 8601)</summary>
        public string? DeliveredDate { get; set; }

        /// <summary>Date order was completed (ISO 8601)</summary>
        public string? CompletedDate { get; set; }

        /// <summary>Date order was cancelled (ISO 8601)</summary>
        public string? CancelledDate { get; set; }

        /// <summary>Contract/service start date (ISO 8601)</summary>
        public string? ContractStartDate { get; set; }

        /// <summary>Contract/service end date (ISO 8601)</summary>
        public string? ContractEndDate { get; set; }

        /// <summary>Sum of line items before discounts and tax</summary>
        public decimal Subtotal { get; set; }

        /// <summary>Total discount amount</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>Discount percentage applied</summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>Reason for discount</summary>
        public string? DiscountReason { get; set; }

        /// <summary>Tax amount</summary>
        public decimal TaxAmount { get; set; }

        /// <summary>Tax rate percentage</summary>
        public decimal TaxRate { get; set; }

        /// <summary>Shipping/freight cost</summary>
        public decimal ShippingAmount { get; set; }

        /// <summary>Handling fees</summary>
        public decimal HandlingAmount { get; set; }

        /// <summary>Total order amount</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Currency code (ISO 4217)</summary>
        public string CurrencyCode { get; set; } = "USD";

        /// <summary>Exchange rate if foreign currency</summary>
        public decimal? ExchangeRate { get; set; }

        /// <summary>Amount in base currency</summary>
        public decimal? BaseCurrencyAmount { get; set; }

        /// <summary>Monthly Recurring Revenue</summary>
        public decimal? MRR { get; set; }

        /// <summary>Annual Recurring Revenue</summary>
        public decimal? ARR { get; set; }

        /// <summary>Total Contract Value</summary>
        public decimal? TCV { get; set; }

        /// <summary>Annual Contract Value</summary>
        public decimal? ACV { get; set; }

        /// <summary>One-time revenue component</summary>
        public decimal? OneTimeRevenue { get; set; }

        /// <summary>Recurring revenue component</summary>
        public decimal? RecurringRevenue { get; set; }

        // Billing Address
        public string? BillingName { get; set; }
        public string? BillingCompany { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string? BillingPhone { get; set; }
        public string? BillingEmail { get; set; }

        // Shipping Address
        public string? ShippingName { get; set; }
        public string? ShippingCompany { get; set; }
        public string? ShippingStreet { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingPostalCode { get; set; }
        public string? ShippingCountry { get; set; }
        public string? ShippingPhone { get; set; }
        public string? ShippingEmail { get; set; }
        public string? ShippingInstructions { get; set; }

        // Shipping Details
        public string? ShippingMethod { get; set; }
        public string? TrackingNumber { get; set; }
        public string? TrackingUrl { get; set; }
        public decimal? ShippingWeight { get; set; }
        public int? PackageCount { get; set; }

        // Payment Information
        public string? PaymentTerms { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal AmountInvoiced { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public bool IsPaid { get; set; }

        // Relationships
        public int? QuoteId { get; set; }
        public int AccountId { get; set; }
        public int? ContactId { get; set; }
        public int? OpportunityId { get; set; }
        public int? OwnerId { get; set; }
        public int? ApprovedById { get; set; }
        public int? ParentOrderId { get; set; }

        /// <summary>Order line items</summary>
        public List<OrderLineItemDto> LineItems { get; set; } = new();

        /// <summary>Related invoice IDs</summary>
        public List<int> InvoiceIds { get; set; } = new();

        /// <summary>Related subscription IDs</summary>
        public List<int> SubscriptionIds { get; set; } = new();

        // Notes & Attachments
        public string? InternalNotes { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? CancellationReason { get; set; }
        public string? TermsAndConditions { get; set; }

        // Workflow Dates
        public string? SubmittedDate { get; set; }
        public string? FulfilledDate { get; set; }

        // Hold Status
        public string? HoldReason { get; set; }
        public string? HoldDate { get; set; }

        // Rejection
        public string? RejectionReason { get; set; }

        // Return Information
        public string? ReturnReason { get; set; }

        // Discount Codes
        public string? DiscountCode { get; set; }
        public string? CouponCode { get; set; }

        // Audit Fields
        public string? Source { get; set; }
        public string? SourceIpAddress { get; set; }

        // Audit
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// DTO for creating a new Order. Only fields allowed on creation.
    /// </summary>
    public class CreateOrderDto
    {
        [Required]
        public int AccountId { get; set; }

        public int? ContactId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public int OrderType { get; set; } = 0;

        public int FulfillmentMethod { get; set; } = 0;
        public int Priority { get; set; } = 1;

        [Required]
        public string OrderDate { get; set; } = string.Empty;

        public string? RequestedDeliveryDate { get; set; }
        public string? PromisedDeliveryDate { get; set; }
        public string? ContractStartDate { get; set; }
        public string? ContractEndDate { get; set; }

        public string? BillingName { get; set; }
        public string? BillingCompany { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string? BillingPhone { get; set; }
        public string? BillingEmail { get; set; }

        public string? ShippingName { get; set; }
        public string? ShippingCompany { get; set; }
        public string? ShippingStreet { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingPostalCode { get; set; }
        public string? ShippingCountry { get; set; }
        public string? ShippingPhone { get; set; }
        public string? ShippingEmail { get; set; }
        public string? ShippingInstructions { get; set; }

        public string? PaymentTerms { get; set; }
        public string? PaymentMethod { get; set; }

        public string? SpecialInstructions { get; set; }
        public string? TermsAndConditions { get; set; }

        public string? DiscountCode { get; set; }
        public string? CouponCode { get; set; }

        public List<CreateOrderLineItemDto> LineItems { get; set; } = new();
    }

    /// <summary>
    /// DTO for updating an existing Order. Only fields allowed to be updated.
    /// </summary>
    public class UpdateOrderDto
    {
        [Required]
        public int Id { get; set; }

        public int? Status { get; set; }
        public int? Priority { get; set; }
        public int? FulfillmentMethod { get; set; }
        public int? OrderType { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ApprovedDate { get; set; }
        public string? RequestedDeliveryDate { get; set; }
        public string? PromisedDeliveryDate { get; set; }
        public string? ShippedDate { get; set; }
        public string? DeliveredDate { get; set; }
        public string? CompletedDate { get; set; }
        public string? CancelledDate { get; set; }
        public string? ContractStartDate { get; set; }
        public string? ContractEndDate { get; set; }

        public string? BillingName { get; set; }
        public string? BillingCompany { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string? BillingPhone { get; set; }
        public string? BillingEmail { get; set; }

        public string? ShippingName { get; set; }
        public string? ShippingCompany { get; set; }
        public string? ShippingStreet { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingPostalCode { get; set; }
        public string? ShippingCountry { get; set; }
        public string? ShippingPhone { get; set; }
        public string? ShippingEmail { get; set; }
        public string? ShippingInstructions { get; set; }

        public string? PaymentTerms { get; set; }
        public string? PaymentMethod { get; set; }

        public string? SpecialInstructions { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? DiscountCode { get; set; }
        public string? CouponCode { get; set; }

        public List<UpdateOrderLineItemDto>? LineItems { get; set; }
    }

    /// <summary>
    /// DTO representing an Order line item in API responses.
    /// </summary>
    public class OrderLineItemDto
    {
        public int Id { get; set; }
        public int LineNumber { get; set; }
        public int? ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public string? ProductCode { get; set; }
        public decimal Quantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal ExtendedAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for adding a line item to an Order.
    /// </summary>
    public class CreateOrderLineItemDto
    {
        [Required]
        public int? ProductId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SKU { get; set; }
        [Required]
        public decimal Quantity { get; set; } = 1;
        public string? UnitOfMeasure { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing Order line item.
    /// </summary>
    public class UpdateOrderLineItemDto
    {
        [Required]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public string? Notes { get; set; }
    }
}
