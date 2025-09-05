using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Shared;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.ProductModels;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for product management operations
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IPricingService _pricingService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IPricingService pricingService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _pricingService = pricingService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated products with filtering and search
    /// </summary>
    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequestDto searchRequest)
    {
        try
        {
            // Build and filter query
            var query = BuildProductQuery(searchRequest);

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paginated results
            var products = await GetPaginatedProductsAsync(query, searchRequest);

            return CreatePagedResult(products, totalCount, searchRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            throw;
        }
    }

    /// <summary>
    /// Build and filter the product query based on search criteria
    /// </summary>
    private IQueryable<Product> BuildProductQuery(ProductSearchRequestDto searchRequest)
    {
        var query = _productRepository.GetQueryable("Supplier", "CategoryType", "KaratType", "SubCategoryLookup");

        // Apply search term filter
        if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchRequest.SearchTerm) ||
                                    p.ProductCode.Contains(searchRequest.SearchTerm) ||
                                    (p.Brand != null && p.Brand.Contains(searchRequest.SearchTerm)));
        }

        // Apply specific filters
        if (searchRequest.CategoryTypeId.HasValue)
            query = query.Where(p => p.CategoryTypeId == searchRequest.CategoryTypeId.Value);

        if (searchRequest.KaratTypeId.HasValue)
            query = query.Where(p => p.KaratTypeId == searchRequest.KaratTypeId.Value);

        if (!string.IsNullOrEmpty(searchRequest.Brand))
            query = query.Where(p => p.Brand == searchRequest.Brand);

        if (searchRequest.SubCategoryId.HasValue)
            query = query.Where(p => p.SubCategoryId == searchRequest.SubCategoryId.Value);
        else if (!string.IsNullOrEmpty(searchRequest.SubCategory))
            query = query.Where(p => p.SubCategoryLookup != null && p.SubCategoryLookup.Name == searchRequest.SubCategory);

        if (searchRequest.SupplierId.HasValue)
            query = query.Where(p => p.SupplierId == searchRequest.SupplierId.Value);

        if (searchRequest.IsActive.HasValue)
            query = query.Where(p => p.IsActive == searchRequest.IsActive.Value);

        return query;
    }

    /// <summary>
    /// Get paginated products from the filtered query
    /// </summary>
    private async Task<List<ProductDto>> GetPaginatedProductsAsync(IQueryable<Product> query, ProductSearchRequestDto searchRequest)
    {
        return await query
            .OrderBy(p => p.CategoryType)
            .ThenBy(p => p.Name)
            .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
            .Take(searchRequest.PageSize)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Create a paged result from products and metadata
    /// </summary>
    private static PagedResult<ProductDto> CreatePagedResult(List<ProductDto> products, int totalCount, ProductSearchRequestDto searchRequest)
    {
        return new PagedResult<ProductDto>
        {
            Items = products,
            TotalCount = totalCount,
            PageNumber = searchRequest.PageNumber,
            PageSize = searchRequest.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
        };
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetQueryable("Supplier", "CategoryType", "KaratType", "SubCategoryLookup")
                .Where(p => p.Id == id)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get product with inventory information
    /// </summary>
    public async Task<ProductWithInventoryDto?> GetProductWithInventoryAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetQueryable("Supplier", "CategoryType", "KaratType", "SubCategoryLookup", "InventoryRecords.Branch")
                .Where(p => p.Id == id)
                .ProjectTo<ProductWithInventoryDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with inventory {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    public async Task<ProductDto> CreateProductAsync(CreateProductRequestDto request, string userId)
    {
        try
        {
            // Check if product code already exists
            if (await _productRepository.ProductCodeExistsAsync(request.ProductCode))
            {
                throw new ArgumentException("Product code already exists");
            }

            var product = _mapper.Map<Product>(request);
            product.CreatedBy = userId;
            product.CreatedAt = DateTime.UtcNow;

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

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

            return productDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    public async Task<ProductDto> UpdateProductAsync(UpdateProductRequestDto request, string userId)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            // Check if product code is being changed and already exists
            if (product.ProductCode != request.ProductCode &&
                await _productRepository.ProductCodeExistsAsync(request.ProductCode, product.Id))
            {
                throw new ArgumentException("Product code already exists");
            }

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
            // SubCategory is a computed property - no need to assign
            product.Shape = request.Shape;
            product.PurityCertificateNumber = request.PurityCertificateNumber;
            product.CountryOfOrigin = request.CountryOfOrigin;
            product.YearOfMinting = request.YearOfMinting;
            product.FaceValue = request.FaceValue;
            product.HasNumismaticValue = request.HasNumismaticValue ?? false;
            product.MakingChargesApplicable = request.MakingChargesApplicable;
            product.UseProductMakingCharges = request.UseProductMakingCharges;
            product.ProductMakingChargesTypeId = request.ProductMakingChargesTypeId;
            product.ProductMakingChargesValue = request.ProductMakingChargesValue ?? 0;
            product.SupplierId = request.SupplierId;
            product.ModifiedBy = userId;
            product.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

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

            return productDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", request.Id);
            throw;
        }
    }

    /// <summary>
    /// Deactivate a product (soft delete)
    /// </summary>
    public async Task DeactivateProductAsync(int id, string userId)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            product.IsActive = false;
            product.ModifiedBy = userId;
            product.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get product pricing information
    /// </summary>
    public async Task<ProductPricingDto> GetProductPricingAsync(int id, decimal quantity = 1, int? customerId = null)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            var priceCalculation = await _pricingService.CalculatePriceAsync(product, quantity, customerId);

            return new ProductPricingDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CurrentGoldRate = priceCalculation.GoldRateUsed?.RatePerGram ?? 0,
                EstimatedBasePrice = priceCalculation.GoldValue,
                EstimatedMakingCharges = priceCalculation.MakingChargesAmount,
                EstimatedTotalPrice = priceCalculation.FinalTotal,
                PriceCalculatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating product pricing for {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Check if product code exists
    /// </summary>
    public async Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null)
    {
        try
        {
            return await _productRepository.ProductCodeExistsAsync(productCode, excludeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if product code exists: {ProductCode}", productCode);
            throw;
        }
    }
}
