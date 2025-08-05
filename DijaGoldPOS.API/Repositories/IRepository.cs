using DijaGoldPOS.API.Models;
using System.Linq.Expressions;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>Entity or null if not found</returns>
    Task<T?> GetByIdAsync(int id, params string[] includeProperties);

    /// <summary>
    /// Get entity by ID with tracking disabled (read-only)
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>Entity or null if not found</returns>
    Task<T?> GetByIdAsNoTrackingAsync(int id, params string[] includeProperties);

    /// <summary>
    /// Get all entities
    /// </summary>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>List of entities</returns>
    Task<List<T>> GetAllAsync(params string[] includeProperties);

    /// <summary>
    /// Get all entities with tracking disabled (read-only)
    /// </summary>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>List of entities</returns>
    Task<List<T>> GetAllAsNoTrackingAsync(params string[] includeProperties);

    /// <summary>
    /// Find entities based on predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>List of entities matching the condition</returns>
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties);

    /// <summary>
    /// Find entities based on predicate with tracking disabled (read-only)
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>List of entities matching the condition</returns>
    Task<List<T>> FindAsNoTrackingAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties);

    /// <summary>
    /// Find first entity based on predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>First entity matching the condition or null</returns>
    Task<T?> FindFirstAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties);

    /// <summary>
    /// Find first entity based on predicate with tracking disabled (read-only)
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>First entity matching the condition or null</returns>
    Task<T?> FindFirstAsNoTrackingAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties);

    /// <summary>
    /// Get paged results
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="predicate">Filter condition (optional)</param>
    /// <param name="orderBy">Order by expression (optional)</param>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>Paged results with total count</returns>
    Task<(List<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params string[] includeProperties);

    /// <summary>
    /// Check if entity exists based on predicate
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <returns>True if entity exists</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Count entities based on predicate
    /// </summary>
    /// <param name="predicate">Filter condition (optional)</param>
    /// <returns>Count of entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <returns>Added entity</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    void Update(T entity);

    /// <summary>
    /// Update multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Delete entity (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    void Delete(T entity);

    /// <summary>
    /// Delete entity by ID (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="id">Entity ID</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Delete multiple entities (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="entities">Entities to delete</param>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Permanently delete entity from database
    /// </summary>
    /// <param name="entity">Entity to permanently delete</param>
    void HardDelete(T entity);

    /// <summary>
    /// Permanently delete multiple entities from database
    /// </summary>
    /// <param name="entities">Entities to permanently delete</param>
    void HardDeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Get queryable for advanced queries
    /// </summary>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>IQueryable for the entity</returns>
    IQueryable<T> GetQueryable(params string[] includeProperties);

    /// <summary>
    /// Get queryable with tracking disabled for read-only advanced queries
    /// </summary>
    /// <param name="includeProperties">Properties to include (navigation properties)</param>
    /// <returns>IQueryable for the entity with no tracking</returns>
    IQueryable<T> GetQueryableAsNoTracking(params string[] includeProperties);
}