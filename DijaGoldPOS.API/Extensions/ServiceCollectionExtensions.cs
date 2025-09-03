using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.IServices;

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
        // Product-related lookup repositories
        services.AddScoped<IKaratTypeLookupRepository, KaratTypeLookupRepository>();
        services.AddScoped<IProductCategoryTypeLookupRepository, ProductCategoryTypeLookupRepository>();
        services.AddScoped<ISubCategoryLookupRepository, SubCategoryLookupRepository>();

        // Financial lookup repositories
        services.AddScoped<IFinancialTransactionTypeLookupRepository, FinancialTransactionTypeLookupRepository>();
        services.AddScoped<IFinancialTransactionStatusLookupRepository, FinancialTransactionStatusLookupRepository>();
        services.AddScoped<IPaymentMethodLookupRepository, PaymentMethodLookupRepository>();
        services.AddScoped<IChargeTypeLookupRepository, ChargeTypeLookupRepository>();

        // Order and repair lookup repositories
        services.AddScoped<IOrderTypeLookupRepository, OrderTypeLookupRepository>();
        services.AddScoped<IOrderStatusLookupRepository, OrderStatusLookupRepository>();
        services.AddScoped<IRepairStatusLookupRepository, RepairStatusLookupRepository>();
        services.AddScoped<IRepairPriorityLookupRepository, RepairPriorityLookupRepository>();

        // Business entity lookup repositories
        services.AddScoped<IBusinessEntityTypeLookupRepository, BusinessEntityTypeLookupRepository>();

        return services;
    }

    /// <summary>
    /// Register all lookup services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLookupServices(this IServiceCollection services)
    {
        // Product-related lookups
        services.AddScoped<IKaratTypeLookupService, KaratTypeLookupService>();
        services.AddScoped<IProductCategoryTypeLookupService, ProductCategoryTypeLookupService>();
        services.AddScoped<ISubCategoryLookupService, SubCategoryLookupService>();

        // Financial lookups
        services.AddScoped<IFinancialTransactionTypeLookupService, FinancialTransactionTypeLookupService>();
        services.AddScoped<IFinancialTransactionStatusLookupService, FinancialTransactionStatusLookupService>();
        services.AddScoped<IPaymentMethodLookupService, PaymentMethodLookupService>();
        services.AddScoped<IChargeTypeLookupService, ChargeTypeLookupService>();

        // Order and repair lookups
        services.AddScoped<IOrderTypeLookupService, OrderTypeLookupService>();
        services.AddScoped<IOrderStatusLookupService, OrderStatusLookupService>();
        services.AddScoped<IRepairStatusLookupService, RepairStatusLookupService>();
        services.AddScoped<IRepairPriorityLookupService, RepairPriorityLookupService>();

        // Business entity lookups
        services.AddScoped<IBusinessEntityTypeLookupService, BusinessEntityTypeLookupService>();

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
        // Base repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Core business repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerPurchaseRepository, CustomerPurchaseRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();

        // Inventory and financial repositories
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        services.AddScoped<IProductOwnershipRepository, ProductOwnershipRepository>();
        services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        services.AddScoped<ICashDrawerBalanceRepository, CashDrawerBalanceRepository>();
        services.AddScoped<ITreasuryRepository, TreasuryRepository>();

        // Manufacturing and pricing repositories
        services.AddScoped<IProductManufactureRepository, ProductManufactureRepository>();
        services.AddScoped<IGoldRateRepository, GoldRateRepository>();

        // Unit of work
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
        // Authentication and authorization
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IStructuredLoggingService, StructuredLoggingService>();

        // Core business services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ICustomerPurchaseService, CustomerPurchaseService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IReportService, ReportService>();

        // Inventory and financial services
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IProductOwnershipService, ProductOwnershipService>();
        services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
        services.AddScoped<ICashDrawerService, CashDrawerService>();
        services.AddScoped<ITreasuryService, TreasuryService>();

        // Manufacturing and repair services
        services.AddScoped<IProductManufactureService, ProductManufactureService>();
        services.AddScoped<IManufacturingWorkflowService, ManufacturingWorkflowService>();
        services.AddScoped<IManufacturingReportsService, ManufacturingReportsService>();
        services.AddScoped<IRepairJobService, RepairJobService>();
        services.AddScoped<ITechnicianService, TechnicianService>();

        // Utility services
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<ILabelPrintingService, LabelPrintingService>();

        // Raw gold services
        services.AddScoped<IRawGoldPurchaseOrderService, RawGoldPurchaseOrderService>();

        // Enhanced ownership and costing services
        services.AddScoped<IOwnershipConsolidationService, OwnershipConsolidationService>();
        services.AddScoped<IWeightedAverageCostingService, WeightedAverageCostingService>();

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
