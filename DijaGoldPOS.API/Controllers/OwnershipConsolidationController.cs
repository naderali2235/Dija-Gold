using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for managing ownership consolidation operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OwnershipConsolidationController : ControllerBase
{
    private readonly IOwnershipConsolidationService _consolidationService;
    private readonly ILogger<OwnershipConsolidationController> _logger;

    public OwnershipConsolidationController(
        IOwnershipConsolidationService consolidationService,
        ILogger<OwnershipConsolidationController> logger)
    {
        _consolidationService = consolidationService;
        _logger = logger;
    }

    /// <summary>
    /// Consolidate ownership records for a specific product and supplier
    /// </summary>
    [HttpPost("consolidate")]
    public async Task<ActionResult<ConsolidationResultDto>> ConsolidateOwnership([FromBody] ConsolidateOwnershipRequest request)
    {
        try
        {
            var result = await _consolidationService.ConsolidateOwnershipAsync(request.ProductId, request.SupplierId, request.BranchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consolidating ownership for ProductId: {ProductId}, SupplierId: {SupplierId}", 
                request.ProductId, request.SupplierId);
            return StatusCode(500, new { error = "An error occurred while consolidating ownership records" });
        }
    }

    /// <summary>
    /// Consolidate all ownership records for a supplier across all products
    /// </summary>
    [HttpPost("consolidate-supplier/{supplierId}")]
    public async Task<ActionResult<List<ConsolidationResultDto>>> ConsolidateSupplierOwnership(int supplierId, [FromQuery] int branchId)
    {
        try
        {
            var results = await _consolidationService.ConsolidateSupplierOwnershipAsync(supplierId, branchId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consolidating supplier ownership for SupplierId: {SupplierId}", supplierId);
            return StatusCode(500, new { error = "An error occurred while consolidating supplier ownership records" });
        }
    }

    /// <summary>
    /// Get consolidation opportunities for a branch
    /// </summary>
    [HttpGet("opportunities")]
    public async Task<ActionResult<List<ConsolidationOpportunityDto>>> GetConsolidationOpportunities([FromQuery] int branchId)
    {
        try
        {
            var opportunities = await _consolidationService.GetConsolidationOpportunitiesAsync(branchId);
            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consolidation opportunities for BranchId: {BranchId}", branchId);
            return StatusCode(500, new { error = "An error occurred while retrieving consolidation opportunities" });
        }
    }

    /// <summary>
    /// Calculate weighted average cost for specific ownership records
    /// </summary>
    [HttpPost("weighted-average-cost")]
    public async Task<ActionResult<WeightedAverageCostDto>> CalculateWeightedAverageCost([FromBody] List<int> ownershipIds)
    {
        try
        {
            var result = await _consolidationService.CalculateWeightedAverageCostAsync(ownershipIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating weighted average cost");
            return StatusCode(500, new { error = "An error occurred while calculating weighted average cost" });
        }
    }
}

/// <summary>
/// Request DTO for consolidating ownership
/// </summary>
public class ConsolidateOwnershipRequest
{
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public int BranchId { get; set; }
}
