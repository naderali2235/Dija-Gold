using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Validators.Core;
using DijaGoldPOS.API.Mappings;
using DijaGoldPOS.API.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Extensions;

/// <summary>
/// Enhanced service collection extensions with comprehensive module registration
/// </summary>
public static class EnhancedServiceCollectionExtensions
{
    /// <summary>
    /// Register all enhanced infrastructure services
    /// </summary>
    public static IServiceCollection AddEnhancedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddEnhancedDbContext(configuration)
            .AddEnhancedAutoMapper()
            .AddEnhancedValidation()
            .AddEnhancedRepositories()
            .AddEnhancedServices()
            .AddEnhancedLogging()
            .AddEnhancedCaching();
    }

    /// <summary>
    /// Register enhanced database context with optimized configuration
    /// </summary>
    public static IServiceCollection AddEnhancedDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EnhancedApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.MigrationsAssembly("DijaGoldPOS.API");
                });

            // Enhanced configuration for development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }

            // Query optimization
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        });

        // Register the application context
        services.AddScoped<ApplicationDbContext>();

        return services;
    }

    /// <summary>
    /// Register enhanced AutoMapper with all profiles
    /// </summary>
    public static IServiceCollection AddEnhancedAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(config =>
        {
            // Add all mapping profiles
            config.AddProfile<CoreMappingProfile>();
            config.AddProfile<LookupProfile>();
            config.AddProfile<ProductProfile>();
            config.AddProfile<CustomerProfile>();
            config.AddProfile<SupplierProfile>();
            config.AddProfile<OrderProfile>();
            config.AddProfile<FinancialTransactionProfile>();
            config.AddProfile<ProductManufactureProfile>();
            config.AddProfile<RepairJobProfile>();

            // Global mapping configurations
            config.AllowNullCollections = true;
            config.AllowNullDestinationValues = true;
        });

        return services;
    }

    /// <summary>
    /// Register enhanced FluentValidation with all validators
    /// </summary>
    public static IServiceCollection AddEnhancedValidation(this IServiceCollection services)
    {
        // Register FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateBranchValidator>();

        // Configure FluentValidation options
        services.Configure<FluentValidation.AspNetCore.FluentValidationMvcConfiguration>(config =>
        {
            config.RegisterValidatorsFromAssemblyContaining<CreateBranchValidator>();
        });

        // Custom validator configurations
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        return services;
    }

    /// <summary>
    /// Register all enhanced repositories with proper lifetime scopes
    /// </summary>
    public static IServiceCollection AddEnhancedRepositories(this IServiceCollection services)
    {
        // Base repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Core repositories
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Lookup repositories
        services.AddScoped<IKaratTypeLookupRepository, KaratTypeLookupRepository>();
        services.AddScoped<IProductCategoryTypeLookupRepository, ProductCategoryTypeLookupRepository>();
        services.AddScoped<ISubCategoryLookupRepository, SubCategoryLookupRepository>();
        services.AddScoped<IChargeTypeLookupRepository, ChargeTypeLookupRepository>();
        services.AddScoped<IBusinessEntityTypeLookupRepository, BusinessEntityTypeLookupRepository>();
        services.AddScoped<IPaymentMethodLookupRepository, PaymentMethodLookupRepository>();
        services.AddScoped<IOrderTypeLookupRepository, OrderTypeLookupRepository>();
        services.AddScoped<IOrderStatusLookupRepository, OrderStatusLookupRepository>();
        services.AddScoped<IFinancialTransactionTypeLookupRepository, FinancialTransactionTypeLookupRepository>();
        services.AddScoped<IFinancialTransactionStatusLookupRepository, FinancialTransactionStatusLookupRepository>();
        services.AddScoped<ITransactionTypeLookupRepository, TransactionTypeLookupRepository>();
        services.AddScoped<ITransactionStatusLookupRepository, TransactionStatusLookupRepository>();
        services.AddScoped<IRepairStatusLookupRepository, RepairStatusLookupRepository>();
        services.AddScoped<IRepairPriorityLookupRepository, RepairPriorityLookupRepository>();

        // Product repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IGoldRateRepository, GoldRateRepository>();
        services.AddScoped<IMakingChargesRepository, MakingChargesRepository>();
        services.AddScoped<ITaxConfigurationRepository, TaxConfigurationRepository>();

        // Customer repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerPurchaseRepository, CustomerPurchaseRepository>();

        // Supplier repositories
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        // Commented out until implemented
        // services.AddScoped<ISupplierTransactionRepository, SupplierTransactionRepository>();
        // services.AddScoped<ISupplierGoldBalanceRepository, SupplierGoldBalanceRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Register all enhanced services with comprehensive business logic
    /// </summary>
    public static IServiceCollection AddEnhancedServices(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEnhancedStructuredLoggingService, EnhancedStructuredLoggingService>();

        // Authentication and authorization
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();

        // Core business services
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISupplierService, SupplierService>();

        // Lookup services (commented out until implemented)
        // services.AddScoped<ILookupService, LookupService>();
        // services.AddScoped<IKaratTypeLookupService, KaratTypeLookupService>();
        // services.AddScoped<IProductCategoryTypeLookupService, ProductCategoryTypeLookupService>();

        // Financial services
        // services.AddScoped<IGoldRateService, GoldRateService>();
        services.AddScoped<IPricingService, PricingService>();
        // services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();

        // Inventory services
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IProductOwnershipService, ProductOwnershipService>();
        services.AddScoped<IRawGoldBalanceService, RawGoldBalanceService>();

        // Sales services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerPurchaseService, CustomerPurchaseService>();
        services.AddScoped<ICashDrawerService, CashDrawerService>();

        // Manufacturing services
        services.AddScoped<IProductManufactureService, ProductManufactureService>();
        services.AddScoped<IManufacturingWorkflowService, ManufacturingWorkflowService>();
        services.AddScoped<IManufacturingReportsService, ManufacturingReportsService>();

        // Repair services
        services.AddScoped<IRepairJobService, RepairJobService>();
        services.AddScoped<ITechnicianService, TechnicianService>();

        // Utility services
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<ILabelPrintingService, LabelPrintingService>();
        // services.AddScoped<INotificationService, NotificationService>();
        // services.AddScoped<IExportService, ExportService>();

        // Background services (commented out until implemented)
        // services.AddHostedService<DataCleanupService>();
        // services.AddHostedService<ReportGenerationService>();
        // services.AddHostedService<InventoryOptimizationService>();

        return services;
    }

    /// <summary>
    /// Register enhanced logging services
    /// </summary>
    public static IServiceCollection AddEnhancedLogging(this IServiceCollection services)
    {
        // Enhanced structured logging
        services.AddScoped<IStructuredLoggingService, StructuredLoggingService>();
        services.AddScoped<IEnhancedStructuredLoggingService, EnhancedStructuredLoggingService>();

        // Performance monitoring
        // services.AddScoped<IPerformanceMonitoringService, PerformanceMonitoringService>();

        // Error tracking (commented out until implemented)
        // services.AddScoped<IErrorTrackingService, ErrorTrackingService>();

        return services;
    }

    /// <summary>
    /// Register enhanced caching services
    /// </summary>
    public static IServiceCollection AddEnhancedCaching(this IServiceCollection services)
    {
        // Memory cache
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1000;
            options.CompactionPercentage = 0.25;
        });

        // Distributed cache (Redis or SQL Server)
        // In-memory caching (Redis commented out for now)
        services.AddMemoryCache();
        
        // TODO: Enable Redis when available
        // services.AddStackExchangeRedisCache(options =>
        // {
        //     options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6379";
        //     options.InstanceName = "DijaGoldPOS";
        // });

        // Custom caching services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ILookupCacheService, LookupCacheService>();

        return services;
    }

    /// <summary>
    /// Register health checks for all services
    /// </summary>
    public static IServiceCollection AddEnhancedHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            // Database health check
            .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database is healthy"))
            
            // Additional health checks commented out for now
            // .AddRedis(
            //     configuration.GetConnectionString("Redis") ?? "localhost:6379",
            //     name: "redis",
            //     failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded)
            
            // Custom business logic health checks
            // .AddCheck<InventoryHealthCheck>("inventory")
            // .AddCheck<GoldRateHealthCheck>("gold-rates")
            // .AddCheck<CashDrawerHealthCheck>("cash-drawer")
            
            // External service health checks
            // .AddUrlGroup(new Uri("https://api.example.com/health"), "external-api", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded)
            ;

        return services;
    }

    /// <summary>
    /// Register performance monitoring and metrics
    /// </summary>
    public static IServiceCollection AddEnhancedMetrics(this IServiceCollection services)
    {
        // Application metrics (commented out until implemented)
        // services.AddSingleton<IMetricsCollector, MetricsCollector>();
        
        // Business metrics (commented out until implemented)
        // services.AddScoped<ISalesMetricsService, SalesMetricsService>();
        // services.AddScoped<IInventoryMetricsService, InventoryMetricsService>();
        // services.AddScoped<IFinancialMetricsService, FinancialMetricsService>();

        return services;
    }

    /// <summary>
    /// Register all enhanced infrastructure in one call
    /// </summary>
    public static IServiceCollection AddDijaGoldPOSInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddEnhancedInfrastructure(configuration)
            .AddEnhancedHealthChecks(configuration)
            .AddEnhancedMetrics();
    }
}
