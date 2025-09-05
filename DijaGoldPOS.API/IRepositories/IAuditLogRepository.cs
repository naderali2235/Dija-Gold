using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for AuditLog operations
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(long id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<AuditLog> AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
    Task<int> GetTotalCountAsync();
    Task<int> GetErrorCountAsync();
    Task SaveChangesAsync();
}
