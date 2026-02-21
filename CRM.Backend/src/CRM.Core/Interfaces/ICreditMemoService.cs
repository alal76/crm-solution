// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces
{
    public interface ICreditMemoService
    {
        // CRUD
        Task<IEnumerable<CreditMemo>> GetAllAsync(int? accountId = null, CreditMemoStatus? status = null, CancellationToken cancellationToken = default);
        Task<CreditMemo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CreditMemo?> GetByCreditMemoNumberAsync(string creditMemoNumber, CancellationToken cancellationToken = default);
        Task<CreditMemo> CreateAsync(CreditMemo creditMemo, CancellationToken cancellationToken = default);
        Task<CreditMemo> UpdateAsync(CreditMemo creditMemo, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Domain operations
        Task<CreditMemo> CreateFromInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
        Task<string> GenerateCreditMemoNumberAsync(CancellationToken cancellationToken = default);
        Task<CreditMemo> ApplyAsync(int creditMemoId, int invoiceId, CancellationToken cancellationToken = default);
        Task<CreditMemo> UnapplyAsync(int creditMemoId, CancellationToken cancellationToken = default);
        Task<CreditMemo> RefundAsync(int creditMemoId, CancellationToken cancellationToken = default);

        // Line items
        Task<CreditMemoLineItem> AddLineItemAsync(int creditMemoId, CreditMemoLineItem lineItem, CancellationToken cancellationToken = default);
        Task<CreditMemoLineItem> UpdateLineItemAsync(CreditMemoLineItem lineItem, CancellationToken cancellationToken = default);
        Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CreditMemoLineItem>> GetLineItemsAsync(int creditMemoId, CancellationToken cancellationToken = default);
    }
}
