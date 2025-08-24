using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service interface for karat type lookups
/// </summary>
public interface IKaratTypeLookupService
{
    Task<IEnumerable<KaratTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for financial transaction type lookups
/// </summary>
public interface IFinancialTransactionTypeLookupService
{
    Task<IEnumerable<FinancialTransactionTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for payment method lookups
/// </summary>
public interface IPaymentMethodLookupService
{
    Task<IEnumerable<PaymentMethodLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for financial transaction status lookups
/// </summary>
public interface IFinancialTransactionStatusLookupService
{
    Task<IEnumerable<FinancialTransactionStatusLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for charge type lookups
/// </summary>
public interface IChargeTypeLookupService
{
    Task<IEnumerable<ChargeTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for product category type lookups
/// </summary>
public interface IProductCategoryTypeLookupService
{
    Task<IEnumerable<ProductCategoryTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for repair status lookups
/// </summary>
public interface IRepairStatusLookupService
{
    Task<IEnumerable<RepairStatusLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for repair priority lookups
/// </summary>
public interface IRepairPriorityLookupService
{
    Task<IEnumerable<RepairPriorityLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for order type lookups
/// </summary>
public interface IOrderTypeLookupService
{
    Task<IEnumerable<OrderTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for order status lookups
/// </summary>
public interface IOrderStatusLookupService
{
    Task<IEnumerable<OrderStatusLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for business entity type lookups
/// </summary>
public interface IBusinessEntityTypeLookupService
{
    Task<IEnumerable<BusinessEntityTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Service interface for sub-category lookups
/// </summary>
public interface ISubCategoryLookupService
{
    Task<IEnumerable<SubCategoryLookupDto>> GetAllActiveAsync();
    Task<IEnumerable<SubCategoryLookupDto>> GetByCategoryIdAsync(int categoryId);
}
