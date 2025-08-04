using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
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

    public ProductsController(
        ApplicationDbContext context,
        IPricingService pricingService,
        IAuditService auditService,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _pricingService = pricingService;
        _auditService = auditService;
        _logger = logger;
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
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchRequest.SearchTerm) ||
                                        p.ProductCode.Contains(searchRequest.SearchTerm) ||
                                        (p.Brand != null && p.Brand.Contains(searchRequest.SearchTerm)));
            }

            if (searchRequest.CategoryType.HasValue)
                query = query.Where(p => p.CategoryType == searchRequest.CategoryType.Value);

            if (searchRequest.KaratType.HasValue)
                query = query.Where(p => p.KaratType == searchRequest.KaratType.Value);

            if (!string.IsNullOrEmpty(searchRequest.Brand))
                query = query.Where(p => p.Brand == searchRequest.Brand);

            if (!string.IsNullOrEmpty(searchRequest.SubCategory))
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
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    CategoryType = p.CategoryType,
                    KaratType = p.KaratType,
                    Weight = p.Weight,
                    Brand = p.Brand,
                    DesignStyle = p.DesignStyle,
                    SubCategory = p.SubCategory,
                    Shape = p.Shape,
                    PurityCertificateNumber = p.PurityCertificateNumber,
                    CountryOfOrigin = p.CountryOfOrigin,
                    YearOfMinting = p.YearOfMinting,
                    FaceValue = p.FaceValue,
                    HasNumismaticValue = p.HasNumismaticValue,
                    MakingChargesApplicable = p.MakingChargesApplicable,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.CompanyName : null,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive
                })
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
            var product = await _context.Products
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                CategoryType = product.CategoryType,
                KaratType = product.KaratType,
                Weight = product.Weight,
                Brand = product.Brand,
                DesignStyle = product.DesignStyle,
                SubCategory = product.SubCategory,
                Shape = product.Shape,
                PurityCertificateNumber = product.PurityCertificateNumber,
                CountryOfOrigin = product.CountryOfOrigin,
                YearOfMinting = product.YearOfMinting,
                FaceValue = product.FaceValue,
                HasNumismaticValue = product.HasNumismaticValue,
                MakingChargesApplicable = product.MakingChargesApplicable,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.CompanyName,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive
            };

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
            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.InventoryRecords)
                .ThenInclude(i => i.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            var productDto = new ProductWithInventoryDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                CategoryType = product.CategoryType,
                KaratType = product.KaratType,
                Weight = product.Weight,
                Brand = product.Brand,
                DesignStyle = product.DesignStyle,
                SubCategory = product.SubCategory,
                Shape = product.Shape,
                PurityCertificateNumber = product.PurityCertificateNumber,
                CountryOfOrigin = product.CountryOfOrigin,
                YearOfMinting = product.YearOfMinting,
                FaceValue = product.FaceValue,
                HasNumismaticValue = product.HasNumismaticValue,
                MakingChargesApplicable = product.MakingChargesApplicable,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.CompanyName,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive,
                TotalQuantityOnHand = product.InventoryRecords.Sum(i => i.QuantityOnHand),
                TotalWeightOnHand = product.InventoryRecords.Sum(i => i.WeightOnHand),
                Inventory = product.InventoryRecords.Select(i => new ProductInventoryDto
                {
                    BranchId = i.BranchId,
                    BranchName = i.Branch.Name,
                    QuantityOnHand = i.QuantityOnHand,
                    WeightOnHand = i.WeightOnHand,
                    MinimumStockLevel = i.MinimumStockLevel,
                    IsLowStock = i.QuantityOnHand <= i.ReorderPoint
                }).ToList()
            };

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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var product = new Product
            {
                ProductCode = request.ProductCode,
                Name = request.Name,
                CategoryType = request.CategoryType,
                KaratType = request.KaratType,
                Weight = request.Weight,
                Brand = request.Brand,
                DesignStyle = request.DesignStyle,
                SubCategory = request.SubCategory,
                Shape = request.Shape,
                PurityCertificateNumber = request.PurityCertificateNumber,
                CountryOfOrigin = request.CountryOfOrigin,
                YearOfMinting = request.YearOfMinting,
                FaceValue = request.FaceValue,
                HasNumismaticValue = request.HasNumismaticValue,
                MakingChargesApplicable = request.MakingChargesApplicable,
                SupplierId = request.SupplierId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

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

            var productDto = new ProductDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                CategoryType = product.CategoryType,
                KaratType = product.KaratType,
                Weight = product.Weight,
                Brand = product.Brand,
                DesignStyle = product.DesignStyle,
                SubCategory = product.SubCategory,
                Shape = product.Shape,
                PurityCertificateNumber = product.PurityCertificateNumber,
                CountryOfOrigin = product.CountryOfOrigin,
                YearOfMinting = product.YearOfMinting,
                FaceValue = product.FaceValue,
                HasNumismaticValue = product.HasNumismaticValue,
                MakingChargesApplicable = product.MakingChargesApplicable,
                SupplierId = product.SupplierId,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive
            };

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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var oldValues = System.Text.Json.JsonSerializer.Serialize(product);

            // Update product properties
            product.ProductCode = request.ProductCode;
            product.Name = request.Name;
            product.CategoryType = request.CategoryType;
            product.KaratType = request.KaratType;
            product.Weight = request.Weight;
            product.Brand = request.Brand;
            product.DesignStyle = request.DesignStyle;
            product.SubCategory = request.SubCategory;
            product.Shape = request.Shape;
            product.PurityCertificateNumber = request.PurityCertificateNumber;
            product.CountryOfOrigin = request.CountryOfOrigin;
            product.YearOfMinting = request.YearOfMinting;
            product.FaceValue = request.FaceValue;
            product.HasNumismaticValue = request.HasNumismaticValue;
            product.MakingChargesApplicable = request.MakingChargesApplicable;
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

            var productDto = new ProductDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                CategoryType = product.CategoryType,
                KaratType = product.KaratType,
                Weight = product.Weight,
                Brand = product.Brand,
                DesignStyle = product.DesignStyle,
                SubCategory = product.SubCategory,
                Shape = product.Shape,
                PurityCertificateNumber = product.PurityCertificateNumber,
                CountryOfOrigin = product.CountryOfOrigin,
                YearOfMinting = product.YearOfMinting,
                FaceValue = product.FaceValue,
                HasNumismaticValue = product.HasNumismaticValue,
                MakingChargesApplicable = product.MakingChargesApplicable,
                SupplierId = product.SupplierId,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive
            };

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