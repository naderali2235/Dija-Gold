using DijaGoldPOS.API.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DijaGoldPOS.API.Middleware;

/// <summary>
/// Middleware for comprehensive audit logging of all API requests and responses
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    // Endpoints to exclude from detailed audit logging
    private readonly string[] _excludedPaths = {
        "/api/health",
        "/api/health/database",
        "/api/health/comprehensive",
        "/swagger",
        "/favicon.ico"
    };

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = GetCorrelationId(context);
        var requestPath = context.Request.Path.Value ?? "/";
        var requestMethod = context.Request.Method;
        var userAgent = context.Request.Headers["User-Agent"].ToString() ?? "unknown";
        var clientIP = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Skip detailed logging for excluded paths
        var shouldLogDetailed = !IsExcludedPath(requestPath);

        // Capture request body for sensitive operations if configured
        string? requestBody = null;
        if (shouldLogDetailed && ShouldCaptureRequestBody(context))
        {
            requestBody = await CaptureRequestBodyAsync(context);
        }

        // Create audit context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", requestPath))
        using (LogContext.PushProperty("RequestMethod", requestMethod))
        using (LogContext.PushProperty("ClientIP", clientIP))
        using (LogContext.PushProperty("UserAgent", userAgent))
        {
            try
            {
                // Log the incoming request
                if (shouldLogDetailed)
                {
                    _logger.LogInformation("API Request: {Method} {Path} from {ClientIP}",
                        requestMethod, requestPath, clientIP);
                }

                // Capture original response body stream
                var originalResponseBody = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                // Process the request
                await _next(context);

                // Calculate response time
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;
                var statusCode = context.Response.StatusCode;

                // Log the response
                using (LogContext.PushProperty("ResponseStatusCode", statusCode))
                using (LogContext.PushProperty("ResponseDuration", duration.TotalMilliseconds))
                {
                    if (shouldLogDetailed)
                    {
                        // Log based on response status
                        if (statusCode >= 400)
                        {
                            _logger.LogWarning("API Response: {Method} {Path} returned {StatusCode} in {Duration}ms",
                                requestMethod, requestPath, statusCode, duration.TotalMilliseconds);
                        }
                        else
                        {
                            _logger.LogInformation("API Response: {Method} {Path} returned {StatusCode} in {Duration}ms",
                                requestMethod, requestPath, statusCode, duration.TotalMilliseconds);
                        }

                        // Audit log sensitive operations
                        if (IsSensitiveOperation(context))
                        {
                            await LogSensitiveOperationAsync(context, requestBody, statusCode, duration);
                        }
                    }

                    // Performance monitoring - log slow requests
                    if (duration.TotalMilliseconds > 5000) // 5 seconds
                    {
                        _logger.LogWarning("Slow request detected: {Method} {Path} took {Duration}ms",
                            requestMethod, requestPath, duration.TotalMilliseconds);

                        using (var scope = context.RequestServices.CreateScope())
                        {
                            var structuredLoggingService = scope.ServiceProvider.GetRequiredService<IStructuredLoggingService>();
                            await structuredLoggingService.LogSecurityEventAsync(
                                "SLOW_REQUEST",
                                $"Request {requestMethod} {requestPath} took {duration.TotalMilliseconds}ms",
                                new Dictionary<string, object>
                                {
                                    ["Duration"] = duration.TotalMilliseconds,
                                    ["Threshold"] = 5000,
                                    ["StatusCode"] = statusCode
                                });
                        }
                    }
                }

                // Copy response body back to original stream
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBody);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                // Log the exception
                _logger.LogError(ex, "Exception during request: {Method} {Path} after {Duration}ms",
                    requestMethod, requestPath, duration.TotalMilliseconds);

                // Create audit log for the error
                using (var scope = context.RequestServices.CreateScope())
                {
                    var structuredLoggingService = scope.ServiceProvider.GetRequiredService<IStructuredLoggingService>();
                    await structuredLoggingService.LogErrorAsync(
                        ex,
                        $"API_{requestMethod}",
                        "API",
                        requestPath,
                        new Dictionary<string, object>
                        {
                            ["StatusCode"] = 500,
                            ["Duration"] = duration.TotalMilliseconds,
                            ["ClientIP"] = clientIP,
                            ["UserAgent"] = userAgent
                        });
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Get correlation ID from request headers
    /// </summary>
    private string GetCorrelationId(HttpContext context)
    {
        const string headerName = "X-Correlation-Id";
        if (context.Request.Headers.TryGetValue(headerName, out var correlationId))
        {
            return correlationId.ToString();
        }
        return Activity.Current?.Id ?? context.TraceIdentifier;
    }

    /// <summary>
    /// Check if path should be excluded from detailed logging
    /// </summary>
    private bool IsExcludedPath(string path)
    {
        return _excludedPaths.Any(excluded =>
            path.Contains(excluded, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if request body should be captured for this request
    /// </summary>
    private bool ShouldCaptureRequestBody(HttpContext context)
    {
        // Only capture body for POST, PUT, PATCH methods
        if (!new[] { "POST", "PUT", "PATCH" }.Contains(context.Request.Method))
        {
            return false;
        }

        // Only capture for certain content types
        var contentType = context.Request.ContentType;
        return contentType != null &&
               (contentType.Contains("application/json") ||
                contentType.Contains("application/xml") ||
                contentType.Contains("application/x-www-form-urlencoded"));
    }

    /// <summary>
    /// Check if this is a sensitive operation that requires detailed audit logging
    /// </summary>
    private bool IsSensitiveOperation(HttpContext context)
    {
        var path = context.Request.Path.Value;
        var method = context.Request.Method;

        // Financial operations
        if (path?.Contains("/api/financial", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        // Authentication operations
        if (path?.Contains("/api/auth", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        // User management operations
        if (path?.Contains("/api/users", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        // Order operations with POST, PUT, DELETE
        if (path?.Contains("/api/orders", StringComparison.OrdinalIgnoreCase) == true &&
            new[] { "POST", "PUT", "DELETE" }.Contains(method))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Capture request body for audit logging
    /// </summary>
    private async Task<string?> CaptureRequestBodyAsync(HttpContext context)
    {
        try
        {
            if (!context.Request.Body.CanSeek)
            {
                // Enable seeking if the stream supports it
                context.Request.EnableBuffering();
            }

            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Truncate very large bodies to prevent log overflow
            if (body.Length > 10000) // 10KB limit
            {
                return body.Substring(0, 10000) + "... [TRUNCATED]";
            }

            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture request body for audit logging");
            return null;
        }
    }

    /// <summary>
    /// Log sensitive operations with detailed audit information
    /// </summary>
    private async Task LogSensitiveOperationAsync(HttpContext context, string? requestBody, int statusCode, TimeSpan duration)
    {
        try
        {
            var userId = context.User?.FindFirst("sub")?.Value ??
                        context.User?.FindFirst("userId")?.Value ??
                        "anonymous";

            var operation = $"{context.Request.Method}_{context.Request.Path}";
            var description = $"Sensitive API operation: {context.Request.Method} {context.Request.Path}";

            using (var scope = context.RequestServices.CreateScope())
            {
                var structuredLoggingService = scope.ServiceProvider.GetRequiredService<IStructuredLoggingService>();
                await structuredLoggingService.LogBusinessOperationAsync(
                    operation,
                    "API",
                    context.Request.Path,
                    new
                    {
                        StatusCode = statusCode,
                        Duration = duration.TotalMilliseconds,
                        RequestBody = requestBody,
                        ClientIP = context.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = context.Request.Headers["User-Agent"].ToString()
                    },
                    description,
                    new Dictionary<string, object>
                    {
                        ["StatusCode"] = statusCode,
                        ["Duration"] = duration.TotalMilliseconds,
                        ["IsSensitive"] = true
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log sensitive operation for {Path}", context.Request.Path);
        }
    }
}
