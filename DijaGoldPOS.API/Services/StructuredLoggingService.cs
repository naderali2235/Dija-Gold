using DijaGoldPOS.API.IServices;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Security.Claims;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Comprehensive structured logging service that integrates with audit logging
/// </summary>
public class StructuredLoggingService : IStructuredLoggingService
{
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<StructuredLoggingService> _logger;

    public StructuredLoggingService(
        IAuditService auditService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<StructuredLoggingService> logger)
    {
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Log a business operation with structured data
    /// </summary>
    public async Task LogBusinessOperationAsync(
        string operation,
        string entityType,
        string? entityId = null,
        object? data = null,
        string? description = null,
        Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = BuildLogContext(operation, entityType, entityId, additionalContext);

            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("EntityType", entityType))
            using (LogContext.PushProperty("EntityId", entityId))
            {
                _logger.LogInformation("Business operation: {Operation} performed on {EntityType} {EntityId}",
                    operation, entityType, entityId);

                // Create audit log entry
                await _auditService.LogAsync(
                    userId: userId,
                    action: operation,
                    entityType: entityType,
                    entityId: entityId,
                    description: description ?? $"Business operation: {operation} on {entityType}",
                    oldValues: null,
                    newValues: data != null ? System.Text.Json.JsonSerializer.Serialize(data) : null,
                    ipAddress: GetClientIP(),
                    userAgent: GetUserAgent()
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging business operation: {Operation} on {EntityType}", operation, entityType);
        }
    }

    /// <summary>
    /// Log a data modification operation with before/after values
    /// </summary>
    public async Task LogDataModificationAsync(
        string operation,
        string entityType,
        string entityId,
        object? oldValues = null,
        object? newValues = null,
        string? description = null,
        Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = BuildLogContext(operation, entityType, entityId, additionalContext);

            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("EntityType", entityType))
            using (LogContext.PushProperty("EntityId", entityId))
            {
                _logger.LogInformation("Data modification: {Operation} on {EntityType} {EntityId}",
                    operation, entityType, entityId);

                // Create audit log entry with change tracking
                await _auditService.LogAsync(
                    userId: userId,
                    action: operation,
                    entityType: entityType,
                    entityId: entityId,
                    description: description ?? $"Data modification: {operation} on {entityType} {entityId}",
                    oldValues: oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
                    newValues: newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues) : null,
                    ipAddress: GetClientIP(),
                    userAgent: GetUserAgent()
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging data modification: {Operation} on {EntityType} {EntityId}",
                operation, entityType, entityId);
        }
    }

    /// <summary>
    /// Log a security event
    /// </summary>
    public async Task LogSecurityEventAsync(
        string eventType,
        string description,
        Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = BuildLogContext("Security", "SecurityEvent", null, additionalContext);

            using (LogContext.PushProperty("EventType", eventType))
            using (LogContext.PushProperty("Description", description))
            {
                _logger.LogWarning("Security event: {EventType} - {Description}", eventType, description);

                // Create audit log entry for security event
                await _auditService.LogAsync(
                    userId: userId,
                    action: "SECURITY_EVENT",
                    entityType: "Security",
                    entityId: eventType,
                    description: description,
                    ipAddress: GetClientIP(),
                    userAgent: GetUserAgent()
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging security event: {EventType}", eventType);
        }
    }

    /// <summary>
    /// Log a performance metric
    /// </summary>
    public void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            var context = BuildLogContext("Performance", "Metric", null, additionalContext);
            context.Add(("Operation", operation));
            context.Add(("DurationMs", duration.TotalMilliseconds));
            context.Add(("Duration", duration));

            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("DurationMs", duration.TotalMilliseconds))
            using (LogContext.PushProperty("Duration", duration))
            {
                _logger.LogInformation("Performance: {Operation} completed in {DurationMs}ms",
                    operation, duration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging performance metric for: {Operation}", operation);
        }
    }

    /// <summary>
    /// Log an error with structured context
    /// </summary>
    public async Task LogErrorAsync(
        Exception exception,
        string operation,
        string? entityType = null,
        string? entityId = null,
        Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = BuildLogContext("Error", entityType ?? "Unknown", entityId, additionalContext);
            context.Add(("Operation", operation));
            context.Add(("ExceptionType", exception.GetType().Name));
            context.Add(("ExceptionMessage", exception.Message));

            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("EntityType", entityType))
            using (LogContext.PushProperty("EntityId", entityId))
            using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
            using (LogContext.PushProperty("ExceptionMessage", exception.Message))
            {
                _logger.LogError(exception, "Error in operation: {Operation} on {EntityType} {EntityId}",
                    operation, entityType, entityId);

                // Create audit log entry for error
                await _auditService.LogAsync(
                    userId: userId,
                    action: "ERROR",
                    entityType: entityType,
                    entityId: entityId,
                    description: $"Error in {operation}: {exception.Message}",
                    ipAddress: GetClientIP(),
                    userAgent: GetUserAgent(),
                    isSuccess: false,
                    errorMessage: exception.ToString()
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging exception for operation: {Operation}", operation);
        }
    }

    /// <summary>
    /// Log a warning with structured context
    /// </summary>
    public void LogWarning(string message, Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            var context = BuildLogContext("Warning", "Application", null, additionalContext);

            using (LogContext.PushProperty("Message", message))
            using (LogContext.PushProperty("LogLevel", "Warning"))
            {
                _logger.LogWarning(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging warning: {Message}", message);
        }
    }

    /// <summary>
    /// Log an informational message with structured context
    /// </summary>
    public void LogInformation(string message, Dictionary<string, object>? additionalContext = null)
    {
        try
        {
            var context = BuildLogContext("Information", "Application", null, additionalContext);

            using (LogContext.PushProperty("Message", message))
            using (LogContext.PushProperty("LogLevel", "Information"))
            {
                _logger.LogInformation(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging information: {Message}", message);
        }
    }

    /// <summary>
    /// Create a performance timer for operations
    /// </summary>
    public IDisposable BeginPerformanceTimer(string operation, Dictionary<string, object>? additionalContext = null)
    {
        return new PerformanceTimer(this, operation, additionalContext);
    }

    /// <summary>
    /// Build log context from various parameters
    /// </summary>
    private List<(string, object?)> BuildLogContext(
        string operation,
        string? entityType,
        string? entityId,
        Dictionary<string, object>? additionalContext)
    {
        var context = new List<(string, object?)>
        {
            ("Operation", operation),
            ("EntityType", entityType),
            ("EntityId", entityId),
            ("UserId", GetCurrentUserId()),
            ("UserName", GetCurrentUserName()),
            ("ClientIP", GetClientIP()),
            ("UserAgent", GetUserAgent()),
            ("Timestamp", DateTime.UtcNow)
        };

        if (additionalContext != null)
        {
            foreach (var kvp in additionalContext)
            {
                context.Add((kvp.Key, kvp.Value));
            }
        }

        return context;
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private string GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               user?.FindFirst("sub")?.Value ??
               user?.FindFirst("userId")?.Value ??
               "system";
    }

    /// <summary>
    /// Get current user name from claims
    /// </summary>
    private string GetCurrentUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.Identity?.Name ??
               user?.FindFirst(ClaimTypes.Name)?.Value ??
               "anonymous";
    }

    /// <summary>
    /// Get client IP address
    /// </summary>
    private string GetClientIP()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Get user agent string
    /// </summary>
    private string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "unknown";
    }

    /// <summary>
    /// Performance timer helper class
    /// </summary>
    private class PerformanceTimer : IDisposable
    {
        private readonly StructuredLoggingService _loggingService;
        private readonly string _operation;
        private readonly Dictionary<string, object>? _additionalContext;
        private readonly Stopwatch _stopwatch;

        public PerformanceTimer(
            StructuredLoggingService loggingService,
            string operation,
            Dictionary<string, object>? additionalContext)
        {
            _loggingService = loggingService;
            _operation = operation;
            _additionalContext = additionalContext;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _loggingService.LogPerformance(_operation, _stopwatch.Elapsed, _additionalContext);
        }
    }
}
