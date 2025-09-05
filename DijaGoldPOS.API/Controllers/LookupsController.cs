using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for managing lookup data (enums, categories, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ILogger<LookupsController> _logger;
    private readonly IKaratTypeLookupService _karatTypeService;
    private readonly IFinancialTransactionTypeLookupService _transactionTypeService;
    private readonly IPaymentMethodLookupService _paymentMethodService;
    private readonly IFinancialTransactionStatusLookupService _transactionStatusService;
    private readonly IChargeTypeLookupService _chargeTypeService;
    private readonly IProductCategoryTypeLookupService _productCategoryService;
    private readonly IRepairStatusLookupService _repairStatusService;
    private readonly IRepairPriorityLookupService _repairPriorityService;
    private readonly IOrderTypeLookupService _orderTypeService;
    private readonly IOrderStatusLookupService _orderStatusService;
    private readonly IBusinessEntityTypeLookupService _businessEntityTypeService;
    private readonly ISubCategoryLookupService _subCategoryService;

    public LookupsController(
        ILogger<LookupsController> logger,
        IKaratTypeLookupService karatTypeService,
        IFinancialTransactionTypeLookupService transactionTypeService,
        IPaymentMethodLookupService paymentMethodService,
        IFinancialTransactionStatusLookupService transactionStatusService,
        IChargeTypeLookupService chargeTypeService,
        IProductCategoryTypeLookupService productCategoryService,
        IRepairStatusLookupService repairStatusService,
        IRepairPriorityLookupService repairPriorityService,
        IOrderTypeLookupService orderTypeService,
        IOrderStatusLookupService orderStatusService,
        IBusinessEntityTypeLookupService businessEntityTypeService,
        ISubCategoryLookupService subCategoryService)
    {
        _logger = logger;
        _karatTypeService = karatTypeService;
        _transactionTypeService = transactionTypeService;
        _paymentMethodService = paymentMethodService;
        _transactionStatusService = transactionStatusService;
        _chargeTypeService = chargeTypeService;
        _productCategoryService = productCategoryService;
        _repairStatusService = repairStatusService;
        _repairPriorityService = repairPriorityService;
        _orderTypeService = orderTypeService;
        _orderStatusService = orderStatusService;
        _businessEntityTypeService = businessEntityTypeService;
        _subCategoryService = subCategoryService;
    }

    /// <summary>
    /// Get lookup values for transaction types
    /// </summary>
    [HttpGet("transaction-types")]
    public async Task<ActionResult<IEnumerable<FinancialTransactionTypeLookupDto>>> GetTransactionTypes()
    {
        try
        {
            var data = await _transactionTypeService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch transaction types");
            return StatusCode(500, new { success = false, message = "Failed to fetch transaction types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for payment methods
    /// </summary>
    [HttpGet("payment-methods")]
    public async Task<ActionResult<IEnumerable<PaymentMethodLookupDto>>> GetPaymentMethods()
    {
        try
        {
            var data = await _paymentMethodService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch payment methods");
            return StatusCode(500, new { success = false, message = "Failed to fetch payment methods", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for transaction statuses
    /// </summary>
    [HttpGet("transaction-statuses")]
    public async Task<ActionResult<IEnumerable<FinancialTransactionStatusLookupDto>>> GetTransactionStatuses()
    {
        try
        {
            var data = await _transactionStatusService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch transaction statuses");
            return StatusCode(500, new { success = false, message = "Failed to fetch transaction statuses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for charge types
    /// </summary>
    [HttpGet("charge-types")]
    public async Task<ActionResult<IEnumerable<ChargeTypeLookupDto>>> GetChargeTypes()
    {
        try
        {
            var data = await _chargeTypeService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch charge types");
            return StatusCode(500, new { success = false, message = "Failed to fetch charge types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for karat types
    /// </summary>
    [HttpGet("karat-types")]
    public async Task<ActionResult<IEnumerable<KaratTypeLookupDto>>> GetKaratTypes()
    {
        try
        {
            var data = await _karatTypeService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch karat types");
            return StatusCode(500, new { success = false, message = "Failed to fetch karat types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for product category types
    /// </summary>
    [HttpGet("product-category-types")]
    public async Task<ActionResult<IEnumerable<ProductCategoryTypeLookupDto>>> GetProductCategoryTypes()
    {
        try
        {
            var data = await _productCategoryService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch product category types");
            return StatusCode(500, new { success = false, message = "Failed to fetch product category types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for repair statuses
    /// </summary>
    [HttpGet("repair-statuses")]
    public async Task<ActionResult<IEnumerable<RepairStatusLookupDto>>> GetRepairStatuses()
    {
        try
        {
            var data = await _repairStatusService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch repair statuses");
            return StatusCode(500, new { success = false, message = "Failed to fetch repair statuses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for repair priorities
    /// </summary>
    [HttpGet("repair-priorities")]
    public async Task<ActionResult<IEnumerable<RepairPriorityLookupDto>>> GetRepairPriorities()
    {
        try
        {
            var data = await _repairPriorityService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch repair priorities");
            return StatusCode(500, new { success = false, message = "Failed to fetch repair priorities", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for order types
    /// </summary>
    [HttpGet("order-types")]
    public async Task<ActionResult<IEnumerable<OrderTypeLookupDto>>> GetOrderTypes()
    {
        try
        {
            var data = await _orderTypeService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch order types");
            return StatusCode(500, new { success = false, message = "Failed to fetch order types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for order statuses
    /// </summary>
    [HttpGet("order-statuses")]
    public async Task<ActionResult<IEnumerable<OrderStatusLookupDto>>> GetOrderStatuses()
    {
        try
        {
            var data = await _orderStatusService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch order statuses");
            return StatusCode(500, new { success = false, message = "Failed to fetch order statuses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for business entity types
    /// </summary>
    [HttpGet("business-entity-types")]
    public async Task<ActionResult<IEnumerable<BusinessEntityTypeLookupDto>>> GetBusinessEntityTypes()
    {
        try
        {
            var data = await _businessEntityTypeService.GetAllActiveAsync();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch business entity types");
            return StatusCode(500, new { success = false, message = "Failed to fetch business entity types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get sub-categories for a specific product category
    /// </summary>
    [HttpGet("sub-categories/{categoryId}")]
    public async Task<IActionResult> GetSubCategories(int categoryId)
    {
        try
        {
            var subCategories = await _subCategoryService.GetByCategoryIdAsync(categoryId);
            return Ok(new { success = true, data = subCategories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch sub-categories for category {CategoryId}", categoryId);
            return StatusCode(500, new { success = false, message = "Failed to fetch sub-categories", error = ex.Message });
        }
    }


}
