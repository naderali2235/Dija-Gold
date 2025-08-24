using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Services;

namespace DijaGoldPOS.API.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all lookup repositories
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLookupRepositories(this IServiceCollection services)
    {
        services.AddScoped<IKaratTypeLookupRepository, KaratTypeLookupRepository>();
        services.AddScoped<IFinancialTransactionTypeLookupRepository, FinancialTransactionTypeLookupRepository>();
        services.AddScoped<IPaymentMethodLookupRepository, PaymentMethodLookupRepository>();
        services.AddScoped<IFinancialTransactionStatusLookupRepository, FinancialTransactionStatusLookupRepository>();
        services.AddScoped<IChargeTypeLookupRepository, ChargeTypeLookupRepository>();
        services.AddScoped<IProductCategoryTypeLookupRepository, ProductCategoryTypeLookupRepository>();
        services.AddScoped<IRepairStatusLookupRepository, RepairStatusLookupRepository>();
        services.AddScoped<IRepairPriorityLookupRepository, RepairPriorityLookupRepository>();
        services.AddScoped<IOrderTypeLookupRepository, OrderTypeLookupRepository>();
        services.AddScoped<IOrderStatusLookupRepository, OrderStatusLookupRepository>();
        services.AddScoped<IBusinessEntityTypeLookupRepository, BusinessEntityTypeLookupRepository>();
        services.AddScoped<ISubCategoryLookupRepository, SubCategoryLookupRepository>();

        return services;
    }

    /// <summary>
    /// Register all lookup services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLookupServices(this IServiceCollection services)
    {
        services.AddScoped<IKaratTypeLookupService, KaratTypeLookupService>();
        services.AddScoped<IFinancialTransactionTypeLookupService, FinancialTransactionTypeLookupService>();
        services.AddScoped<IPaymentMethodLookupService, PaymentMethodLookupService>();
        services.AddScoped<IFinancialTransactionStatusLookupService, FinancialTransactionStatusLookupService>();
        services.AddScoped<IChargeTypeLookupService, ChargeTypeLookupService>();
        services.AddScoped<IProductCategoryTypeLookupService, ProductCategoryTypeLookupService>();
        services.AddScoped<IRepairStatusLookupService, RepairStatusLookupService>();
        services.AddScoped<IRepairPriorityLookupService, RepairPriorityLookupService>();
        services.AddScoped<IOrderTypeLookupService, OrderTypeLookupService>();
        services.AddScoped<IOrderStatusLookupService, OrderStatusLookupService>();
        services.AddScoped<IBusinessEntityTypeLookupService, BusinessEntityTypeLookupService>();
        services.AddScoped<ISubCategoryLookupService, SubCategoryLookupService>();

        return services;
    }

    /// <summary>
    /// Register all lookup-related services (repositories and services)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLookupInfrastructure(this IServiceCollection services)
    {
        return services
            .AddLookupRepositories()
            .AddLookupServices();
    }

    /// <summary>
    /// Register all core repositories
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        services.AddScoped<IGoldRateRepository, GoldRateRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<ICashDrawerBalanceRepository, CashDrawerBalanceRepository>();
        services.AddScoped<IProductOwnershipRepository, ProductOwnershipRepository>();
        services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Register all core business services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ILabelPrintingService, LabelPrintingService>();
        services.AddScoped<ICashDrawerService, CashDrawerService>();
        services.AddScoped<IRepairJobService, RepairJobService>();
        services.AddScoped<ITechnicianService, TechnicianService>();
        services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductOwnershipService, ProductOwnershipService>();

        return services;
    }

    /// <summary>
    /// Register all core infrastructure (repositories and services)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services)
    {
        return services
            .AddCoreRepositories()
            .AddCoreServices();
    }
}
