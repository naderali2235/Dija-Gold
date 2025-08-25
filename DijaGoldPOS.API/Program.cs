using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Extensions;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Reflection;
using System.Text;
using AutoMapper;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with comprehensive enrichment and structured logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .Enrich.WithCorrelationId()
        .Enrich.WithProperty("Application", "DijaGold POS API")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {SourceContext} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/dijaGoldPOS-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: context.Configuration.GetValue<int>("Logging:RetainedFileCountLimit", 10),
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {MachineName} {EnvironmentUserName} {ThreadId} {ProcessId} {SourceContext} {Message:lj}{NewLine}{Exception}",
            shared: true)
        .WriteTo.Seq(
            serverUrl: context.Configuration["Serilog:WriteTo:2:Args:serverUrl"] ?? "http://localhost:5341",
            apiKey: context.Configuration["Serilog:WriteTo:2:Args:apiKey"],
            restrictedToMinimumLevel: LogEventLevel.Information);
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("CashierOrManager", policy => policy.RequireRole("Cashier", "Manager"));
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Add HTTP context accessor for audit service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add structured logging service
builder.Services.AddScoped<IStructuredLoggingService, StructuredLoggingService>();

// Add core infrastructure (repositories and services)
builder.Services.AddCoreInfrastructure();

// Add lookup infrastructure (repositories and services)
builder.Services.AddLookupInfrastructure();

// Configure JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configure FluentValidation for ASP.NET Core
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();


// Add CORS via configuration
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var exposedHeaders = builder.Configuration.GetSection("Cors:ExposedHeaders").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredCors", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            // Check if we want to allow all origins (development mode)
            if (corsOrigins.Contains("*"))
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        }
        else
        {
            // If no origins configured, allow none explicitly to avoid accidental wide-open CORS
            policy.DisallowCredentials();
        }

        if (exposedHeaders.Length > 0)
        {
            policy.WithExposedHeaders(exposedHeaders);
        }
    });
});


// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dija Gold POS API", Version = "v1" });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register custom health check service
builder.Services.AddScoped<DijaGoldHealthCheck>(sp =>
    new DijaGoldHealthCheck(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        sp.GetRequiredService<ILogger<DijaGoldHealthCheck>>()));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "Database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "database" })
    .AddCheck<DijaGoldHealthCheck>(
        name: "DijaGold Comprehensive Health Check",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "comprehensive", "dijagold" });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dija Gold POS API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Audit logging middleware
app.UseMiddleware<AuditLoggingMiddleware>();

// Enhanced Correlation Id middleware with comprehensive context
app.Use(async (context, next) =>
{
    var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
    const string headerName = "X-Correlation-Id";

    // Set correlation ID in request and response headers
    if (!context.Request.Headers.TryGetValue(headerName, out var existing))
    {
        context.Request.Headers[headerName] = traceId;
    }
    context.Response.Headers[headerName] = context.Request.Headers[headerName].ToString();

    // Get user information
    var userName = context.User?.Identity?.IsAuthenticated == true
        ? context.User.Identity?.Name ?? "unknown"
        : "anonymous";
    var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("userId")?.Value ?? "anonymous";
    var clientIP = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var userAgent = context.Request.Headers["User-Agent"].ToString() ?? "unknown";

    using (LogContext.PushProperty("CorrelationId", traceId))
    using (LogContext.PushProperty("UserId", userId))
    using (LogContext.PushProperty("UserName", userName))
    using (LogContext.PushProperty("ClientIP", clientIP))
    using (LogContext.PushProperty("UserAgent", userAgent))
    using (LogContext.PushProperty("RequestMethod", context.Request.Method))
    using (LogContext.PushProperty("RequestPath", context.Request.Path))
    using (LogContext.PushProperty("RequestHost", context.Request.Host.Value))
    using (LogContext.PushProperty("RequestScheme", context.Request.Scheme))
    using (LogContext.PushProperty("RequestContentType", context.Request.ContentType))
    {
        var startTime = DateTime.UtcNow;
        LogContext.PushProperty("RequestStartTime", startTime);

        await next();

        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;
        LogContext.PushProperty("RequestDuration", duration.TotalMilliseconds);
        LogContext.PushProperty("ResponseStatusCode", context.Response.StatusCode);
    }
});

// Enhanced Serilog request logging with detailed context
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagCtx, httpContext) =>
    {
        var correlationId = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var userName = httpContext.User?.Identity?.IsAuthenticated == true
            ? httpContext.User.Identity?.Name ?? "unknown"
            : "anonymous";
        var userId = httpContext.User?.FindFirst("sub")?.Value ?? httpContext.User?.FindFirst("userId")?.Value ?? "anonymous";

        diagCtx.Set("CorrelationId", correlationId);
        diagCtx.Set("UserId", userId);
        diagCtx.Set("UserName", userName);
        diagCtx.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        diagCtx.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString() ?? "unknown");
        diagCtx.Set("RequestHost", httpContext.Request.Host.Value);
        diagCtx.Set("RequestScheme", httpContext.Request.Scheme);
        diagCtx.Set("RequestPath", httpContext.Request.Path);
        diagCtx.Set("RequestMethod", httpContext.Request.Method);
        diagCtx.Set("RequestContentType", httpContext.Request.ContentType);
        diagCtx.Set("RequestContentLength", httpContext.Request.ContentLength);
        diagCtx.Set("ResponseStatusCode", httpContext.Response.StatusCode);
        diagCtx.Set("ResponseContentType", httpContext.Response.ContentType);
        diagCtx.Set("ResponseContentLength", httpContext.Response.ContentLength);
    };

    // Custom message template for request logging
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    // Health check endpoints are filtered out in the message template by checking the path
});

app.UseCors("ConfiguredCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/api/health");
app.MapHealthChecks("/api/health/database", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});
app.MapHealthChecks("/api/health/comprehensive", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("comprehensive")
});

// Simple health check endpoint
app.MapGet("/api/health/simple", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "DijaGold POS API"
}));

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
   await DbInitializer.InitializeAsync(context, userManager, roleManager);
}

app.Run();
