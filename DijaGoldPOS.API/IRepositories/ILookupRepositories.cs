using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for karat type lookups
/// </summary>
public interface IKaratTypeLookupRepository
{
    Task<IEnumerable<KaratTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for financial transaction type lookups
/// </summary>
public interface IFinancialTransactionTypeLookupRepository
{
    Task<IEnumerable<FinancialTransactionTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for payment method lookups
/// </summary>
public interface IPaymentMethodLookupRepository
{
    Task<IEnumerable<PaymentMethodLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for financial transaction status lookups
/// </summary>
public interface IFinancialTransactionStatusLookupRepository
{
    Task<IEnumerable<FinancialTransactionStatusLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for charge type lookups
/// </summary>
public interface IChargeTypeLookupRepository
{
    Task<IEnumerable<ChargeTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for product category type lookups
/// </summary>
public interface IProductCategoryTypeLookupRepository
{
    Task<IEnumerable<ProductCategoryTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for repair status lookups
/// </summary>
public interface IRepairStatusLookupRepository
{
    Task<IEnumerable<RepairStatusLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for repair priority lookups
/// </summary>
public interface IRepairPriorityLookupRepository
{
    Task<IEnumerable<RepairPriorityLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for order type lookups
/// </summary>
public interface IOrderTypeLookupRepository
{
    Task<IEnumerable<OrderTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for order status lookups
/// </summary>
public interface IOrderStatusLookupRepository
{
    Task<IEnumerable<OrderStatusLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for business entity type lookups
/// </summary>
public interface IBusinessEntityTypeLookupRepository
{
    Task<IEnumerable<BusinessEntityTypeLookupDto>> GetAllActiveAsync();
}

/// <summary>
/// Repository interface for sub-category lookups
/// </summary>
public interface ISubCategoryLookupRepository
{
    Task<IEnumerable<SubCategoryLookupDto>> GetAllActiveAsync();
    Task<IEnumerable<SubCategoryLookupDto>> GetByCategoryIdAsync(int categoryId);
}
