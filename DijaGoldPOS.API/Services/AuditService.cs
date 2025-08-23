using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Audit logging service implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        ApplicationDbContext context,
        ILogger<AuditService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Log user action for audit trail
    /// </summary>
    public async Task<long> LogAsync(
        string userId,
        string action,
        string? entityType = null,
        string? entityId = null,
        string? description = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        int? branchId = null,
        int? transactionId = null,
        bool isSuccess = true,
        string? errorMessage = null)
    {
        try
        {
            // Get user information
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Audit log attempted for non-existent user {UserId}", userId);
                return 0;
            }

            // Get IP address and user agent from HTTP context if not provided
            var httpContext = _httpContextAccessor.HttpContext;
            ipAddress ??= httpContext?.Connection?.RemoteIpAddress?.ToString();
            userAgent ??= httpContext?.Request?.Headers["User-Agent"].ToString();

            // Create audit log entry
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = user.FullName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description ?? $"User performed {action}",
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                BranchId = branchId ?? user.BranchId,
                FinancialTransactionId = transactionId,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            return auditLog.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for user {UserId}, action {Action}", userId, action);
            return 0;
        }
    }

    /// <summary>
    /// Log user action for audit trail (alias for LogAsync)
    /// </summary>
    public async Task<long> LogActionAsync(
        string userId,
        string action,
        string? entityType = null,
        string? entityId = null,
        string? description = null)
    {
        return await LogAsync(
            userId,
            action,
            entityType,
            entityId,
            description);
    }

    /// <summary>
    /// Log user login
    /// </summary>
    public async Task<long> LogLoginAsync(
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccess = true,
        string? errorMessage = null)
    {
        var description = isSuccess ? 
            $"User {userName} logged in successfully" : 
            $"Failed login attempt for user {userName}";

        return await LogAsync(
            userId,
            "LOGIN",
            description: description,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: isSuccess,
            errorMessage: errorMessage
        );
    }

    /// <summary>
    /// Log user logout
    /// </summary>
    public async Task<long> LogLogoutAsync(
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return await LogAsync(
            userId,
            "LOGOUT",
            description: $"User {userName} logged out",
            ipAddress: ipAddress,
            userAgent: userAgent
        );
    }

    /// <summary>
    /// Get audit logs for specific entity
    /// </summary>
    public async Task<(List<AuditLog> Logs, int TotalCount)> GetEntityAuditLogsAsync(
        string entityType,
        string entityId,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = _context.AuditLogs
            .Include(al => al.User)
            .Where(al => al.EntityType == entityType && al.EntityId == entityId);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(al => al.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }

    /// <summary>
    /// Get audit logs for specific user
    /// </summary>
    public async Task<(List<AuditLog> Logs, int TotalCount)> GetUserAuditLogsAsync(
        string userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = _context.AuditLogs
            .Include(al => al.User)
            .Where(al => al.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(al => al.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.Timestamp <= toDate.Value);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(al => al.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }

    /// <summary>
    /// Get system audit logs
    /// </summary>
    public async Task<(List<AuditLog> Logs, int TotalCount)> GetSystemAuditLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? action = null,
        string? entityType = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = _context.AuditLogs
            .Include(al => al.User)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(al => al.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.Timestamp <= toDate.Value);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(al => al.Action == action);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(al => al.EntityType == entityType);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(al => al.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }
}