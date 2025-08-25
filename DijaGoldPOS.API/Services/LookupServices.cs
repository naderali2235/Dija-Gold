using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for karat type lookups
/// </summary>
public class KaratTypeLookupService : IKaratTypeLookupService
{
    private readonly IKaratTypeLookupRepository _repository;
    private readonly ILogger<KaratTypeLookupService> _logger;

    public KaratTypeLookupService(IKaratTypeLookupRepository repository, ILogger<KaratTypeLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<KaratTypeLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active karat type lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for financial transaction type lookups
/// </summary>
public class FinancialTransactionTypeLookupService : IFinancialTransactionTypeLookupService
{
    private readonly IFinancialTransactionTypeLookupRepository _repository;
    private readonly ILogger<FinancialTransactionTypeLookupService> _logger;

    public FinancialTransactionTypeLookupService(IFinancialTransactionTypeLookupRepository repository, ILogger<FinancialTransactionTypeLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<FinancialTransactionTypeLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active financial transaction type lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for payment method lookups
/// </summary>
public class PaymentMethodLookupService : IPaymentMethodLookupService
{
    private readonly IPaymentMethodLookupRepository _repository;
    private readonly ILogger<PaymentMethodLookupService> _logger;

    public PaymentMethodLookupService(IPaymentMethodLookupRepository repository, ILogger<PaymentMethodLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentMethodLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active payment method lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for financial transaction status lookups
/// </summary>
public class FinancialTransactionStatusLookupService : IFinancialTransactionStatusLookupService
{
    private readonly IFinancialTransactionStatusLookupRepository _repository;
    private readonly ILogger<FinancialTransactionStatusLookupService> _logger;

    public FinancialTransactionStatusLookupService(IFinancialTransactionStatusLookupRepository repository, ILogger<FinancialTransactionStatusLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<FinancialTransactionStatusLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active financial transaction status lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for charge type lookups
/// </summary>
public class ChargeTypeLookupService : IChargeTypeLookupService
{
    private readonly IChargeTypeLookupRepository _repository;
    private readonly ILogger<ChargeTypeLookupService> _logger;

    public ChargeTypeLookupService(IChargeTypeLookupRepository repository, ILogger<ChargeTypeLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ChargeTypeLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active charge type lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for product category type lookups
/// </summary>
public class ProductCategoryTypeLookupService : IProductCategoryTypeLookupService
{
    private readonly IProductCategoryTypeLookupRepository _repository;
    private readonly ILogger<ProductCategoryTypeLookupService> _logger;

    public ProductCategoryTypeLookupService(IProductCategoryTypeLookupRepository repository, ILogger<ProductCategoryTypeLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductCategoryTypeLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active product category type lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for repair status lookups
/// </summary>
public class RepairStatusLookupService : IRepairStatusLookupService
{
    private readonly IRepairStatusLookupRepository _repository;
    private readonly ILogger<RepairStatusLookupService> _logger;

    public RepairStatusLookupService(IRepairStatusLookupRepository repository, ILogger<RepairStatusLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<RepairStatusLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active repair status lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for repair priority lookups
/// </summary>
public class RepairPriorityLookupService : IRepairPriorityLookupService
{
    private readonly IRepairPriorityLookupRepository _repository;
    private readonly ILogger<RepairPriorityLookupService> _logger;

    public RepairPriorityLookupService(IRepairPriorityLookupRepository repository, ILogger<RepairPriorityLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<RepairPriorityLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active repair priority lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for order type lookups
/// </summary>
public class OrderTypeLookupService : IOrderTypeLookupService
{
    private readonly IOrderTypeLookupRepository _repository;
    private readonly ILogger<OrderTypeLookupService> _logger;

    public OrderTypeLookupService(IOrderTypeLookupRepository repository, ILogger<OrderTypeLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderTypeLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active order type lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for order status lookups
/// </summary>
public class OrderStatusLookupService : IOrderStatusLookupService
{
    private readonly IOrderStatusLookupRepository _repository;
    private readonly ILogger<OrderStatusLookupService> _logger;

    public OrderStatusLookupService(IOrderStatusLookupRepository repository, ILogger<OrderStatusLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderStatusLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active order status lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for business entity type lookups
/// </summary>
public class BusinessEntityTypeLookupService : IBusinessEntityTypeLookupService
{
    private readonly IBusinessEntityTypeLookupRepository _repository;
    private readonly ILogger<BusinessEntityTypeLookupService> _logger;

    public BusinessEntityTypeLookupService(IBusinessEntityTypeLookupRepository repository, ILogger<BusinessEntityTypeLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<BusinessEntityTypeLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active business entity type lookups");
            throw;
        }
    }
}

/// <summary>
/// Service implementation for sub-category lookups
/// </summary>
public class SubCategoryLookupService : ISubCategoryLookupService
{
    private readonly ISubCategoryLookupRepository _repository;
    private readonly ILogger<SubCategoryLookupService> _logger;

    public SubCategoryLookupService(ISubCategoryLookupRepository repository, ILogger<SubCategoryLookupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<SubCategoryLookupDto>> GetAllActiveAsync()
    {
        try
        {
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active sub-category lookups");
            throw;
        }
    }

    public async Task<IEnumerable<SubCategoryLookupDto>> GetByCategoryIdAsync(int categoryId)
    {
        try
        {
            return await _repository.GetByCategoryIdAsync(categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sub-categories for category {CategoryId}", categoryId);
            throw;
        }
    }
}
