namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for audit logging service
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log user action for audit trail
    /// </summary>
    /// <param name="userId">User performing the action</param>
    /// <param name="action">Action performed</param>
    /// <param name="entityType">Entity type affected</param>
    /// <param name="entityId">Entity ID affected</param>
    /// <param name="description">Description of the action</param>
    /// <param name="oldValues">Old values (JSON)</param>
    /// <param name="newValues">New values (JSON)</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="transactionId">Transaction ID if applicable</param>
    /// <param name="isSuccess">Whether the action was successful</param>
    /// <param name="errorMessage">Error message if action failed</param>
    /// <returns>Audit log ID</returns>
    Task<long> LogAsync(
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
        string? errorMessage = null);

    /// <summary>
    /// Log user login
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userName">Username</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent</param>
    /// <param name="isSuccess">Whether login was successful</param>
    /// <param name="errorMessage">Error message if login failed</param>
    /// <returns>Audit log ID</returns>
    Task<long> LogLoginAsync(
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccess = true,
        string? errorMessage = null);

    /// <summary>
    /// Log user logout
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userName">Username</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent</param>
    /// <returns>Audit log ID</returns>
    Task<long> LogLogoutAsync(
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Get audit logs for specific entity
    /// </summary>
    /// <param name="entityType">Entity type</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit logs</returns>
    Task<(List<Models.AuditLog> Logs, int TotalCount)> GetEntityAuditLogsAsync(
        string entityType,
        string entityId,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Get audit logs for specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit logs</returns>
    Task<(List<Models.AuditLog> Logs, int TotalCount)> GetUserAuditLogsAsync(
        string userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Get system audit logs
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="action">Specific action filter</param>
    /// <param name="entityType">Entity type filter</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit logs</returns>
    Task<(List<Models.AuditLog> Logs, int TotalCount)> GetSystemAuditLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? action = null,
        string? entityType = null,
        int pageNumber = 1,
        int pageSize = 50);
}