using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for managing lookup data (enums, categories, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LookupsController> _logger;

    public LookupsController(ApplicationDbContext context, ILogger<LookupsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all lookup data
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllLookups()
    {
        try
        {
            var lookups = new
            {
                TransactionTypes = await GetLookupTableData<FinancialTransactionTypeLookup>(),
                PaymentMethods = await GetLookupTableData<PaymentMethodLookup>(),
                TransactionStatuses = await GetLookupTableData<FinancialTransactionStatusLookup>(),
                ChargeTypes = await GetLookupTableData<ChargeTypeLookup>(),
                KaratTypes = await GetLookupTableData<KaratTypeLookup>(),
                ProductCategoryTypes = await GetLookupTableData<ProductCategoryTypeLookup>(),
                RepairStatuses = await GetLookupTableData<RepairStatusLookup>(),
                RepairPriorities = await GetLookupTableData<RepairPriorityLookup>()
            };

            return Ok(new { success = true, data = lookups });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch lookup data", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for transaction types
    /// </summary>
    [HttpGet("transaction-types")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetTransactionTypes()
    {
        try
        {
            var data = await GetLookupTableData<FinancialTransactionTypeLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch transaction types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for payment methods
    /// </summary>
    [HttpGet("payment-methods")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetPaymentMethods()
    {
        try
        {
            var data = await GetLookupTableData<PaymentMethodLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch payment methods", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for transaction statuses
    /// </summary>
    [HttpGet("transaction-statuses")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetTransactionStatuses()
    {
        try
        {
            var data = await GetLookupTableData<FinancialTransactionStatusLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch transaction statuses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for charge types
    /// </summary>
    [HttpGet("charge-types")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetChargeTypes()
    {
        try
        {
            var data = await GetLookupTableData<ChargeTypeLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch charge types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for karat types
    /// </summary>
    [HttpGet("karat-types")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetKaratTypes()
    {
        try
        {
            var data = await GetLookupTableData<KaratTypeLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch karat types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for product category types
    /// </summary>
    [HttpGet("product-category-types")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetProductCategoryTypes()
    {
        try
        {
            var data = await GetLookupTableData<ProductCategoryTypeLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch product category types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for repair statuses
    /// </summary>
    [HttpGet("repair-statuses")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetRepairStatuses()
    {
        try
        {
            var data = await GetLookupTableData<RepairStatusLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch repair statuses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for repair priorities
    /// </summary>
    [HttpGet("repair-priorities")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetRepairPriorities()
    {
        try
        {
            var data = await GetLookupTableData<RepairPriorityLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch repair priorities", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for order types
    /// </summary>
    [HttpGet("order-types")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetOrderTypes()
    {
        try
        {
            var data = await GetLookupTableData<OrderTypeLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch order types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for order statuses
    /// </summary>
    [HttpGet("order-statuses")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetOrderStatuses()
    {
        try
        {
            var data = await GetLookupTableData<OrderStatusLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch order statuses", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lookup values for business entity types
    /// </summary>
    [HttpGet("business-entity-types")]
    public async Task<ActionResult<IEnumerable<EnumLookupDto>>> GetBusinessEntityTypes()
    {
        try
        {
            var data = await GetLookupTableData<BusinessEntityTypeLookup>();
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
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
            // Note: SubCategoryLookup model doesn't have CategoryTypeId relationship yet
            // For now, return all active sub-categories
            var subCategories = await _context.SubCategoryLookups
                .Where(sc => sc.IsActive)
                .OrderBy(sc => sc.SortOrder)
                .ThenBy(sc => sc.Name)
                .Select(sc => new SubCategoryLookupDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    SortOrder = sc.SortOrder
                })
                .ToListAsync();

            return Ok(new { success = true, data = subCategories });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Failed to fetch sub-categories", error = ex.Message });
        }
    }

    /// <summary>
    /// Generic method to get lookup table data
    /// </summary>
    /// <typeparam name="T">Lookup table type</typeparam>
    /// <returns>List of lookup DTOs</returns>
    private async Task<List<EnumLookupDto>> GetLookupTableData<T>() where T : class, ILookupEntity
    {
        var dbSet = _context.Set<T>();

        var items = await dbSet
            .Where(item => item.IsActive)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToListAsync();

        return items.Select(item => new EnumLookupDto
        {
            Value = item.Id,
            Name = item.Name,
            DisplayName = item.Name,
            Code = item.Name
        }).ToList();
    }

}

/// <summary>
/// DTO for sub-category lookup data
/// </summary>
public class SubCategoryLookupDto
{
    /// <summary>
    /// Sub-category ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Sub-category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sub-category description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// DTO for enum lookup data
/// </summary>
public class EnumLookupDto
{
    /// <summary>
    /// Enum numeric value
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Enum name (e.g., "K18", "Sale")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display-friendly name for UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Code for API usage
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
