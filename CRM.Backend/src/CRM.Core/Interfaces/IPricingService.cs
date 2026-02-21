// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces
{
    public interface IPricingService
    {
        Task<IEnumerable<PriceBook>> GetAllPriceBooksAsync(CancellationToken cancellationToken = default);
        Task<PriceBook?> GetPriceBookByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PriceBook> CreatePriceBookAsync(PriceBook priceBook, CancellationToken cancellationToken = default);
        Task<PriceBook> UpdatePriceBookAsync(PriceBook priceBook, CancellationToken cancellationToken = default);
        Task<bool> DeletePriceBookAsync(int id, CancellationToken cancellationToken = default);

        Task<decimal> CalculatePriceAsync(int productId, int? priceBookId = null, int quantity = 1, CancellationToken cancellationToken = default);
    }
}
