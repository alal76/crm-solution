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
    public interface IProductBundleService
    {
        Task<IEnumerable<ProductBundle>> GetAllBundlesAsync(CancellationToken cancellationToken = default);
        Task<ProductBundle?> GetBundleByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductBundle> CreateBundleAsync(ProductBundle bundle, CancellationToken cancellationToken = default);
        Task<ProductBundle> UpdateBundleAsync(ProductBundle bundle, CancellationToken cancellationToken = default);
        Task<bool> DeleteBundleAsync(int id, CancellationToken cancellationToken = default);

        Task<decimal> CalculateBundlePriceAsync(int bundleId, CancellationToken cancellationToken = default);
    }
}
