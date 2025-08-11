using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
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
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));

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
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var ok = await _service.ReceiveAsync(request, userId);
        return ok
            ? Ok(ApiResponse.SuccessResponse("Purchase order updated"))
            : BadRequest(ApiResponse.ErrorResponse("Unable to update purchase order"));
    }
}


