using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ManagerOnly")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _service.CreateAsync(request, userId);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetAsync(id);
        if (result == null)
            return NotFound(ApiResponse.ErrorResponse("Purchase order not found"));

        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var result = await _service.UpdateAsync(id, request, userId);
            return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePurchaseOrderStatusRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var result = await _service.UpdateStatusAsync(id, request, userId);
            return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id:int}/status-transitions")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderStatusTransitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusTransitions(int id)
    {
        try
        {
            var result = await _service.GetAvailableStatusTransitionsAsync(id);
            return Ok(ApiResponse<PurchaseOrderStatusTransitionDto>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] PurchaseOrderSearchRequestDto request)
    {
        var (items, total) = await _service.SearchAsync(request);
        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(ApiResponse<List<PurchaseOrderDto>>.SuccessResponse(items));
    }

    [HttpPost("receive")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Receive([FromBody] ReceivePurchaseOrderRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var ok = await _service.ReceiveAsync(request, userId);
        return ok
            ? Ok(ApiResponse.SuccessResponse("Purchase order updated"))
            : BadRequest(ApiResponse.ErrorResponse("Unable to update purchase order"));
    }

    [HttpPost("{id:int}/payments")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderPaymentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] ProcessPurchaseOrderPaymentRequestDto request)
    {
        if (id != request.PurchaseOrderId)
            return BadRequest(ApiResponse.ErrorResponse("Route id and body purchase order id do not match"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _service.ProcessPaymentAsync(request, userId);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.ErrorMessage ?? "Payment failed"));

        return Ok(ApiResponse<PurchaseOrderPaymentResult>.SuccessResponse(result));
    }
}


