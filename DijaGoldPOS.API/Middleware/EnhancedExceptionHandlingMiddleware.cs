using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Shared;
using System.Security;
using System.Data.Common;
using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Middleware;

/// <summary>
/// Enhanced exception handling middleware with comprehensive error logging and user-friendly responses
/// </summary>
public sealed class EnhancedExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDiagnosticContext? _diagnosticContext;
    private readonly IHostEnvironment _environment;
    private readonly IStructuredLoggingService? _loggingService;

    public EnhancedExceptionHandlingMiddleware(
        RequestDelegate next,
        IDiagnosticContext? diagnosticContext,
        IHostEnvironment environment,
        IStructuredLoggingService? loggingService = null)
    {
        _next = next;
        _diagnosticContext = diagnosticContext;
        _environment = environment;
        _loggingService = loggingService;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var errorResponse = CreateErrorResponse(httpContext, exception);
        await LogExceptionAsync(httpContext, exception, errorResponse);
        await WriteErrorResponseAsync(httpContext, errorResponse);
    }

    private ErrorResponse CreateErrorResponse(HttpContext httpContext, Exception exception)
    {
        var traceId = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? traceId;

        return exception switch
        {
            ValidationException validationEx => CreateValidationErrorResponse(httpContext, validationEx, correlationId),
            UnauthorizedAccessException => CreateErrorResponse(httpContext, HttpStatusCode.Unauthorized, "Unauthorized", "You are not authorized to perform this action.", correlationId),
            SecurityException => CreateErrorResponse(httpContext, HttpStatusCode.Forbidden, "Forbidden", "Access to this resource is forbidden.", correlationId),
            KeyNotFoundException => CreateErrorResponse(httpContext, HttpStatusCode.NotFound, "Resource Not Found", "The requested resource was not found.", correlationId),
            ArgumentException argumentEx => CreateArgumentErrorResponse(httpContext, argumentEx, correlationId),
            ArgumentNullException argumentNullEx => CreateArgumentErrorResponse(httpContext, argumentNullEx, correlationId),
            InvalidOperationException invalidOpEx => CreateInvalidOperationErrorResponse(httpContext, invalidOpEx, correlationId),
            DbUpdateException dbUpdateEx => CreateDatabaseErrorResponse(httpContext, dbUpdateEx, correlationId),
            DbException dbEx => CreateDatabaseErrorResponse(httpContext, dbEx, correlationId),
            TimeoutException => CreateErrorResponse(httpContext, HttpStatusCode.RequestTimeout, "Request Timeout", "The request took too long to process. Please try again.", correlationId),
            NotImplementedException => CreateErrorResponse(httpContext, HttpStatusCode.NotImplemented, "Not Implemented", "This feature is not yet implemented.", correlationId),
            NotSupportedException => CreateErrorResponse(httpContext, HttpStatusCode.BadRequest, "Not Supported", "This operation is not supported.", correlationId),
            BusinessRuleException businessEx => CreateBusinessRuleErrorResponse(httpContext, businessEx, correlationId),
            DomainException domainEx => CreateDomainErrorResponse(httpContext, domainEx, correlationId),
            _ => CreateGenericErrorResponse(httpContext, exception, correlationId)
        };
    }

    private ErrorResponse CreateValidationErrorResponse(HttpContext httpContext, ValidationException exception, string correlationId)
    {
        var validationErrors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage).Distinct().ToArray());

        return new ErrorResponse
        {
            Type = "validation_error",
            Title = "Validation Failed",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "One or more validation errors occurred.",
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            ValidationErrors = validationErrors,
            UserFriendlyMessage = "Please check your input and try again."
        };
    }

    private ErrorResponse CreateArgumentErrorResponse(HttpContext httpContext, Exception exception, string correlationId)
    {
        var parameterName = exception is ArgumentException argEx ? argEx.ParamName : null;
        
        return new ErrorResponse
        {
            Type = "argument_error",
            Title = "Invalid Argument",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = _environment.IsDevelopment() ? exception.Message : "One or more arguments are invalid.",
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            UserFriendlyMessage = $"Invalid {parameterName ?? "parameter"} provided. Please check your input.",
            Extensions = _environment.IsDevelopment() && !string.IsNullOrEmpty(parameterName) 
                ? new Dictionary<string, object> { { "parameterName", parameterName } } 
                : null
        };
    }

    private ErrorResponse CreateInvalidOperationErrorResponse(HttpContext httpContext, InvalidOperationException exception, string correlationId)
    {
        var userMessage = DetermineUserFriendlyMessage(exception.Message);
        
        return new ErrorResponse
        {
            Type = "invalid_operation",
            Title = "Invalid Operation",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = _environment.IsDevelopment() ? exception.Message : "The requested operation cannot be performed.",
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            UserFriendlyMessage = userMessage
        };
    }

    private ErrorResponse CreateDatabaseErrorResponse(HttpContext httpContext, Exception exception, string correlationId)
    {
        var (statusCode, title, userMessage) = AnalyzeDatabaseException(exception);

        return new ErrorResponse
        {
            Type = "database_error",
            Title = title,
            Status = (int)statusCode,
            Detail = _environment.IsDevelopment() ? exception.Message : "A database error occurred.",
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            UserFriendlyMessage = userMessage,
            Extensions = _environment.IsDevelopment() ? CreateDatabaseErrorExtensions(exception) : null
        };
    }

    private ErrorResponse CreateBusinessRuleErrorResponse(HttpContext httpContext, BusinessRuleException exception, string correlationId)
    {
        return new ErrorResponse
        {
            Type = "business_rule_violation",
            Title = "Business Rule Violation",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            UserFriendlyMessage = exception.UserFriendlyMessage ?? exception.Message,
            Extensions = new Dictionary<string, object>
            {
                { "ruleCode", exception.RuleCode ?? "UNKNOWN" },
                { "category", exception.Category ?? "General" }
            }
        };
    }

    private ErrorResponse CreateDomainErrorResponse(HttpContext httpContext, DomainException exception, string correlationId)
    {
        return new ErrorResponse
        {
            Type = "domain_error",
            Title = "Domain Error",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            UserFriendlyMessage = exception.UserFriendlyMessage ?? "A business logic error occurred.",
            Extensions = new Dictionary<string, object>
            {
                { "errorCode", exception.ErrorCode ?? "UNKNOWN" },
                { "entityType", exception.EntityType ?? "Unknown" }
            }
        };
    }

    private ErrorResponse CreateGenericErrorResponse(HttpContext httpContext, Exception exception, string correlationId)
    {
        return new ErrorResponse
        {
            Type = "internal_server_error",
            Title = "Internal Server Error",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            UserFriendlyMessage = "We're sorry, something went wrong. Please try again later or contact support if the problem persists.",
            Extensions = _environment.IsDevelopment() ? new Dictionary<string, object>
            {
                { "exceptionType", exception.GetType().Name },
                { "stackTrace", exception.StackTrace ?? "No stack trace available" }
            } : null
        };
    }

    private ErrorResponse CreateErrorResponse(HttpContext httpContext, HttpStatusCode statusCode, string title, string userMessage, string correlationId)
    {
        return new ErrorResponse
        {
            Type = statusCode.ToString().ToLowerInvariant().Replace(" ", "_"),
            Title = title,
            Status = (int)statusCode,
            Detail = userMessage,
            Instance = httpContext.Request.Path,
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow,
            Method = httpContext.Request.Method,
            UserFriendlyMessage = userMessage
        };
    }

    private (HttpStatusCode statusCode, string title, string userMessage) AnalyzeDatabaseException(Exception exception)
    {
        var message = exception.Message.ToLowerInvariant();
        
        if (message.Contains("unique") || message.Contains("duplicate"))
        {
            return (HttpStatusCode.Conflict, "Duplicate Data", "This record already exists. Please check for duplicates.");
        }
        
        if (message.Contains("foreign key") || message.Contains("reference"))
        {
            return (HttpStatusCode.Conflict, "Reference Constraint", "Cannot complete this operation due to related data dependencies.");
        }
        
        if (message.Contains("timeout"))
        {
            return (HttpStatusCode.RequestTimeout, "Database Timeout", "The operation took too long to complete. Please try again.");
        }
        
        if (message.Contains("deadlock"))
        {
            return (HttpStatusCode.Conflict, "Concurrency Issue", "A concurrency conflict occurred. Please try again.");
        }

        return (HttpStatusCode.InternalServerError, "Database Error", "A database error occurred. Please try again or contact support.");
    }

    private Dictionary<string, object>? CreateDatabaseErrorExtensions(Exception exception)
    {
        var extensions = new Dictionary<string, object>();
        
        if (exception is DbUpdateException dbUpdateEx)
        {
            extensions["affectedEntries"] = dbUpdateEx.Entries?.Count() ?? 0;
            if (dbUpdateEx.InnerException != null)
            {
                extensions["innerExceptionType"] = dbUpdateEx.InnerException.GetType().Name;
            }
        }
        
        if (exception is DbException dbEx)
        {
            extensions["errorCode"] = dbEx.ErrorCode;
            if (!string.IsNullOrEmpty(dbEx.SqlState))
            {
                extensions["sqlState"] = dbEx.SqlState;
            }
        }

        return extensions.Any() ? extensions : null;
    }

    private string DetermineUserFriendlyMessage(string exceptionMessage)
    {
        var message = exceptionMessage.ToLowerInvariant();
        
        return message switch
        {
            var m when m.Contains("sequence contains no elements") => "No matching records found.",
            var m when m.Contains("sequence contains more than one element") => "Multiple records found when only one was expected.",
            var m when m.Contains("collection was modified") => "Data was changed during processing. Please refresh and try again.",
            var m when m.Contains("disposed") => "The operation cannot be completed as the resource is no longer available.",
            _ => "The requested operation cannot be performed at this time."
        };
    }

    private async Task LogExceptionAsync(HttpContext httpContext, Exception exception, ErrorResponse errorResponse)
    {
        var logLevel = DetermineLogLevel(exception);
        var userId = httpContext.User?.Identity?.Name ?? "anonymous";
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Use structured logging service if available
        if (_loggingService != null)
        {
            await _loggingService.LogExceptionAsync(
                exception,
                httpContext.Request.Path,
                httpContext.Request.Method,
                userId,
                errorResponse.TraceId,
                new Dictionary<string, object>
                {
                    { "StatusCode", errorResponse.Status },
                    { "ErrorType", errorResponse.Type },
                    { "UserAgent", userAgent },
                    { "IpAddress", ipAddress },
                    { "RequestBody", await GetRequestBodyAsync(httpContext) }
                });
        }
        else
        {
            // Fallback to direct Serilog logging
            using var _ = LogContext.PushProperty("CorrelationId", errorResponse.TraceId);
            using var __ = LogContext.PushProperty("UserId", userId);
            using var ___ = LogContext.PushProperty("UserAgent", userAgent);
            using var ____ = LogContext.PushProperty("IpAddress", ipAddress);

            Log.Write(logLevel, exception,
                "HTTP {Method} {Path} threw {ExceptionType}: {Message} | StatusCode: {StatusCode} | User: {User} | TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.GetType().Name,
                exception.Message,
                errorResponse.Status,
                userId,
                errorResponse.TraceId);
        }

        // Set diagnostic context for Serilog request logging
        _diagnosticContext?.Set("Exception", exception.GetType().Name);
        _diagnosticContext?.Set("ExceptionMessage", exception.Message);
        _diagnosticContext?.Set("ResponseStatusCode", errorResponse.Status);
    }

    private static Serilog.Events.LogEventLevel DetermineLogLevel(Exception exception)
    {
        return exception switch
        {
            ValidationException => Serilog.Events.LogEventLevel.Warning,
            ArgumentException => Serilog.Events.LogEventLevel.Warning,
            KeyNotFoundException => Serilog.Events.LogEventLevel.Information,
            UnauthorizedAccessException => Serilog.Events.LogEventLevel.Warning,
            BusinessRuleException => Serilog.Events.LogEventLevel.Warning,
            DomainException => Serilog.Events.LogEventLevel.Warning,
            _ => Serilog.Events.LogEventLevel.Error
        };
    }

    private async Task<string> GetRequestBodyAsync(HttpContext httpContext)
    {
        try
        {
            httpContext.Request.EnableBuffering();
            httpContext.Request.Body.Position = 0;
            using var reader = new StreamReader(httpContext.Request.Body);
            var body = await reader.ReadToEndAsync();
            httpContext.Request.Body.Position = 0;
            
            // Sanitize sensitive data
            return SanitizeRequestBody(body);
        }
        catch
        {
            return "Unable to read request body";
        }
    }

    private string SanitizeRequestBody(string body)
    {
        if (string.IsNullOrEmpty(body)) return body;

        // Remove sensitive fields (case-insensitive)
        var sensitiveFields = new[] { "password", "token", "secret", "key", "pin", "ssn", "nationalid" };
        
        try
        {
            var jsonDoc = JsonDocument.Parse(body);
            // This is a simplified sanitization - in production you'd want more robust JSON manipulation
            var sanitized = body;
            foreach (var field in sensitiveFields)
            {
                sanitized = System.Text.RegularExpressions.Regex.Replace(
                    sanitized, 
                    $"\"{field}\"\\s*:\\s*\"[^\"]*\"", 
                    $"\"{field}\": \"***\"", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return sanitized;
        }
        catch
        {
            return "Unable to sanitize request body";
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext httpContext, ErrorResponse errorResponse)
    {
        if (httpContext.Response.HasStarted)
        {
            Log.Warning("The response has already started, cannot write error response for TraceId: {TraceId}", errorResponse.TraceId);
            return;
        }

        httpContext.Response.StatusCode = errorResponse.Status;
        httpContext.Response.ContentType = "application/problem+json";
        
        // Add correlation ID to response headers
        if (!httpContext.Response.Headers.ContainsKey("X-Correlation-Id"))
        {
            httpContext.Response.Headers["X-Correlation-Id"] = errorResponse.TraceId;
        }

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        try
        {
            var payload = JsonSerializer.Serialize(errorResponse, serializerOptions);
            await httpContext.Response.WriteAsync(payload);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to write error response for TraceId: {TraceId}", errorResponse.TraceId);
            
            // Fallback to minimal response
            httpContext.Response.StatusCode = 500;
            await httpContext.Response.WriteAsync("{\"error\":\"An error occurred while processing your request.\"}");
        }
    }
}
