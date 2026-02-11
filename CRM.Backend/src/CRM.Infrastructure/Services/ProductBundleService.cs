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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;

namespace CRM.Infrastructure.Services
{
    public class ProductBundleService : IProductBundleService
    {
        private readonly ICrmDbContext _context;
        private readonly ILogger<ProductBundleService> _logger;

        public ProductBundleService(ICrmDbContext context, ILogger<ProductBundleService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ProductBundle>> GetAllBundlesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductBundles
                .Include(b => b.Items)
                .Where(b => !b.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductBundle?> GetBundleByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.ProductBundles
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        }

        public async Task<ProductBundle> CreateBundleAsync(ProductBundle bundle, CancellationToken cancellationToken = default)
        {
            bundle.CreatedAt = DateTime.UtcNow;
            _context.ProductBundles.Add(bundle);
            await _context.SaveChangesAsync(cancellationToken);
            return bundle;
        }

        public async Task<ProductBundle> UpdateBundleAsync(ProductBundle bundle, CancellationToken cancellationToken = default)
        {
            bundle.UpdatedAt = DateTime.UtcNow;
            _context.ProductBundles.Update(bundle);
            await _context.SaveChangesAsync(cancellationToken);
            return bundle;
        }

        public async Task<bool> DeleteBundleAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProductBundles.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<decimal> CalculateBundlePriceAsync(int bundleId, CancellationToken cancellationToken = default)
        {
            var bundle = await _context.ProductBundles
                .Include(b => b.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(b => b.Id == bundleId && !b.IsDeleted, cancellationToken);

            if (bundle == null)
            {
                _logger.LogWarning("CalculateBundlePriceAsync: bundle {BundleId} not found", bundleId);
                return 0m;
            }

            // For FixedPrice bundles, return the fixed price directly (ignore component sum)
            if (bundle.PricingType == BundlePricingType.FixedPrice && bundle.FixedPrice.HasValue)
            {
                return bundle.FixedPrice.Value;
            }

            // Calculate component sum with item-level discounts
            decimal componentSum = 0m;
            foreach (var item in bundle.Items.Where(i => !i.IsDeleted))
            {
                // Free items contribute nothing
                if (item.IsFree)
                {
                    continue;
                }

                // Use override price if set, otherwise product catalog price
                var unitPrice = item.OverridePrice ?? item.Product?.Price ?? 0m;
                var itemTotal = unitPrice * item.DefaultQuantity;

                // Apply item-level discount
                if (item.DiscountPercent.HasValue && item.DiscountPercent.Value > 0)
                {
                    itemTotal -= itemTotal * (item.DiscountPercent.Value / 100m);
                }

                componentSum += itemTotal;
            }

            decimal finalPrice = componentSum;

            // Apply bundle-level percentage discount
            if (bundle.PricingType == BundlePricingType.PercentDiscount && bundle.DiscountPercent.HasValue && bundle.DiscountPercent.Value > 0)
            {
                finalPrice -= componentSum * (bundle.DiscountPercent.Value / 100m);
            }

            // For Custom type, treat as ComponentSum (no additional rules yet)

            // Enforce MaxDiscountPercent cap: total discount must not exceed this percentage of component sum
            if (bundle.MaxDiscountPercent.HasValue && componentSum > 0)
            {
                var maxDiscount = componentSum * (bundle.MaxDiscountPercent.Value / 100m);
                var actualDiscount = componentSum - finalPrice;
                if (actualDiscount > maxDiscount)
                {
                    finalPrice = componentSum - maxDiscount;
                }
            }

            // Enforce MinimumPrice floor
            if (bundle.MinimumPrice.HasValue && finalPrice < bundle.MinimumPrice.Value)
            {
                finalPrice = bundle.MinimumPrice.Value;
            }

            return Math.Max(finalPrice, 0m);
        }
    }
}
