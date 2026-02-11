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
