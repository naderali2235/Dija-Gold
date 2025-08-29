using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for weighted average costing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeightedAverageCostingController : ControllerBase
{
    private readonly IWeightedAverageCostingService _costingService;
    private readonly ILogger<WeightedAverageCostingController> _logger;

    public WeightedAverageCostingController(
        IWeightedAverageCostingService costingService,
        ILogger<WeightedAverageCostingController> logger)
    {
        _costingService = costingService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate weighted average cost for a product across all ownership records
    /// </summary>
    [HttpGet("product/{productId}/weighted-average")]
    public async Task<ActionResult<WeightedAverageCostResultDto>> CalculateProductWeightedAverageCost(int productId, [FromQuery] int branchId)
    {
        try
        {
            var result = await _costingService.CalculateProductWeightedAverageCostAsync(productId, branchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating weighted average cost for ProductId: {ProductId}", productId);
            return StatusCode(500, new { error = "An error occurred while calculating weighted average cost" });
        }
    }

    /// <summary>
    /// Calculate weighted average cost for manufacturing using multiple raw materials
    /// </summary>
    [HttpPost("manufacturing/weighted-cost")]
    public async Task<ActionResult<WeightedAverageCostResultDto>> CalculateManufacturingWeightedCost([FromBody] List<ProductManufactureRawMaterialDto> rawMaterials)
    {
        try
        {
            var result = await _costingService.CalculateManufacturingWeightedCostAsync(rawMaterials);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating manufacturing weighted cost");
            return StatusCode(500, new { error = "An error occurred while calculating manufacturing weighted cost" });
        }
    }

    /// <summary>
    /// Update product costs based on weighted average costing method
    /// </summary>
    [HttpPut("product/{productId}/update-cost")]
    public async Task<ActionResult<bool>> UpdateProductCostWithWeightedAverage(int productId, [FromQuery] int branchId)
    {
        try
        {
            var result = await _costingService.UpdateProductCostWithWeightedAverageAsync(productId, branchId);
            return Ok(new { success = result, message = result ? "Product cost updated successfully" : "Failed to update product cost" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product cost with weighted average for ProductId: {ProductId}", productId);
            return StatusCode(500, new { error = "An error occurred while updating product cost" });
        }
    }

    /// <summary>
    /// Get comprehensive cost analysis for a product showing all costing methods
    /// </summary>
    [HttpGet("product/{productId}/cost-analysis")]
    public async Task<ActionResult<ProductCostAnalysisDto>> GetProductCostAnalysis(int productId, [FromQuery] int branchId)
    {
        try
        {
            var result = await _costingService.GetProductCostAnalysisAsync(productId, branchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product cost analysis for ProductId: {ProductId}", productId);
            return StatusCode(500, new { error = "An error occurred while retrieving product cost analysis" });
        }
    }

    /// <summary>
    /// Calculate FIFO (First In, First Out) cost for a product
    /// </summary>
    [HttpGet("product/{productId}/fifo-cost")]
    public async Task<ActionResult<FifoCostResultDto>> CalculateFifoCost(int productId, [FromQuery] int branchId, [FromQuery] decimal requestedQuantity = 1)
    {
        try
        {
            var result = await _costingService.CalculateFifoCostAsync(productId, branchId, requestedQuantity);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating FIFO cost for ProductId: {ProductId}", productId);
            return StatusCode(500, new { error = "An error occurred while calculating FIFO cost" });
        }
    }

    /// <summary>
    /// Calculate LIFO (Last In, First Out) cost for a product
    /// </summary>
    [HttpGet("product/{productId}/lifo-cost")]
    public async Task<ActionResult<LifoCostResultDto>> CalculateLifoCost(int productId, [FromQuery] int branchId, [FromQuery] decimal requestedQuantity = 1)
    {
        try
        {
            var result = await _costingService.CalculateLifoCostAsync(productId, branchId, requestedQuantity);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating LIFO cost for ProductId: {ProductId}", productId);
            return StatusCode(500, new { error = "An error occurred while calculating LIFO cost" });
        }
    }
}
