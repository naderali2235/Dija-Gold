using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for managing raw gold purchase orders
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RawGoldPurchaseOrdersController : ControllerBase
{
    private readonly IRawGoldPurchaseOrderService _rawGoldPurchaseOrderService;
    private readonly ILogger<RawGoldPurchaseOrdersController> _logger;

    public RawGoldPurchaseOrdersController(
        IRawGoldPurchaseOrderService rawGoldPurchaseOrderService,
        ILogger<RawGoldPurchaseOrdersController> logger)
    {
        _rawGoldPurchaseOrderService = rawGoldPurchaseOrderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all raw gold purchase orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RawGoldPurchaseOrderDto>>> GetAll()
    {
        try
        {
            var purchaseOrders = await _rawGoldPurchaseOrderService.GetAllAsync();
            return Ok(purchaseOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw gold purchase orders");
            return StatusCode(500, "An error occurred while retrieving raw gold purchase orders");
        }
    }

    /// <summary>
    /// Get a specific raw gold purchase order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RawGoldPurchaseOrderDto>> GetById(int id)
    {
        try
        {
            var purchaseOrder = await _rawGoldPurchaseOrderService.GetByIdAsync(id);
            if (purchaseOrder == null)
                return NotFound($"Raw gold purchase order with ID {id} not found");

            return Ok(purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw gold purchase order with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the raw gold purchase order");
        }
    }

    /// <summary>
    /// Create a new raw gold purchase order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RawGoldPurchaseOrderDto>> Create([FromBody] CreateRawGoldPurchaseOrderDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var purchaseOrder = await _rawGoldPurchaseOrderService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = purchaseOrder.Id }, purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw gold purchase order");
            return StatusCode(500, "An error occurred while creating the raw gold purchase order");
        }
    }

    /// <summary>
    /// Update an existing raw gold purchase order
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RawGoldPurchaseOrderDto>> Update(int id, [FromBody] UpdateRawGoldPurchaseOrderDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var purchaseOrder = await _rawGoldPurchaseOrderService.UpdateAsync(id, updateDto);
            return Ok(purchaseOrder);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Raw gold purchase order with ID {id} not found");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw gold purchase order with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the raw gold purchase order");
        }
    }

    /// <summary>
    /// Delete a raw gold purchase order (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _rawGoldPurchaseOrderService.DeleteAsync(id);
            if (!result)
                return NotFound($"Raw gold purchase order with ID {id} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting raw gold purchase order with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the raw gold purchase order");
        }
    }

    /// <summary>
    /// Receive raw gold from a purchase order
    /// </summary>
    [HttpPost("{id}/receive")]
    public async Task<ActionResult<RawGoldPurchaseOrderDto>> ReceiveRawGold(int id, [FromBody] ReceiveRawGoldDto receiveDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var purchaseOrder = await _rawGoldPurchaseOrderService.ReceiveRawGoldAsync(id, receiveDto);
            return Ok(purchaseOrder);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Raw gold purchase order with ID {id} not found");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving raw gold for purchase order with ID {Id}", id);
            return StatusCode(500, "An error occurred while receiving the raw gold");
        }
    }

    /// <summary>
    /// Get raw gold inventory for all branches or a specific branch
    /// </summary>
    [HttpGet("inventory")]
    public async Task<ActionResult<IEnumerable<RawGoldInventoryDto>>> GetRawGoldInventory([FromQuery] int? branchId = null)
    {
        try
        {
            var inventory = await _rawGoldPurchaseOrderService.GetRawGoldInventoryAsync(branchId);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw gold inventory");
            return StatusCode(500, "An error occurred while retrieving raw gold inventory");
        }
    }

    /// <summary>
    /// Get raw gold inventory for a specific karat type and branch
    /// </summary>
    [HttpGet("inventory/{karatTypeId}/branch/{branchId}")]
    public async Task<ActionResult<RawGoldInventoryDto>> GetRawGoldInventoryByKarat(int karatTypeId, int branchId)
    {
        try
        {
            var inventory = await _rawGoldPurchaseOrderService.GetRawGoldInventoryByKaratAsync(karatTypeId, branchId);
            if (inventory == null)
                return NotFound($"Raw gold inventory not found for karat type {karatTypeId} in branch {branchId}");

            return Ok(inventory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw gold inventory for karat type {KaratTypeId} in branch {BranchId}", karatTypeId, branchId);
            return StatusCode(500, "An error occurred while retrieving raw gold inventory");
        }
    }

    /// <summary>
    /// Process a payment (partial or full) for a raw gold purchase order
    /// </summary>
    [HttpPost("{id}/payments")]
    public async Task<ActionResult<RawGoldPurchaseOrderPaymentResult>> ProcessPayment(int id, [FromBody] ProcessRawGoldPurchaseOrderPaymentRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure request uses the route id
            request.RawGoldPurchaseOrderId = id;

            var result = await _rawGoldPurchaseOrderService.ProcessPaymentAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for raw gold purchase order with ID {Id}", id);
            return StatusCode(500, "An error occurred while processing the payment");
        }
    }

    /// <summary>
    /// Update status for a raw gold purchase order (e.g., Cancelled)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<RawGoldPurchaseOrderDto>> UpdateStatus(int id, [FromBody] UpdateRawGoldPurchaseOrderStatusRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var po = await _rawGoldPurchaseOrderService.UpdateStatusAsync(id, request.NewStatus, request.Notes);
            return Ok(po);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for raw gold purchase order with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the status");
        }
    }
}
