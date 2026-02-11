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
