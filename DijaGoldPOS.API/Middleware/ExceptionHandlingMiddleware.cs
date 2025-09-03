using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DijaGoldPOS.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDiagnosticContext? _diagnosticContext; // available when UseSerilogRequestLogging is enabled
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IDiagnosticContext? diagnosticContext,
        IHostEnvironment environment)
    {
        _next = next;
        _diagnosticContext = diagnosticContext;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException validationException)
        {
            await HandleValidationExceptionAsync(httpContext, validationException);
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            await HandleExceptionAsync(
                httpContext,
                unauthorizedAccessException,
                statusCode: (int)HttpStatusCode.Forbidden,
                title: "Forbidden");
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            await HandleExceptionAsync(
                httpContext,
                keyNotFoundException,
                statusCode: (int)HttpStatusCode.NotFound,
                title: "Resource not found");
        }
        catch (ArgumentException argumentException)
        {
            await HandleExceptionAsync(
                httpContext,
                argumentException,
                statusCode: (int)HttpStatusCode.BadRequest,
                title: "Invalid argument");
        }
        catch (InvalidOperationException invalidOpException)
        {
            await HandleExceptionAsync(
                httpContext,
                invalidOpException,
                statusCode: (int)HttpStatusCode.BadRequest,
                title: "Invalid operation");
        }
        catch (DbUpdateException dbUpdateException)
        {
            await HandleExceptionAsync(
                httpContext,
                dbUpdateException,
                statusCode: (int)HttpStatusCode.Conflict,
                title: "Database update error");
        }
        catch (NotImplementedException notImplementedException)
        {
            await HandleExceptionAsync(
                httpContext,
                notImplementedException,
                statusCode: (int)HttpStatusCode.NotImplemented,
                title: "Not implemented");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                httpContext,
                ex,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "An unexpected error occurred");
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext httpContext, ValidationException exception)
    {
        var problemDetails = CreateBaseProblemDetails(httpContext,
            statusCode: (int)HttpStatusCode.BadRequest,
            title: "Validation failed");

        var validationErrors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage).Distinct().ToArray());

        problemDetails.Extensions["errors"] = validationErrors;

        LogException(httpContext, exception, problemDetails.Status ?? 400, problemDetails.Title ?? "Validation failed");

        await WriteProblemDetailsAsync(httpContext, problemDetails);
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, int statusCode, string title)
    {
        var problemDetails = CreateBaseProblemDetails(httpContext, statusCode, title);

        if (_environment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        LogException(httpContext, exception, statusCode, title);

        await WriteProblemDetailsAsync(httpContext, problemDetails);
    }

    private static ProblemDetails CreateBaseProblemDetails(HttpContext httpContext, int statusCode, string title)
    {
        var traceId = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Attach correlation id as response header for client tracing
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.Headers["X-Correlation-Id"] = traceId;
        }

        return new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7807",
            Title = title,
            Status = statusCode,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId,
                ["method"] = httpContext.Request.Method
            }
        };
    }

    private async Task WriteProblemDetailsAsync(HttpContext httpContext, ProblemDetails problemDetails)
    {
        if (httpContext.Response.HasStarted)
        {
            Log.Warning("The response has already started, the problem details middleware will not be executed.");
            return;
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        try
        {
            var payload = JsonSerializer.Serialize(problemDetails, serializerOptions);
            await httpContext.Response.WriteAsync(payload);
        }
        catch (ObjectDisposedException ode)
        {
            // Client disconnected or stream closed; log and swallow to avoid crashing the pipeline
            Log.Warning(ode, "Response stream disposed while writing problem details.");
        }
        catch (IOException ioe)
        {
            // Network/connection issue while writing; log and swallow
            Log.Warning(ioe, "I/O error while writing problem details to response.");
        }
    }

    private void LogException(HttpContext httpContext, Exception exception, int statusCode, string title)
    {
        var route = httpContext.Request.Path.Value ?? string.Empty;
        var method = httpContext.Request.Method;
        var user = httpContext.User?.Identity?.IsAuthenticated == true ? httpContext.User.Identity?.Name : "anonymous";
        var traceId = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _diagnosticContext?.Set("ResponseStatusCode", statusCode);

        Log.Error(exception,
            "HTTP {Method} {Route} threw an exception. StatusCode: {StatusCode}, Title: {Title}, User: {User}, TraceId: {TraceId}",
            method, route, statusCode, title, user, traceId);
    }
}


