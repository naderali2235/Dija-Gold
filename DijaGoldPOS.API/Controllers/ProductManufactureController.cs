using DijaGoldPOS.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.ManfacturingModels;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for managing product manufacturing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductManufactureController : ControllerBase
{
    private readonly IProductManufactureService _productManufactureService;
    private readonly ILogger<ProductManufactureController> _logger;

    public ProductManufactureController(
        IProductManufactureService productManufactureService,
        ILogger<ProductManufactureController> logger)
    {
        _productManufactureService = productManufactureService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new product manufacture record
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductManufactureDto>> CreateProductManufacture([FromBody] CreateProductManufactureDto createDto)
    {
        try
        {
            var result = await _productManufactureService.CreateManufacturingRecordAsync(createDto);
            return CreatedAtAction(nameof(GetProductManufacture), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product manufacture record");
            return StatusCode(500, new { error = "An error occurred while creating the product manufacture record" });
        }
    }

    /// <summary>
    /// Get a product manufacture record by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductManufactureDto>> GetProductManufacture(int id)
    {
        try
        {
            var result = await _productManufactureService.GetManufacturingRecordByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product manufacture record with ID: {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the product manufacture record" });
        }
    }

    /// <summary>
    /// Update a product manufacture record
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductManufactureDto>> UpdateProductManufacture(int id, [FromBody] UpdateProductManufactureDto updateDto)
    {
        try
        {
            var result = await _productManufactureService.UpdateManufacturingRecordAsync(id, updateDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product manufacture record with ID: {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the product manufacture record" });
        }
    }

    /// <summary>
    /// Delete a product manufacture record
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProductManufacture(int id)
    {
        try
        {
            var result = await _productManufactureService.DeleteManufacturingRecordAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product manufacture record with ID: {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the product manufacture record" });
        }
    }

    /// <summary>
    /// Get all manufacturing records for a specific product
    /// </summary>
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductManufactureDto>>> GetManufacturingRecordsByProduct(int productId)
    {
        try
        {
            var result = await _productManufactureService.GetManufacturingRecordsByProductAsync(productId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing records for ProductId: {ProductId}", productId);
            return StatusCode(500, new { error = "An error occurred while retrieving the manufacturing records" });
        }
    }

    /// <summary>
    /// Get manufacturing summary for a product with cost breakdown
    /// </summary>
    [HttpGet("product/{productId}/summary")]
    public async Task<ActionResult<ProductManufactureSummaryDto>> GetManufacturingSummary(int productId)
    {
        try
        {
            var result = await _productManufactureService.GetManufacturingSummaryByProductAsync(productId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing summary for ProductId: {ProductId}", productId);
            return StatusCode(500, new { error = "An error occurred while retrieving the manufacturing summary" });
        }
    }

    /// <summary>
    /// Get manufacturing records by batch number
    /// </summary>
    [HttpGet("batch/{batchNumber}")]
    public async Task<ActionResult<IEnumerable<ProductManufactureDto>>> GetManufacturingRecordsByBatch(string batchNumber)
    {
        try
        {
            var result = await _productManufactureService.GetManufacturingRecordsByBatchAsync(batchNumber);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing records for BatchNumber: {BatchNumber}", batchNumber);
            return StatusCode(500, new { error = "An error occurred while retrieving the manufacturing records" });
        }
    }



    /// <summary>
    /// Get available raw gold purchase order items for manufacturing
    /// </summary>
    [HttpGet("available-raw-gold")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderItemDto>>> GetAvailableRawGoldItems()
    {
        try
        {
            var result = await _productManufactureService.GetAvailableRawGoldItemsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available raw gold items");
            return StatusCode(500, new { error = "An error occurred while retrieving available raw gold items" });
        }
    }

    /// <summary>
    /// Get remaining weight for a specific purchase order item
    /// </summary>
    [HttpGet("purchase-order-item/{purchaseOrderItemId}/remaining-weight")]
    public async Task<ActionResult<decimal>> GetRemainingWeight(int purchaseOrderItemId)
    {
        try
        {
            var result = await _productManufactureService.GetRemainingWeightAsync(purchaseOrderItemId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving remaining weight for PurchaseOrderItemId: {PurchaseOrderItemId}", purchaseOrderItemId);
            return StatusCode(500, new { error = "An error occurred while retrieving the remaining weight" });
        }
    }

    /// <summary>
    /// Check if a purchase order item has sufficient weight for manufacturing
    /// </summary>
    [HttpGet("purchase-order-item/{purchaseOrderItemId}/check-weight")]
    public async Task<ActionResult<bool>> CheckSufficientWeight(int purchaseOrderItemId, [FromQuery] decimal? requiredWeight = null)
    {
        try
        {
            if (!requiredWeight.HasValue)
            {
                return BadRequest(new { error = "requiredWeight parameter is required. Please specify the weight needed for manufacturing (e.g., ?requiredWeight=5.0)" });
            }

            var result = await _productManufactureService.CheckSufficientWeightAsync(purchaseOrderItemId, requiredWeight.Value);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking sufficient weight for PurchaseOrderItemId: {PurchaseOrderItemId}", purchaseOrderItemId);
            return StatusCode(500, new { error = "An error occurred while checking sufficient weight" });
        }
    }

    /// <summary>
    /// Get all product manufacture records with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProductManufactureDto>>> GetAllProductManufactures(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _productManufactureService.GetAllManufacturingRecordsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated product manufacture records");
            return StatusCode(500, new { error = "An error occurred while retrieving the product manufacture records" });
        }
    }

    #region Workflow Endpoints

    /// <summary>
    /// Transitions a manufacturing record to the next workflow step
    /// </summary>
    [HttpPut("{id}/transition")]
    public async Task<ActionResult<ProductManufactureDto>> TransitionWorkflow(int id, [FromBody] WorkflowTransitionDto transitionDto)
    {
        try
        {
            var result = await _productManufactureService.TransitionWorkflowAsync(id, transitionDto.TargetStatus, transitionDto.Notes);
            if (!result)
                return NotFound();

            var updatedRecord = await _productManufactureService.GetManufacturingRecordByIdAsync(id);
            return Ok(updatedRecord);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning workflow for manufacturing {Id}", id);
            return StatusCode(500, new { error = "An error occurred while transitioning the workflow" });
        }
    }

    /// <summary>
    /// Performs quality check on a manufacturing record
    /// </summary>
    [HttpPut("{id}/quality-check")]
    public async Task<ActionResult<ProductManufactureDto>> PerformQualityCheck(int id, [FromBody] QualityCheckDto qualityCheckDto)
    {
        try
        {
            var result = await _productManufactureService.PerformQualityCheckAsync(id, qualityCheckDto.Passed, qualityCheckDto.Notes);
            if (!result)
                return NotFound();

            var updatedRecord = await _productManufactureService.GetManufacturingRecordByIdAsync(id);
            return Ok(updatedRecord);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing quality check for manufacturing {Id}", id);
            return StatusCode(500, new { error = "An error occurred while performing the quality check" });
        }
    }

    /// <summary>
    /// Performs final approval on a manufacturing record
    /// </summary>
    [HttpPut("{id}/final-approval")]
    public async Task<ActionResult<ProductManufactureDto>> PerformFinalApproval(int id, [FromBody] FinalApprovalDto approvalDto)
    {
        try
        {
            var result = await _productManufactureService.PerformFinalApprovalAsync(id, approvalDto.Approved, approvalDto.Notes);
            if (!result)
                return NotFound();

            var updatedRecord = await _productManufactureService.GetManufacturingRecordByIdAsync(id);
            return Ok(updatedRecord);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing final approval for manufacturing {Id}", id);
            return StatusCode(500, new { error = "An error occurred while performing the final approval" });
        }
    }

    /// <summary>
    /// Gets workflow history for a manufacturing record
    /// </summary>
    [HttpGet("{id}/workflow-history")]
    public async Task<ActionResult<IEnumerable<ManufacturingWorkflowHistory>>> GetWorkflowHistory(int id)
    {
        try
        {
            var result = await _productManufactureService.GetWorkflowHistoryAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow history for manufacturing {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the workflow history" });
        }
    }

    /// <summary>
    /// Gets available workflow transitions for a manufacturing record
    /// </summary>
    [HttpGet("{id}/available-transitions")]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableTransitions(int id)
    {
        try
        {
            var result = await _productManufactureService.GetAvailableTransitionsAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available transitions for manufacturing {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving available transitions" });
        }
    }

    /// <summary>
    /// Complete manufacturing and finalize raw gold consumption
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ProductManufactureDto>> CompleteManufacturing(int id)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _productManufactureService.CompleteManufacturingAsync(id, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing manufacturing {Id}", id);
            return StatusCode(500, new { error = "An error occurred while completing the manufacturing" });
        }
    }

    /// <summary>
    /// Cancel manufacturing and reverse all reservations and records
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ProductManufactureDto>> CancelManufacturing(int id, [FromBody] CancelManufacturingRequest? request = null)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var cancellationReason = request?.Reason ?? "No reason provided";
            var result = await _productManufactureService.CancelManufacturingAsync(id, userId, cancellationReason);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling manufacturing {Id}", id);
            return StatusCode(500, new { error = "An error occurred while cancelling the manufacturing" });
        }
    }

    #endregion
}

/// <summary>
/// Request model for cancelling manufacturing
/// </summary>
public class CancelManufacturingRequest
{
    /// <summary>
    /// Reason for cancellation
    /// </summary>
    public string? Reason { get; set; }
}
