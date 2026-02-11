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

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for invoice management operations.
/// Handles invoice lifecycle from creation to payment reconciliation.
/// </summary>
public interface IInvoiceService
{
    #region CRUD Operations

    /// <summary>Gets all invoices with optional filtering.</summary>
    Task<IEnumerable<Invoice>> GetAllAsync(
        int? customerId = null,
        InvoiceStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an invoice by ID.</summary>
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets an invoice by invoice number.</summary>
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);

    /// <summary>Creates a new invoice.</summary>
    Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing invoice.</summary>
    Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>Deletes an invoice (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Invoice Operations

    /// <summary>Creates an invoice from an order.</summary>
    Task<Invoice> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Creates an invoice from a quote.</summary>
    Task<Invoice> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default);

    /// <summary>Generates the next invoice number.</summary>
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>Sends an invoice to the customer.</summary>
    Task<bool> SendInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Marks an invoice as viewed.</summary>
    Task<bool> MarkAsViewedAsync(int invoiceId, CancellationToken cancellationToken = default);

    #endregion

    #region Status Management

    /// <summary>Updates invoice status.</summary>
    Task<Invoice> UpdateStatusAsync(int invoiceId, InvoiceStatus status, CancellationToken cancellationToken = default);

    /// <summary>Approves a draft invoice.</summary>
    Task<Invoice> ApproveAsync(int invoiceId, int approvedById, CancellationToken cancellationToken = default);

    /// <summary>Voids an invoice.</summary>
    Task<Invoice> VoidAsync(int invoiceId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Marks invoice as paid.</summary>
    Task<Invoice> MarkAsPaidAsync(int invoiceId, CancellationToken cancellationToken = default);

    #endregion

    #region Payment Operations

    /// <summary>Records a payment against an invoice.</summary>
    Task<Invoice> RecordPaymentAsync(int invoiceId, decimal amount, PaymentMethod method, CancellationToken cancellationToken = default);

    /// <summary>Gets the outstanding balance for an invoice.</summary>
    Task<decimal> GetOutstandingBalanceAsync(int invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Gets all payments for an invoice.</summary>
    Task<IEnumerable<Payment>> GetPaymentsAsync(int invoiceId, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets overdue invoices.</summary>
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(int? daysPastDue = null, CancellationToken cancellationToken = default);

    /// <summary>Gets invoices due within a date range.</summary>
    Task<IEnumerable<Invoice>> GetInvoicesDueAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Gets invoice statistics for a customer.</summary>
    Task<InvoiceStatistics> GetCustomerStatisticsAsync(int customerId, CancellationToken cancellationToken = default);

    #endregion

    #region Line Items

    /// <summary>Adds a line item to an invoice.</summary>
    Task<InvoiceLineItem> AddLineItemAsync(int invoiceId, InvoiceLineItem lineItem, CancellationToken cancellationToken = default);

    /// <summary>Updates a line item.</summary>
    Task<InvoiceLineItem> UpdateLineItemAsync(InvoiceLineItem lineItem, CancellationToken cancellationToken = default);

    /// <summary>Removes a line item.</summary>
    Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default);

    /// <summary>Gets all line items for an invoice.</summary>
    Task<IEnumerable<InvoiceLineItem>> GetLineItemsAsync(int invoiceId, CancellationToken cancellationToken = default);

    #endregion

    #region Calculations

    /// <summary>Recalculates invoice totals.</summary>
    Task<Invoice> RecalculateTotalsAsync(int invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Applies a discount to an invoice.</summary>
    Task<Invoice> ApplyDiscountAsync(int invoiceId, decimal discountAmount, string? discountCode = null, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Invoice statistics for reporting.
/// </summary>
public class InvoiceStatistics
{
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public int DraftInvoices { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal AverageInvoiceAmount { get; set; }
    public double AverageDaysToPayment { get; set; }
}
