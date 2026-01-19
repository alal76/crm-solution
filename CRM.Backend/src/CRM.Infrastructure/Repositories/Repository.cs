using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;

namespace CRM.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly CrmDbContext _context;

    public Repository(CrmDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Task.FromResult(_context.Set<T>().Where(e => !e.IsDeleted).ToList());
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
    {
        return await Task.FromResult(_context.Set<T>().Where(predicate).ToList());
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
