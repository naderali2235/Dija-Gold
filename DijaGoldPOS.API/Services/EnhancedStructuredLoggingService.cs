using Serilog;
using Serilog.Context;
using DijaGoldPOS.API.IServices;
using System.Text.Json;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Enhanced structured logging service with comprehensive context and correlation
/// </summary>
public class EnhancedStructuredLoggingService : IEnhancedStructuredLoggingService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EnhancedStructuredLoggingService(
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Log an exception with full context
    /// </summary>
    public async Task LogExceptionAsync(
        Exception exception,
        string path,
        string method,
        string userId,
        string correlationId,
        Dictionary<string, object>? additionalContext = null)
    {
        var context = await BuildLogContextAsync();
        context["Path"] = path;
        context["Method"] = method;
        context["UserId"] = userId;
        context["CorrelationId"] = correlationId;
        context["ExceptionType"] = exception.GetType().Name;
        context["ExceptionSource"] = exception.Source ?? "Unknown";

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                context[item.Key] = item.Value;
            }
        }

        // Add inner exception details
        if (exception.InnerException != null)
        {
            context["InnerExceptionType"] = exception.InnerException.GetType().Name;
            context["InnerExceptionMessage"] = exception.InnerException.Message;
        }

        // Add stack trace for critical errors
        if (IsCriticalException(exception))
        {
            context["StackTrace"] = exception.StackTrace;
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            var logLevel = DetermineLogLevel(exception);
            Log.Write(logLevel, exception, 
                "Exception occurred: {ExceptionType} in {Method} {Path} | User: {UserId} | Correlation: {CorrelationId} | Message: {Message}",
                exception.GetType().Name, method, path, userId, correlationId, exception.Message);
        }
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
        var context = await BuildLogContextAsync();
        context["Operation"] = operation;
        context["EntityType"] = entityType;
        context["EntityId"] = entityId ?? "Unknown";
        
        if (data != null)
        {
            context["Data"] = SanitizeLogData(data);
        }

        if (!string.IsNullOrEmpty(description))
        {
            context["Description"] = description;
        }

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                context[item.Key] = item.Value;
            }
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Information("Business Operation: {Operation} | Entity: {EntityType}({EntityId}) | User: {UserId}",
                operation, entityType, entityId, _currentUserService.UserId);
        }
    }

    /// <summary>
    /// Log a business operation with context (enhanced version)
    /// </summary>
    public async Task LogBusinessOperationAsync(
        string operation,
        string entityType,
        object? entityId,
        string action,
        Dictionary<string, object>? additionalData = null)
    {
        var context = await BuildLogContextAsync();
        context["Operation"] = operation;
        context["EntityType"] = entityType;
        context["EntityId"] = entityId?.ToString() ?? "Unknown";
        context["Action"] = action;

        if (additionalData != null)
        {
            foreach (var item in additionalData)
            {
                context[item.Key] = item.Value;
            }
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Information("Business Operation: {Operation} | Entity: {EntityType}({EntityId}) | Action: {Action} | User: {UserId}",
                operation, entityType, entityId, action, _currentUserService.UserId);
        }
    }

    /// <summary>
    /// Log a performance metric
    /// </summary>
    public async Task LogPerformanceAsync(
        string operation,
        TimeSpan duration,
        Dictionary<string, object>? metrics = null)
    {
        var context = await BuildLogContextAsync();
        context["Operation"] = operation;
        context["Duration"] = duration.TotalMilliseconds;
        context["DurationFormatted"] = $"{duration.TotalMilliseconds:F2}ms";

        if (metrics != null)
        {
            foreach (var metric in metrics)
            {
                context[metric.Key] = metric.Value;
            }
        }

        var logLevel = duration.TotalMilliseconds > 5000 ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information;

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Write(logLevel, "Performance: {Operation} completed in {Duration:F2}ms | User: {UserId}",
                operation, duration.TotalMilliseconds, _currentUserService.UserId);
        }
    }

    /// <summary>
    /// Log a security event
    /// </summary>
    public async Task LogSecurityEventAsync(
        string eventType,
        string description,
        Dictionary<string, object>? securityContext = null)
    {
        var context = await BuildLogContextAsync();
        context["EventType"] = eventType;
        context["Description"] = description;
        context["SecurityEvent"] = true;

        if (securityContext != null)
        {
            foreach (var item in securityContext)
            {
                context[item.Key] = item.Value;
            }
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Warning("Security Event: {EventType} | {Description} | User: {UserId} | IP: {ClientIP}",
                eventType, description, _currentUserService.UserId, GetClientIpAddress());
        }
    }

    /// <summary>
    /// Log a data change event
    /// </summary>
    public async Task LogDataChangeAsync(
        string entityType,
        object entityId,
        string changeType,
        object? oldValues = null,
        object? newValues = null)
    {
        var context = await BuildLogContextAsync();
        context["EntityType"] = entityType;
        context["EntityId"] = entityId.ToString() ?? "Unknown";
        context["ChangeType"] = changeType;

        if (oldValues != null)
        {
            context["OldValues"] = SanitizeLogData(oldValues);
        }

        if (newValues != null)
        {
            context["NewValues"] = SanitizeLogData(newValues);
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Information("Data Change: {ChangeType} {EntityType}({EntityId}) | User: {UserId}",
                changeType, entityType, entityId, _currentUserService.UserId);
        }
    }

    /// <summary>
    /// Log an integration event (external API calls, etc.)
    /// </summary>
    public async Task LogIntegrationEventAsync(
        string integration,
        string operation,
        bool success,
        TimeSpan? duration = null,
        Dictionary<string, object>? integrationData = null)
    {
        var context = await BuildLogContextAsync();
        context["Integration"] = integration;
        context["Operation"] = operation;
        context["Success"] = success;
        context["IntegrationType"] = "External";

        if (duration.HasValue)
        {
            context["Duration"] = duration.Value.TotalMilliseconds;
        }

        if (integrationData != null)
        {
            foreach (var item in integrationData)
            {
                context[item.Key] = item.Value;
            }
        }

        var logLevel = success ? Serilog.Events.LogEventLevel.Information : Serilog.Events.LogEventLevel.Warning;

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Write(logLevel, "Integration: {Integration} | Operation: {Operation} | Success: {Success} | Duration: {Duration}ms",
                integration, operation, success, duration?.TotalMilliseconds ?? 0);
        }
    }

    /// <summary>
    /// Log a custom event with structured data
    /// </summary>
    public async Task LogCustomEventAsync(
        string eventName,
        Serilog.Events.LogEventLevel level,
        string message,
        Dictionary<string, object>? eventData = null)
    {
        var context = await BuildLogContextAsync();
        context["EventName"] = eventName;
        context["CustomEvent"] = true;

        if (eventData != null)
        {
            foreach (var item in eventData)
            {
                context[item.Key] = item.Value;
            }
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Write(level, "Custom Event: {EventName} | {Message} | User: {UserId}",
                eventName, message, _currentUserService.UserId);
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
        var context = await BuildLogContextAsync();
        context["Operation"] = operation;
        context["EntityType"] = entityType;
        context["EntityId"] = entityId;

        if (oldValues != null)
        {
            context["OldValues"] = SanitizeLogData(oldValues);
        }

        if (newValues != null)
        {
            context["NewValues"] = SanitizeLogData(newValues);
        }

        if (!string.IsNullOrEmpty(description))
        {
            context["Description"] = description;
        }

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                context[item.Key] = item.Value;
            }
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Information("Data Modification: {Operation} {EntityType}({EntityId}) | User: {UserId}",
                operation, entityType, entityId, _currentUserService.UserId);
        }
    }

    /// <summary>
    /// Log a performance metric
    /// </summary>
    public void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object>? additionalContext = null)
    {
        var context = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["Duration"] = duration.TotalMilliseconds,
            ["DurationFormatted"] = $"{duration.TotalMilliseconds:F2}ms",
            ["Timestamp"] = DateTime.UtcNow,
            ["UserId"] = _currentUserService.UserId
        };

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                context[item.Key] = item.Value;
            }
        }

        var logLevel = duration.TotalMilliseconds > 5000 ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information;

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Write(logLevel, "Performance: {Operation} completed in {Duration:F2}ms | User: {UserId}",
                operation, duration.TotalMilliseconds, _currentUserService.UserId);
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
        var context = await BuildLogContextAsync();
        context["Operation"] = operation;
        context["ExceptionType"] = exception.GetType().Name;
        context["ExceptionSource"] = exception.Source ?? "Unknown";

        if (!string.IsNullOrEmpty(entityType))
        {
            context["EntityType"] = entityType;
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            context["EntityId"] = entityId;
        }

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                context[item.Key] = item.Value;
            }
        }

        // Add inner exception details
        if (exception.InnerException != null)
        {
            context["InnerExceptionType"] = exception.InnerException.GetType().Name;
            context["InnerExceptionMessage"] = exception.InnerException.Message;
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            var logLevel = DetermineLogLevel(exception);
            Log.Write(logLevel, exception,
                "Error in {Operation}: {ExceptionType} | Entity: {EntityType}({EntityId}) | User: {UserId} | Message: {Message}",
                operation, exception.GetType().Name, entityType, entityId, _currentUserService.UserId, exception.Message);
        }
    }

    /// <summary>
    /// Log a warning with structured context
    /// </summary>
    public void LogWarning(string message, Dictionary<string, object>? additionalContext = null)
    {
        var context = new Dictionary<string, object>
        {
            ["Message"] = message,
            ["Timestamp"] = DateTime.UtcNow,
            ["UserId"] = _currentUserService.UserId
        };

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                context[item.Key] = item.Value;
            }
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Warning("Warning: {Message} | User: {UserId}", message, _currentUserService.UserId);
        }
    }

    /// <summary>
    /// Log an informational message with structured context
    /// </summary>
    public void LogInformation(string message, Dictionary<string, object>? additionalContext = null)
    {
        var context = new Dictionary<string, object>
        {
            ["Message"] = message,
            ["Timestamp"] = DateTime.UtcNow,
            ["UserId"] = _currentUserService.UserId
        };

        if (additionalContext != null)
        {
            foreach (var item in additionalContext)
            {
                context[item.Key] = item.Value;
            }
        }

        using (LogContext.PushProperty("Context", context, true))
        {
            Log.Information("Information: {Message} | User: {UserId}", message, _currentUserService.UserId);
        }
    }

    /// <summary>
    /// Create a performance timer for operations
    /// </summary>
    public IDisposable BeginPerformanceTimer(string operation, Dictionary<string, object>? additionalContext = null)
    {
        return new PerformanceTimer(operation, this, additionalContext);
    }

    /// <summary>
    /// Performance timer implementation
    /// </summary>
    private class PerformanceTimer : IDisposable
    {
        private readonly string _operation;
        private readonly EnhancedStructuredLoggingService _loggingService;
        private readonly Dictionary<string, object>? _additionalContext;
        private readonly System.Diagnostics.Stopwatch _stopwatch;
        private bool _disposed = false;

        public PerformanceTimer(string operation, EnhancedStructuredLoggingService loggingService, Dictionary<string, object>? additionalContext)
        {
            _operation = operation;
            _loggingService = loggingService;
            _additionalContext = additionalContext;
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                _loggingService.LogPerformance(_operation, _stopwatch.Elapsed, _additionalContext);
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Build comprehensive log context
    /// </summary>
    private async Task<Dictionary<string, object>> BuildLogContextAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var context = new Dictionary<string, object>
        {
            ["Timestamp"] = DateTime.UtcNow,
            ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            ["MachineName"] = Environment.MachineName,
            ["ProcessId"] = Environment.ProcessId,
            ["ThreadId"] = Environment.CurrentManagedThreadId
        };

        // User context
        if (_currentUserService != null)
        {
            context["UserId"] = _currentUserService.UserId ?? "Anonymous";
            context["UserName"] = _currentUserService.UserName ?? "Anonymous";
            context["BranchId"] = _currentUserService.BranchId?.ToString() ?? "Unknown";
            context["BranchName"] = _currentUserService.BranchName ?? "Unknown";
        }

        // HTTP context
        if (httpContext != null)
        {
            context["RequestId"] = httpContext.TraceIdentifier;
            context["CorrelationId"] = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? httpContext.TraceIdentifier;
            context["ClientIP"] = GetClientIpAddress();
            context["UserAgent"] = httpContext.Request.Headers["User-Agent"].ToString();
            context["RequestPath"] = httpContext.Request.Path.Value ?? "";
            context["RequestMethod"] = httpContext.Request.Method;
            context["RequestScheme"] = httpContext.Request.Scheme;
            context["RequestHost"] = httpContext.Request.Host.Value;
            context["RequestContentType"] = httpContext.Request.ContentType ?? "";
            
            // Session information
            if (httpContext.Session != null)
            {
                context["SessionId"] = httpContext.Session.Id;
            }
        }

        return context;
    }

    /// <summary>
    /// Determine log level based on exception type
    /// </summary>
    private static Serilog.Events.LogEventLevel DetermineLogLevel(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => Serilog.Events.LogEventLevel.Warning,
            ArgumentException => Serilog.Events.LogEventLevel.Warning,
            InvalidOperationException => Serilog.Events.LogEventLevel.Warning,
            UnauthorizedAccessException => Serilog.Events.LogEventLevel.Warning,
            KeyNotFoundException => Serilog.Events.LogEventLevel.Information,
            TimeoutException => Serilog.Events.LogEventLevel.Warning,
            NotImplementedException => Serilog.Events.LogEventLevel.Error,
            OutOfMemoryException => Serilog.Events.LogEventLevel.Fatal,
            StackOverflowException => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Error
        };
    }

    /// <summary>
    /// Check if exception is critical and requires full stack trace
    /// </summary>
    private static bool IsCriticalException(Exception exception)
    {
        return exception is OutOfMemoryException ||
               exception is StackOverflowException ||
               exception is AccessViolationException ||
               exception is AppDomainUnloadedException ||
               exception is BadImageFormatException ||
               exception is InvalidProgramException;
    }

    /// <summary>
    /// Get client IP address from HTTP context
    /// </summary>
    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        // Check for forwarded IP first (common in load balancer scenarios)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        // Check for real IP (common in proxy scenarios)
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP address
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Sanitize log data to remove sensitive information
    /// </summary>
    private object SanitizeLogData(object data)
    {
        if (data == null) return "null";

        try
        {
            var json = JsonSerializer.Serialize(data);
            
            // Remove sensitive fields
            var sensitiveFields = new[] { "password", "token", "secret", "key", "pin", "ssn", "nationalid", "creditcard" };
            
            foreach (var field in sensitiveFields)
            {
                json = System.Text.RegularExpressions.Regex.Replace(
                    json, 
                    $"\"{field}\"\\s*:\\s*\"[^\"]*\"", 
                    $"\"{field}\": \"***\"", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return JsonSerializer.Deserialize<object>(json) ?? data;
        }
        catch
        {
            return "[Unable to sanitize data]";
        }
    }
}
