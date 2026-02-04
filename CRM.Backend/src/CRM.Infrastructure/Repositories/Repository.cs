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
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation.
/// Uses ICrmDbContext for dynamic database resolution (supports demo mode switching).
/// Optimized with AsNoTracking() for read-only operations.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ICrmDbContext _context;

    public Repository(ICrmDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
    {
        return await Task.FromResult(_context.Set<T>()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .ToList());
    }

    public virtual async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        var dbContext = _context as DbContext;
        if (dbContext != null)
        {
            // Check if entity is already being tracked
            var entry = dbContext.ChangeTracker.Entries<T>()
                .FirstOrDefault(e => e.Entity.Id == entity.Id);

            if (entry != null)
            {
                // Entity is already tracked - update its values
                entry.CurrentValues.SetValues(entity);
                entry.State = EntityState.Modified;
            }
            else
            {
                // Entity is not tracked - attach and mark as modified
                _context.Set<T>().Update(entity);
            }
        }
        else
        {
            _context.Set<T>().Update(entity);
        }
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        _context.Set<T>().Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
