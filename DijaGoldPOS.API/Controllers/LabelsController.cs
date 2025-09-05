using AutoMapper;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Endpoints for product label generation/printing and QR decoding
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabelsController : ControllerBase
{
    private readonly ILabelPrintingService _labelService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<LabelsController> _logger;
    private readonly IMapper _mapper;

    public LabelsController(ILabelPrintingService labelService, ApplicationDbContext db, ILogger<LabelsController> logger, IMapper mapper)
    {
        _labelService = labelService;
        _db = db;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Generate ZPL for a product label
    /// </summary>
    [HttpGet("{productId}/zpl")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> GenerateZpl(int productId, [FromQuery] int copies = 1)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return NotFound(ApiResponse.ErrorResponse("Product not found"));

        var zpl = _labelService.GenerateProductLabelZpl(product, copies);
        return Ok(ApiResponse<string>.SuccessResponse(zpl));
    }

    /// <summary>
    /// Print product label
    /// </summary>
    [HttpPost("{productId}/print")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> PrintProductLabel(int productId, [FromQuery] int copies = 1)
    {
        var ok = await _labelService.PrintProductLabelAsync(productId, copies);
        if (!ok) return StatusCode(500, ApiResponse.ErrorResponse("Failed to print label"));
        return Ok(ApiResponse.SuccessResponse("Label sent to printer"));
    }

    /// <summary>
    /// Decode QR payload scanned from Zebra and return product DTO
    /// </summary>
    [HttpPost("decode-qr")]
    [AllowAnonymous]
    public async Task<IActionResult> DecodeQr([FromBody] DecodeQrRequestDto request)
    {
        var product = await _labelService.DecodeQrPayloadAsync(request.Payload);
        if (product == null) return NotFound(ApiResponse.ErrorResponse("Product not found"));

        var dto = _mapper.Map<ProductDto>(product);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(dto));
    }
}


