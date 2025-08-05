using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Generic repository implementation for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    public async Task<T?> GetByIdAsync(int id, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Get entity by ID with tracking disabled (read-only)
    /// </summary>
    public async Task<T?> GetByIdAsNoTrackingAsync(int id, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    public async Task<List<T>> GetAllAsync(params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Get all entities with tracking disabled (read-only)
    /// </summary>
    public async Task<List<T>> GetAllAsNoTrackingAsync(params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Find entities based on predicate
    /// </summary>
    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet.Where(predicate);

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Find entities based on predicate with tracking disabled (read-only)
    /// </summary>
    public async Task<List<T>> FindAsNoTrackingAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Find first entity based on predicate
    /// </summary>
    public async Task<T?> FindFirstAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet.Where(predicate);

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Find first entity based on predicate with tracking disabled (read-only)
    /// </summary>
    public async Task<T?> FindFirstAsNoTrackingAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get paged results
    /// </summary>
    public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Check if entity exists based on predicate
    /// </summary>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <summary>
    /// Count entities based on predicate
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync();
        }

        return await _dbSet.CountAsync(predicate);
    }

    /// <summary>
    /// Add new entity
    /// </summary>
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Add multiple entities
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// Update entity
    /// </summary>
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Update multiple entities
    /// </summary>
    public void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Delete entity (soft delete by setting IsActive = false)
    /// </summary>
    public void Delete(T entity)
    {
        entity.IsActive = false;
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Delete entity by ID (soft delete by setting IsActive = false)
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    /// <summary>
    /// Delete multiple entities (soft delete by setting IsActive = false)
    /// </summary>
    public void DeleteRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsActive = false;
        }
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Permanently delete entity from database
    /// </summary>
    public void HardDelete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Permanently delete multiple entities from database
    /// </summary>
    public void HardDeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Get queryable for advanced queries
    /// </summary>
    public IQueryable<T> GetQueryable(params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return query;
    }

    /// <summary>
    /// Get queryable with tracking disabled for read-only advanced queries
    /// </summary>
    public IQueryable<T> GetQueryableAsNoTracking(params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return query;
    }
}