using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Products controller for product management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    private readonly ILabelPrintingService _labelPrintingService;

    public ProductsController(
        ApplicationDbContext context,
        IPricingService pricingService,
        IAuditService auditService,
        ILogger<ProductsController> logger,
        IMapper mapper,
        ILabelPrintingService labelPrintingService)
    {
        _context = context;
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
            var query = _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.CategoryType)
                .Include(p => p.KaratType)
                .Include(p => p.SubCategoryLookup)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchRequest.SearchTerm) ||
                                        p.ProductCode.Contains(searchRequest.SearchTerm) ||
                                        (p.Brand != null && p.Brand.Contains(searchRequest.SearchTerm)));
            }

            if (searchRequest.CategoryTypeId.HasValue)
                query = query.Where(p => p.CategoryTypeId == searchRequest.CategoryTypeId.Value);

            if (searchRequest.KaratTypeId.HasValue)
                query = query.Where(p => p.KaratTypeId == searchRequest.KaratTypeId.Value);

            if (!string.IsNullOrEmpty(searchRequest.Brand))
                query = query.Where(p => p.Brand == searchRequest.Brand);

            if (searchRequest.SubCategoryId.HasValue)
                query = query.Where(p => p.SubCategoryId == searchRequest.SubCategoryId.Value);
            else if (!string.IsNullOrEmpty(searchRequest.SubCategory))
                query = query.Where(p => p.SubCategory == searchRequest.SubCategory);

            if (searchRequest.SupplierId.HasValue)
                query = query.Where(p => p.SupplierId == searchRequest.SupplierId.Value);

            if (searchRequest.IsActive.HasValue)
                query = query.Where(p => p.IsActive == searchRequest.IsActive.Value);

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.CategoryType)
                .ThenBy(p => p.Name)
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PagedResult<ProductDto>
            {
                Items = products,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
            };

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
            var productDto = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.CategoryType)
                .Include(p => p.KaratType)
                .Include(p => p.SubCategoryLookup)
                .Where(p => p.Id == id)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

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
            var productDto = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.CategoryType)
                .Include(p => p.KaratType)
                .Include(p => p.SubCategoryLookup)
                .Include(p => p.InventoryRecords)
                    .ThenInclude(i => i.Branch)
                .Where(p => p.Id == id)
                .ProjectTo<ProductWithInventoryDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            // Check if product code already exists
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductCode == request.ProductCode);

            if (existingProduct != null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Product code already exists"));
            }

            // Validate supplier if provided
            if (request.SupplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(request.SupplierId.Value);
                if (supplier == null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Supplier not found"));
                }
            }

            // Validate product-level making charges if enabled
            if (request.UseProductMakingCharges)
            {
                if (!request.ProductMakingChargesTypeId.HasValue || !request.ProductMakingChargesValue.HasValue)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Product making charges type and value are required when using product-level making charges"));
                }

                if (request.ProductMakingChargesTypeId.Value < 1 || request.ProductMakingChargesTypeId.Value > 2)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Product making charges type must be 1 (Percentage) or 2 (Fixed)"));
                }

                if (request.ProductMakingChargesValue.Value <= 0)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Product making charges value must be greater than 0"));
                }

                // Validate percentage making charges cannot exceed 100%
                if (request.ProductMakingChargesTypeId.Value == 1 && request.ProductMakingChargesValue.Value > 100)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Percentage making charges cannot exceed 100%"));
                }
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var product = _mapper.Map<Product>(request);
            product.CreatedBy = userId;
            product.CreatedAt = DateTime.UtcNow;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "CREATE_PRODUCT",
                "Product",
                product.Id.ToString(),
                $"Created product: {product.Name}",
                newValues: System.Text.Json.JsonSerializer.Serialize(request)
            );

            var productDto = _mapper.Map<ProductDto>(product);

            _logger.LogInformation("Product created: {ProductId} by user {UserId}", product.Id, userId);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                ApiResponse<ProductDto>.SuccessResponse(productDto, "Product created successfully"));
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

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            // Check if product code is being changed and already exists
            if (product.ProductCode != request.ProductCode)
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductCode == request.ProductCode && p.Id != id);

                if (existingProduct != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Product code already exists"));
                }
            }

            // Validate supplier if provided
            if (request.SupplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(request.SupplierId.Value);
                if (supplier == null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Supplier not found"));
                }
            }

            // Validate product-level making charges if enabled
            if (request.UseProductMakingCharges)
            {
                if (!request.ProductMakingChargesTypeId.HasValue || !request.ProductMakingChargesValue.HasValue)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Product making charges type and value are required when using product-level making charges"));
                }

                if (request.ProductMakingChargesTypeId.Value < 1 || request.ProductMakingChargesTypeId.Value > 2)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Product making charges type must be 1 (Percentage) or 2 (Fixed)"));
                }

                if (request.ProductMakingChargesValue.Value <= 0)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Product making charges value must be greater than 0"));
                }

                // Validate percentage making charges cannot exceed 100%
                if (request.ProductMakingChargesTypeId.Value == 1 && request.ProductMakingChargesValue.Value > 100)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Percentage making charges cannot exceed 100%"));
                }
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            
            // Create DTO for old values to avoid circular references
            var oldValuesDto = _mapper.Map<ProductDto>(product);
            var oldValues = System.Text.Json.JsonSerializer.Serialize(oldValuesDto);

            // Update product properties
            product.ProductCode = request.ProductCode;
            product.Name = request.Name;
            product.CategoryTypeId = request.CategoryTypeId;
            product.KaratTypeId = request.KaratTypeId;
            product.Weight = request.Weight;
            product.Brand = request.Brand;
            product.DesignStyle = request.DesignStyle;
            product.SubCategoryId = request.SubCategoryId;
            product.SubCategory = request.SubCategory; // Keep for backward compatibility
            product.Shape = request.Shape;
            product.PurityCertificateNumber = request.PurityCertificateNumber;
            product.CountryOfOrigin = request.CountryOfOrigin;
            product.YearOfMinting = request.YearOfMinting;
            product.FaceValue = request.FaceValue;
            product.HasNumismaticValue = request.HasNumismaticValue;
            product.MakingChargesApplicable = request.MakingChargesApplicable;
            product.UseProductMakingCharges = request.UseProductMakingCharges;
            product.ProductMakingChargesTypeId = request.ProductMakingChargesTypeId;
            product.ProductMakingChargesValue = request.ProductMakingChargesValue;
            product.SupplierId = request.SupplierId;
            product.ModifiedBy = userId;
            product.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "UPDATE_PRODUCT",
                "Product",
                product.Id.ToString(),
                $"Updated product: {product.Name}",
                oldValues,
                System.Text.Json.JsonSerializer.Serialize(request)
            );

            var productDto = _mapper.Map<ProductDto>(product);

            _logger.LogInformation("Product updated: {ProductId} by user {UserId}", product.Id, userId);

            return Ok(ApiResponse<ProductDto>.SuccessResponse(productDto, "Product updated successfully"));
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
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            product.IsActive = false;
            product.ModifiedBy = userId;
            product.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "DEACTIVATE_PRODUCT",
                "Product",
                product.Id.ToString(),
                $"Deactivated product: {product.Name}",
                "IsActive: true",
                "IsActive: false"
            );

            _logger.LogInformation("Product deactivated: {ProductId} by user {UserId}", product.Id, userId);

            return Ok(ApiResponse.SuccessResponse("Product deactivated successfully"));
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
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            var priceCalculation = await _pricingService.CalculatePriceAsync(product, quantity, customerId);

            var pricingDto = new ProductPricingDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CurrentGoldRate = priceCalculation.GoldRateUsed?.RatePerGram ?? 0,
                EstimatedBasePrice = priceCalculation.GoldValue,
                EstimatedMakingCharges = priceCalculation.MakingChargesAmount,
                EstimatedTotalPrice = priceCalculation.FinalTotal,
                PriceCalculatedAt = DateTime.UtcNow
            };

            return Ok(ApiResponse<ProductPricingDto>.SuccessResponse(pricingDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating product pricing for {ProductId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while calculating product pricing"));
        }
    }
}

/// <summary>
/// Paged result wrapper
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}