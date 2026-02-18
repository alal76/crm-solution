// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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
    public class PricingService : IPricingService
    {
        private readonly ICrmDbContext _context;
        private readonly ILogger<PricingService> _logger;

        public PricingService(ICrmDbContext context, ILogger<PricingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PriceBook>> GetAllPriceBooksAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PriceBooks
                .Include(pb => pb.Entries)
                .Where(pb => !pb.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<PriceBook?> GetPriceBookByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PriceBooks
                .Include(pb => pb.Entries)
                .FirstOrDefaultAsync(pb => pb.Id == id && !pb.IsDeleted, cancellationToken);
        }

        public async Task<PriceBook> CreatePriceBookAsync(PriceBook priceBook, CancellationToken cancellationToken = default)
        {
            priceBook.CreatedAt = DateTime.UtcNow;
            _context.PriceBooks.Add(priceBook);
            await _context.SaveChangesAsync(cancellationToken);
            return priceBook;
        }

        public async Task<PriceBook> UpdatePriceBookAsync(PriceBook priceBook, CancellationToken cancellationToken = default)
        {
            priceBook.UpdatedAt = DateTime.UtcNow;
            _context.PriceBooks.Update(priceBook);
            await _context.SaveChangesAsync(cancellationToken);
            return priceBook;
        }

        public async Task<bool> DeletePriceBookAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.PriceBooks.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<decimal> CalculatePriceAsync(int productId, int? priceBookId = null, int quantity = 1, CancellationToken cancellationToken = default)
        {
            // Minimal pricing logic: prefer pricebook entry unit price, fall back to product list price
            PriceBookEntry? entry = null;
            if (priceBookId.HasValue)
            {
                entry = await _context.PriceBookEntries
                    .FirstOrDefaultAsync(e => e.ProductId == productId && e.PriceBookId == priceBookId && !e.IsDeleted, cancellationToken);
            }

            if (entry != null)
            {
                return entry.UnitPrice * quantity;
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);
            if (product != null)
            {
                return product.Price * quantity;
            }

            _logger.LogWarning("CalculatePriceAsync: product {ProductId} not found", productId);
            return 0m;
        }
    }
}
