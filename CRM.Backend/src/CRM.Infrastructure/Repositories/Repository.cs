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
        _context.Set<T>().Update(entity);
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
