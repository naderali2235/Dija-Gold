using Serilog.Events;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Enhanced structured logging service interface with comprehensive logging capabilities
/// </summary>
public interface IEnhancedStructuredLoggingService : IStructuredLoggingService
{
    /// <summary>
    /// Log a business operation with context
    /// </summary>
    Task LogBusinessOperationAsync(
        string operation,
        string entityType,
        object? entityId,
        string action,
        Dictionary<string, object>? additionalData = null);

    /// <summary>
    /// Log a performance metric
    /// </summary>
    Task LogPerformanceAsync(
        string operation,
        TimeSpan duration,
        Dictionary<string, object>? metrics = null);

    /// <summary>
    /// Log a security event
    /// </summary>
    new Task LogSecurityEventAsync(
        string eventType,
        string description,
        Dictionary<string, object>? securityContext = null);

    /// <summary>
    /// Log a data change event
    /// </summary>
    Task LogDataChangeAsync(
        string entityType,
        object entityId,
        string changeType,
        object? oldValues = null,
        object? newValues = null);

    /// <summary>
    /// Log an integration event (external API calls, etc.)
    /// </summary>
    Task LogIntegrationEventAsync(
        string integration,
        string operation,
        bool success,
        TimeSpan? duration = null,
        Dictionary<string, object>? integrationData = null);

    /// <summary>
    /// Log a custom event with structured data
    /// </summary>
    Task LogCustomEventAsync(
        string eventName,
        LogEventLevel level,
        string message,
        Dictionary<string, object>? eventData = null);
}
