namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Interface for structured logging service
/// </summary>
public interface IStructuredLoggingService
{
    /// <summary>
    /// Log a business operation with structured data
    /// </summary>
    Task LogBusinessOperationAsync(
        string operation,
        string entityType,
        string? entityId = null,
        object? data = null,
        string? description = null,
        Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Log a data modification operation with before/after values
    /// </summary>
    Task LogDataModificationAsync(
        string operation,
        string entityType,
        string entityId,
        object? oldValues = null,
        object? newValues = null,
        string? description = null,
        Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Log a security event
    /// </summary>
    Task LogSecurityEventAsync(
        string eventType,
        string description,
        Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Log a performance metric
    /// </summary>
    void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Log an error with structured context
    /// </summary>
    Task LogErrorAsync(
        Exception exception,
        string operation,
        string? entityType = null,
        string? entityId = null,
        Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Log a warning with structured context
    /// </summary>
    void LogWarning(string message, Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Log an informational message with structured context
    /// </summary>
    void LogInformation(string message, Dictionary<string, object>? additionalContext = null);

    /// <summary>
    /// Create a performance timer for operations
    /// </summary>
    IDisposable BeginPerformanceTimer(string operation, Dictionary<string, object>? additionalContext = null);
}
