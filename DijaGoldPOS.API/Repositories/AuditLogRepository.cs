using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for AuditLog operations
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    protected readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(long id)
    {
        return await _context.AuditLogs.FindAsync(id);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs.ToListAsync();
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        return auditLog;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Include(a => a.Branch)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Include(a => a.Branch)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Include(a => a.Branch)
            .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Include(a => a.Branch)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.AuditLogs.CountAsync();
    }

    public async Task<int> GetErrorCountAsync()
    {
        return await _context.AuditLogs
            .CountAsync(a => !string.IsNullOrEmpty(a.ErrorMessage));
    }
}
