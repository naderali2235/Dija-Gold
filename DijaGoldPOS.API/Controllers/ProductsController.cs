using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Products controller for product management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IPricingService _pricingService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    private readonly ILabelPrintingService _labelPrintingService;

    public ProductsController(
        IProductService productService,
        IPricingService pricingService,
        IAuditService auditService,
        ILogger<ProductsController> logger,
        IMapper mapper,
        ILabelPrintingService labelPrintingService)
    {
        _productService = productService;
        _pricingService = pricingService;
        _auditService = auditService;
        _logger = logger;
        _mapper = mapper;
        _labelPrintingService = labelPrintingService;
    }

    /// <summary>
    /// Get all products with optional filtering and pagination
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>List of products</returns>
    [HttpGet]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductSearchRequestDto searchRequest)
    {
        try
        {
            var result = await _productService.GetProductsAsync(searchRequest);
            return Ok(ApiResponse<PagedResult<ProductDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving products"));
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var productDto = await _productService.GetProductByIdAsync(id);

            if (productDto == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResponse(productDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the product"));
        }
    }

    /// <summary>
    /// Get product with inventory information
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product with inventory details</returns>
    [HttpGet("{id}/inventory")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<ProductWithInventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductWithInventory(int id)
    {
        try
        {
            var productDto = await _productService.GetProductWithInventoryAsync(id);

            if (productDto == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            return Ok(ApiResponse<ProductWithInventoryDto>.SuccessResponse(productDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with inventory {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the product"));
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var productDto = await _productService.CreateProductAsync(request, userId);

            _logger.LogInformation("Product created: {ProductId} by user {UserId}", productDto.Id, userId);

            return CreatedAtAction(nameof(GetProduct), new { id = productDto.Id },
                ApiResponse<ProductDto>.SuccessResponse(productDto, "Product created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the product"));
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update request</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequestDto request)
    {
        try
        {
            if (id != request.Id)
            {
                return BadRequest(ApiResponse.ErrorResponse("Product ID mismatch"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var productDto = await _productService.UpdateProductAsync(request, userId);

            _logger.LogInformation("Product updated: {ProductId} by user {UserId}", productDto.Id, userId);

            return Ok(ApiResponse<ProductDto>.SuccessResponse(productDto, "Product updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the product"));
        }
    }

    /// <summary>
    /// Deactivate a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateProduct(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            await _productService.DeactivateProductAsync(id, userId);

            _logger.LogInformation("Product deactivated: {ProductId} by user {UserId}", id, userId);

            return Ok(ApiResponse.SuccessResponse("Product deactivated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deactivating the product"));
        }
    }

    /// <summary>
    /// Get product pricing information
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="quantity">Quantity for pricing calculation</param>
    /// <param name="customerId">Customer ID for loyalty pricing</param>
    /// <returns>Product pricing details</returns>
    [HttpGet("{id}/pricing")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<ProductPricingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductPricing(int id, [FromQuery] decimal quantity = 1, [FromQuery] int? customerId = null)
    {
        try
        {
            var pricingDto = await _productService.GetProductPricingAsync(id, quantity, customerId);

            return Ok(ApiResponse<ProductPricingDto>.SuccessResponse(pricingDto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating product pricing for {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while calculating product pricing"));
        }
    }
}
