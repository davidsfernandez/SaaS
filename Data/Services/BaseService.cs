using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data.Interfaces;

namespace SaasAsaasApp.Data.Services;

public class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseService(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        // Global filters (TenantId and IsDeleted) are automatically applied by DbContext
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        // Global filters are applied
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        // TenantId is automatically injected by ApplicationDbContext.SaveChangesAsync
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            // ApplicationDbContext.SaveChangesAsync will handle Soft Delete
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
