using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Text;
using FluentValidation;
using Microsoft.OpenApi.Models;
using System.Reflection;
using AutoMapper;
using DijaGoldPOS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with enrichment and request logging support
// Retention: if audits are sufficient, keep only today's log and erase prior days
var eraseDailyIfAuditsSufficient = builder.Configuration.GetValue<bool>("Logging:EraseDailyIfAuditsSufficient");
var configuredRetainedFiles = builder.Configuration.GetValue<int?>("Logging:RetainedFileCountLimit");
var retainedFiles = eraseDailyIfAuditsSufficient ? 1 : (configuredRetainedFiles ?? 10);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/dijaGoldPOS-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Information,
        retainedFileCountLimit: retainedFiles)
    .CreateLogger();

builder.Host.UseSerilog();

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
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add HTTP context accessor for audit service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Repository Pattern and Unit of Work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IGoldRateRepository, GoldRateRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add business services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<ILabelPrintingService, LabelPrintingService>();

builder.Services.AddControllers();


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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dija Gold POS API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at application root
    });
}

// Global exception handling - must be early in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Correlation Id and Serilog request logging
app.Use(async (context, next) =>
{
    var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
    // propagate correlation id header
    const string headerName = "X-Correlation-Id";
    if (!context.Request.Headers.TryGetValue(headerName, out var existing))
    {
        context.Request.Headers[headerName] = traceId;
    }
    context.Response.Headers[headerName] = context.Request.Headers[headerName].ToString();

    using (LogContext.PushProperty("CorrelationId", traceId))
    using (LogContext.PushProperty("UserName", context.User?.Identity?.IsAuthenticated == true ? context.User.Identity?.Name : "anonymous"))
    using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress?.ToString()))
    using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagCtx, httpContext) =>
    {
        diagCtx.Set("CorrelationId", System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier);
        diagCtx.Set("UserName", httpContext.User?.Identity?.IsAuthenticated == true ? httpContext.User.Identity?.Name : "anonymous");
        diagCtx.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        diagCtx.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagCtx.Set("RequestHost", httpContext.Request.Host.Value);
        diagCtx.Set("RequestScheme", httpContext.Request.Scheme);
        diagCtx.Set("RequestPath", httpContext.Request.Path);
        diagCtx.Set("RequestMethod", httpContext.Request.Method);
        diagCtx.Set("ResponseStatusCode", httpContext.Response.StatusCode);
    };
});

app.UseCors("ConfiguredCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await DbInitializer.InitializeAsync(context, userManager, roleManager);
}

app.Run();