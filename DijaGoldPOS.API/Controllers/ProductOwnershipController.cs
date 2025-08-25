using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Validators;
using DijaGoldPOS.API.Shared;
using FluentValidation;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for managing product ownership operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductOwnershipController : ControllerBase
{
    private readonly IProductOwnershipService _productOwnershipService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProductOwnershipController> _logger;

    public ProductOwnershipController(
        IProductOwnershipService productOwnershipService,
        ICurrentUserService currentUserService,
        ILogger<ProductOwnershipController> logger)
    {
        _productOwnershipService = productOwnershipService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Create or update product ownership
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ProductOwnershipDto>> CreateOrUpdateOwnership([FromBody] ProductOwnershipRequest request)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _productOwnershipService.CreateOrUpdateOwnershipAsync(request, userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating/updating ownership");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating product ownership");
            return StatusCode(500, "An error occurred while processing the request");
        }
    }

    /// <summary>
    /// Validate product ownership for sales
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<OwnershipValidationResult>> ValidateOwnership([FromBody] ValidateOwnershipRequest request)
    {
        try
        {
            var result = await _productOwnershipService.ValidateProductOwnershipAsync(
                request.ProductId,
                request.BranchId,
                request.RequestedQuantity);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product ownership");
            return StatusCode(500, "An error occurred while validating ownership");
        }
    }

    /// <summary>
    /// Update ownership after payment
    /// </summary>
    [HttpPost("payment")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<bool>> UpdateOwnershipAfterPayment([FromBody] UpdateOwnershipPaymentRequest request)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _productOwnershipService.UpdateOwnershipAfterPaymentAsync(
                request.ProductOwnershipId,
                request.PaymentAmount,
                request.ReferenceNumber,
                userId);

            if (!result)
            {
                return BadRequest("Failed to update ownership after payment");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ownership after payment");
            return StatusCode(500, "An error occurred while updating ownership");
        }
    }

    /// <summary>
    /// Update ownership after sale
    /// </summary>
    [HttpPost("sale")]
    [Authorize(Policy = "CashierOrManager")]
    public async Task<ActionResult<bool>> UpdateOwnershipAfterSale([FromBody] UpdateOwnershipSaleRequest request)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _productOwnershipService.UpdateOwnershipAfterSaleAsync(
                request.ProductId,
                request.BranchId,
                request.SoldQuantity,
                request.ReferenceNumber,
                userId);

            if (!result)
            {
                return BadRequest("Failed to update ownership after sale");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ownership after sale");
            return StatusCode(500, "An error occurred while updating ownership");
        }
    }

    /// <summary>
    /// Convert raw gold to products
    /// </summary>
    [HttpPost("convert-raw-gold")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ConvertRawGoldResponse>> ConvertRawGoldToProducts([FromBody] ConvertRawGoldRequest request)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var (success, message) = await _productOwnershipService.ConvertRawGoldToProductsAsync(request, userId);

            var response = new ConvertRawGoldResponse
            {
                Success = success,
                Message = message
            };

            if (success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting raw gold to products");
            return StatusCode(500, "An error occurred while converting raw gold");
        }
    }

    /// <summary>
    /// Get ownership alerts
    /// </summary>
    [HttpGet("alerts")]
    public async Task<ActionResult<ApiResponse<List<OwnershipAlertDto>>>> GetOwnershipAlerts([FromQuery] int? branchId = null)
    {
        try
        {
            var alerts = await _productOwnershipService.GetOwnershipAlertsAsync(branchId);
            return Ok(new ApiResponse<List<OwnershipAlertDto>>
            {
                Data=alerts,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ownership alerts");
            return StatusCode(500, "An error occurred while retrieving alerts");
        }
    }

    /// <summary>
    /// Get product ownership details
    /// </summary>
    [HttpGet("product/{productId}/branch/{branchId}")]
    public async Task<ActionResult<List<ProductOwnershipDto>>> GetProductOwnership(int productId, int branchId)
    {
        try
        {
            var ownerships = await _productOwnershipService.GetProductOwnershipAsync(productId, branchId);
            return Ok(ownerships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product ownership details");
            return StatusCode(500, "An error occurred while retrieving ownership details");
        }
    }

    /// <summary>
    /// Get ownership movements
    /// </summary>
    [HttpGet("movements/{productOwnershipId}")]
    public async Task<ActionResult<List<OwnershipMovementDto>>> GetOwnershipMovements(int productOwnershipId)
    {
        try
        {
            var movements = await _productOwnershipService.GetOwnershipMovementsAsync(productOwnershipId);
            return Ok(movements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ownership movements");
            return StatusCode(500, "An error occurred while retrieving movements");
        }
    }

    /// <summary>
    /// Get low ownership products
    /// </summary>
    [HttpGet("low-ownership")]
    public async Task<ActionResult<ApiResponse<List<ProductOwnershipDto>>>> GetLowOwnershipProducts([FromQuery] decimal threshold = 0.5m)
    {
        try
        {
            var products = await _productOwnershipService.GetLowOwnershipProductsAsync(threshold);
            return Ok(new ApiResponse<List<ProductOwnershipDto>>
            {
                Data = products,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low ownership products");
            return StatusCode(500, "An error occurred while retrieving low ownership products");
        }
    }

    /// <summary>
    /// Get products with outstanding payments
    /// </summary>
    [HttpGet("outstanding-payments")]
    public async Task<ActionResult<ApiResponse<List<ProductOwnershipDto>>>> GetProductsWithOutstandingPayments()
    {
        try
        {
            var products = await _productOwnershipService.GetProductsWithOutstandingPaymentsAsync();
            return Ok(new ApiResponse<List<ProductOwnershipDto>>()
            {
                Data = products,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with outstanding payments");
            return StatusCode(500, "An error occurred while retrieving products with outstanding payments");
        }
    }

    /// <summary>
    /// Get product ownership list with pagination and filtering
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductOwnershipDto>>>> GetProductOwnershipList(
        [FromQuery] int branchId,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount, currentPage, currentPageSize, totalPages) = 
                await _productOwnershipService.GetProductOwnershipListAsync(
                    branchId, searchTerm, supplierId, pageNumber, pageSize);

            var response = new PaginatedResponse<ProductOwnershipDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = currentPage,
                PageSize = currentPageSize,
                TotalPages = totalPages
            };

            return Ok(new ApiResponse<PaginatedResponse<ProductOwnershipDto>>
            {
                Data = response,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product ownership list");
            return StatusCode(500, "An error occurred while retrieving product ownership list");
        }
    }
}
