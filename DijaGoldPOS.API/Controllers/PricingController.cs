using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.LookupTables;
using DijaGoldPOS.API.Shared;
using System.Security.Claims;
using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Pricing controller for managing gold rates, making charges, and taxes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PricingController : ControllerBase
{
    private readonly IPricingService _pricingService;
    private readonly IAuditService _auditService;
    private readonly ILogger<PricingController> _logger;
    private readonly ApplicationDbContext _context;

    public PricingController(
        IPricingService pricingService,
        IAuditService auditService,
        ILogger<PricingController> logger,
        ApplicationDbContext context)
    {
        _pricingService = pricingService;
        _auditService = auditService;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Get current gold rates
    /// </summary>
    /// <returns>Current gold rates</returns>
    [HttpGet("gold-rates")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<GoldRateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentGoldRates()
    {
        try
        {
            var goldRates = new List<GoldRateDto>();

            // Get all karat types from lookup table
            var karatTypes = await _context.KaratTypeLookups
                .Where(kt => kt.IsActive)
                .OrderBy(kt => kt.SortOrder)
                .ToListAsync();

            foreach (var karatType in karatTypes)
            {
                var rate = await _pricingService.GetCurrentGoldRateAsync(karatType.Id);
                if (rate != null)
                {
                    goldRates.Add(new GoldRateDto
                    {
                        Id = rate.Id,
                        KaratTypeId = rate.KaratTypeId,
                        RatePerGram = rate.RatePerGram,
                        EffectiveFrom = rate.EffectiveFrom,
                        EffectiveTo = rate.EffectiveTo,
                        IsCurrent = rate.IsCurrent,
                        CreatedAt = rate.CreatedAt,
                        CreatedBy = rate.CreatedBy,
                        KaratType = new KaratTypeLookupDto
                        {
                            Id = karatType.Id,
                            Name = karatType.Name,
                            Description = karatType.Description,
                            SortOrder = karatType.SortOrder,
                            IsActive = karatType.IsActive
                        }
                    });
                }
            }

            return Ok(ApiResponse<List<GoldRateDto>>.SuccessResponse(goldRates.OrderBy(gr => gr.KaratTypeId).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current gold rates");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving gold rates"));
        }
    }

    /// <summary>
    /// Update gold rates (Manager only)
    /// </summary>
    /// <param name="request">Gold rate updates</param>
    /// <returns>Success message</returns>
    [HttpPost("gold-rates")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateGoldRates([FromBody] UpdateGoldRatesRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var goldRateUpdates = request.GoldRates.Select(gr => new GoldRateUpdate
            {
                KaratTypeId = gr.KaratTypeId,
                RatePerGram = gr.RatePerGram,
                EffectiveFrom = gr.EffectiveFrom
            }).ToList();

            var success = await _pricingService.UpdateGoldRatesAsync(goldRateUpdates, userId);

            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to update gold rates"));
            }

            _logger.LogInformation("Gold rates updated by user {UserId}", userId);

            return Ok(ApiResponse.SuccessResponse("Gold rates updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating gold rates");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating gold rates"));
        }
    }

    /// <summary>
    /// Get current making charges
    /// </summary>
    /// <returns>Current making charges</returns>
    [HttpGet("making-charges")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<MakingChargesDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentMakingCharges()
    {
        try
        {
            var makingChargesList = new List<MakingChargesDto>();

            // Get making charges for each product category
            var productCategories = await _context.ProductCategoryTypeLookups
                .Where(pc => pc.IsActive)
                .OrderBy(pc => pc.SortOrder)
                .ToListAsync();

            foreach (var categoryType in productCategories)
            {
                var charges = await _pricingService.GetCurrentMakingChargesAsync(categoryType.Id);
                if (charges != null)
                {
                    makingChargesList.Add(new MakingChargesDto
                    {
                        Id = charges.Id,
                        Name = charges.Name,
                        ProductCategoryId = charges.ProductCategoryId,
                        SubCategory = charges.SubCategory,
                        ChargeTypeId = charges.ChargeTypeId,
                        ChargeValue = charges.ChargeValue,
                        EffectiveFrom = charges.EffectiveFrom,
                        EffectiveTo = charges.EffectiveTo,
                        IsCurrent = charges.IsCurrent,
                        CreatedAt = charges.CreatedAt,
                        CreatedBy = charges.CreatedBy
                    });
                }
            }

            return Ok(ApiResponse<List<MakingChargesDto>>.SuccessResponse(makingChargesList.OrderBy(mc => mc.ProductCategoryId).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current making charges");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving making charges"));
        }
    }

    /// <summary>
    /// Update making charges (Manager only)
    /// </summary>
    /// <param name="request">Making charges update</param>
    /// <returns>Success message</returns>
    [HttpPost("making-charges")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMakingCharges([FromBody] UpdateMakingChargesRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var makingChargesUpdate = new MakingChargesUpdate
            {
                Id = request.Id,
                Name = request.Name,
                ProductCategoryId = request.ProductCategoryId,
                SubCategory = request.SubCategory,
                ChargeTypeId = request.ChargeTypeId,
                ChargeValue = request.ChargeValue,
                EffectiveFrom = request.EffectiveFrom
            };

            var success = await _pricingService.UpdateMakingChargesAsync(makingChargesUpdate, userId);

            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to update making charges"));
            }

            _logger.LogInformation("Making charges updated by user {UserId}", userId);

            return Ok(ApiResponse.SuccessResponse("Making charges updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating making charges");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating making charges"));
        }
    }

    /// <summary>
    /// Get current tax configurations
    /// </summary>
    /// <returns>Current tax configurations</returns>
    [HttpGet("taxes")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<TaxConfigurationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentTaxConfigurations()
    {
        try
        {
            var taxConfigurations = await _pricingService.GetCurrentTaxConfigurationsAsync();

            var taxDtos = taxConfigurations.Select(tc => new TaxConfigurationDto
            {
                Id = tc.Id,
                TaxName = tc.TaxName,
                TaxCode = tc.TaxCode,
                TaxTypeId = tc.TaxTypeId,
                TaxRate = tc.TaxRate,
                IsMandatory = tc.IsMandatory,
                EffectiveFrom = tc.EffectiveFrom,
                EffectiveTo = tc.EffectiveTo,
                IsCurrent = tc.IsCurrent,
                DisplayOrder = tc.DisplayOrder,
                CreatedAt = tc.CreatedAt,
                CreatedBy = tc.CreatedBy
            }).ToList();

            return Ok(ApiResponse<List<TaxConfigurationDto>>.SuccessResponse(taxDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current tax configurations");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving tax configurations"));
        }
    }

    /// <summary>
    /// Update tax configurations (Manager only)
    /// </summary>
    /// <param name="request">Tax configuration updates</param>
    /// <returns>Success message</returns>
    [HttpPost("taxes")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTaxConfigurations([FromBody] UpdateTaxConfigurationRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var taxConfigurationUpdate = new TaxConfigurationUpdate
            {
                Id = request.Id,
                TaxName = request.TaxName,
                TaxCode = request.TaxCode,
                TaxTypeId = request.TaxTypeId,
                TaxRate = request.TaxRate,
                IsMandatory = request.IsMandatory,
                EffectiveFrom = request.EffectiveFrom,
                DisplayOrder = request.DisplayOrder
            };

            var success = await _pricingService.UpdateTaxConfigurationAsync(taxConfigurationUpdate, userId);

            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to update tax configurations"));
            }

            _logger.LogInformation("Tax configurations updated by user {UserId}", userId);

            return Ok(ApiResponse.SuccessResponse("Tax configurations updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tax configurations");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating tax configurations"));
        }
    }

    /// <summary>
    /// Calculate price for a product
    /// </summary>
    /// <param name="request">Price calculation request</param>
    /// <returns>Price calculation result</returns>
    [HttpPost("calculate")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PriceCalculationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculatePrice([FromBody] PriceCalculationRequestDto request)
    {
        try
        {
            // This would need to get the product from the database
            // For now, we'll assume it's passed in the request or retrieved separately
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Log price calculation request
            await _auditService.LogAsync(
                userId,
                "CALCULATE_PRICE",
                "PriceCalculation",
                request.ProductId.ToString(),
                $"Price calculation for product {request.ProductId}, quantity {request.Quantity}"
            );

            // Return placeholder response - in real implementation would use the pricing service
            var result = new PriceCalculationResultDto
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                GoldValue = 1000.00m,
                MakingChargesAmount = 120.00m,
                SubTotal = 1120.00m,
                DiscountAmount = 0.00m,
                TaxableAmount = 1120.00m,
                Taxes = new List<TaxCalculationDto>
                {
                    new TaxCalculationDto
                    {
                        TaxName = "VAT",
                        TaxRate = 14.00m,
                        TaxableAmount = 1120.00m,
                        TaxAmount = 156.80m
                    }
                },
                TotalTaxAmount = 156.80m,
                FinalTotal = 1276.80m,
                CalculatedAt = DateTime.UtcNow
            };

            return Ok(ApiResponse<PriceCalculationResultDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while calculating price"));
        }
    }

    /// <summary>
    /// Test product-level making charges calculation
    /// </summary>
    /// <param name="request">Price calculation request</param>
    /// <returns>Price calculation result with making charges breakdown</returns>
    [HttpPost("test-product-making-charges")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<PriceCalculationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestProductMakingCharges([FromBody] PriceCalculationRequestDto request)
    {
        try
        {
            // Get the product from database
            var product = await _context.Products
                .Include(p => p.CategoryType)
                .Include(p => p.KaratType)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);

            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            // Calculate price using the pricing service
            var result = await _pricingService.CalculatePriceAsync(product, request.Quantity, request.CustomerId);

            // Map to DTO
            var resultDto = new PriceCalculationResultDto
            {
                ProductId = product.Id,
                Quantity = request.Quantity,
                GoldValue = result.GoldValue,
                MakingChargesAmount = result.MakingChargesAmount,
                SubTotal = result.SubTotal,
                DiscountAmount = result.DiscountAmount,
                TaxableAmount = result.TaxableAmount,
                TotalTaxAmount = result.TotalTaxAmount,
                FinalTotal = result.FinalTotal,
                CalculatedAt = DateTime.UtcNow,
                Taxes = result.Taxes.Select(t => new TaxCalculationDto
                {
                    TaxName = t.TaxName,
                    TaxRate = t.TaxRate,
                    TaxableAmount = t.TaxableAmount,
                    TaxAmount = t.TaxAmount
                }).ToList()
            };

            // Add debug information
            var debugInfo = new
            {
                ProductInfo = new
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductCode = product.ProductCode,
                    MakingChargesApplicable = product.MakingChargesApplicable,
                    UseProductMakingCharges = product.UseProductMakingCharges,
                    ProductMakingChargesTypeId = product.ProductMakingChargesTypeId,
                    ProductMakingChargesValue = product.ProductMakingChargesValue,
                    CategoryType = product.CategoryType?.Name,
                    SubCategory = product.SubCategory
                },
                MakingChargesUsed = result.MakingChargesUsed != null ? new
                {
                    Name = result.MakingChargesUsed.Name,
                    ChargeTypeId = result.MakingChargesUsed.ChargeTypeId,
                    ChargeValue = result.MakingChargesUsed.ChargeValue
                } : null,
                GoldRateUsed = result.GoldRateUsed != null ? new
                {
                    KaratTypeId = result.GoldRateUsed.KaratTypeId,
                    RatePerGram = result.GoldRateUsed.RatePerGram
                } : null
            };

            _logger.LogInformation("Product making charges test: {@DebugInfo}", debugInfo);

            return Ok(ApiResponse<PriceCalculationResultDto>.SuccessResponse(resultDto, "Price calculation completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing product making charges for product {ProductId}", request.ProductId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while testing product making charges"));
        }
    }
}

/// <summary>
/// Gold rate DTO
/// </summary>
public class GoldRateDto
{
    public int Id { get; set; }
    public int KaratTypeId { get; set; }
    public decimal RatePerGram { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public KaratTypeLookupDto? KaratType { get; set; }
}

/// <summary>
/// Update gold rates request DTO
/// </summary>
public class UpdateGoldRatesRequestDto
{
    public List<GoldRateUpdateDto> GoldRates { get; set; } = new();
}

/// <summary>
/// Gold rate update DTO
/// </summary>
public class GoldRateUpdateDto
{
    public int KaratTypeId { get; set; }
    public decimal RatePerGram { get; set; }
    public DateTime EffectiveFrom { get; set; }
}

/// <summary>
/// Making charges DTO
/// </summary>
public class MakingChargesDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public string? SubCategory { get; set; }
    public int ChargeTypeId { get; set; }
    public decimal ChargeValue { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Update making charges request DTO
/// </summary>
public class UpdateMakingChargesRequestDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public string? SubCategory { get; set; }
    public int ChargeTypeId { get; set; }
    public decimal ChargeValue { get; set; }
    public DateTime EffectiveFrom { get; set; }
}

/// <summary>
/// Update tax configuration request DTO
/// </summary>
public class UpdateTaxConfigurationRequestDto
{
    public int? Id { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public int TaxTypeId { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsMandatory { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

/// <summary>
/// Tax configuration DTO
/// </summary>
public class TaxConfigurationDto
{
    public int Id { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public int TaxTypeId { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrent { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Price calculation request DTO
/// </summary>
public class PriceCalculationRequestDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public int? CustomerId { get; set; }
}

/// <summary>
/// Price calculation result DTO
/// </summary>
public class PriceCalculationResultDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal GoldValue { get; set; }
    public decimal MakingChargesAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public List<TaxCalculationDto> Taxes { get; set; } = new();
    public decimal TotalTaxAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Tax calculation DTO
/// </summary>
public class TaxCalculationDto
{
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}
