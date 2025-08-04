using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Inventory controller for inventory management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IAuditService _auditService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        IAuditService auditService,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get inventory for a specific product at a branch
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Inventory information</returns>
    [HttpGet("product/{productId}/branch/{branchId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInventory(int productId, int branchId)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryAsync(productId, branchId);

            if (inventory == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Inventory not found"));
            }

            var inventoryDto = new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product?.Name ?? "",
                ProductCode = inventory.Product?.ProductCode ?? "",
                BranchId = inventory.BranchId,
                BranchName = inventory.Branch?.Name ?? "",
                QuantityOnHand = inventory.QuantityOnHand,
                WeightOnHand = inventory.WeightOnHand,
                MinimumStockLevel = inventory.MinimumStockLevel,
                MaximumStockLevel = inventory.MaximumStockLevel,
                ReorderPoint = inventory.ReorderPoint,
                LastCountDate = inventory.LastCountDate,
                IsLowStock = inventory.QuantityOnHand <= inventory.ReorderPoint
            };

            return Ok(ApiResponse<InventoryDto>.SuccessResponse(inventoryDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory for product {ProductId} at branch {BranchId}", productId, branchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving inventory"));
        }
    }

    /// <summary>
    /// Get all inventory for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="includeZeroStock">Include products with zero stock</param>
    /// <returns>List of inventory items</returns>
    [HttpGet("branch/{branchId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<InventoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranchInventory(int branchId, [FromQuery] bool includeZeroStock = false)
    {
        try
        {
            var inventoryItems = await _inventoryService.GetBranchInventoryAsync(branchId, includeZeroStock);

            var inventoryDtos = inventoryItems.Select(i => new InventoryDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                ProductCode = i.Product?.ProductCode ?? "",
                BranchId = i.BranchId,
                BranchName = i.Branch?.Name ?? "",
                QuantityOnHand = i.QuantityOnHand,
                WeightOnHand = i.WeightOnHand,
                MinimumStockLevel = i.MinimumStockLevel,
                MaximumStockLevel = i.MaximumStockLevel,
                ReorderPoint = i.ReorderPoint,
                LastCountDate = i.LastCountDate,
                IsLowStock = i.QuantityOnHand <= i.ReorderPoint
            }).ToList();

            return Ok(ApiResponse<List<InventoryDto>>.SuccessResponse(inventoryDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch inventory for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving branch inventory"));
        }
    }

    /// <summary>
    /// Get low stock items for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of low stock items</returns>
    [HttpGet("branch/{branchId}/low-stock")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<InventoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockItems(int branchId)
    {
        try
        {
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync(branchId);

            var inventoryDtos = lowStockItems.Select(i => new InventoryDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                ProductCode = i.Product?.ProductCode ?? "",
                BranchId = i.BranchId,
                BranchName = i.Branch?.Name ?? "",
                QuantityOnHand = i.QuantityOnHand,
                WeightOnHand = i.WeightOnHand,
                MinimumStockLevel = i.MinimumStockLevel,
                MaximumStockLevel = i.MaximumStockLevel,
                ReorderPoint = i.ReorderPoint,
                LastCountDate = i.LastCountDate,
                IsLowStock = true
            }).ToList();

            return Ok(ApiResponse<List<InventoryDto>>.SuccessResponse(inventoryDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock items for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving low stock items"));
        }
    }

    /// <summary>
    /// Check stock availability for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="requestedQuantity">Requested quantity</param>
    /// <returns>Stock availability status</returns>
    [HttpGet("check-availability")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<StockAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckStockAvailability(
        [FromQuery] int productId,
        [FromQuery] int branchId,
        [FromQuery] decimal requestedQuantity)
    {
        try
        {
            var isAvailable = await _inventoryService.CheckStockAvailabilityAsync(productId, branchId, requestedQuantity);
            var inventory = await _inventoryService.GetInventoryAsync(productId, branchId);

            var availabilityDto = new StockAvailabilityDto
            {
                ProductId = productId,
                BranchId = branchId,
                RequestedQuantity = requestedQuantity,
                AvailableQuantity = inventory?.QuantityOnHand ?? 0,
                IsAvailable = isAvailable,
                ShortfallQuantity = isAvailable ? 0 : requestedQuantity - (inventory?.QuantityOnHand ?? 0)
            };

            return Ok(ApiResponse<StockAvailabilityDto>.SuccessResponse(availabilityDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for product {ProductId} at branch {BranchId}", productId, branchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while checking stock availability"));
        }
    }

    /// <summary>
    /// Add inventory (from purchases or adjustments)
    /// </summary>
    /// <param name="request">Add inventory request</param>
    /// <returns>Success message</returns>
    [HttpPost("add")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddInventory([FromBody] AddInventoryRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var success = await _inventoryService.AddInventoryAsync(
                request.ProductId,
                request.BranchId,
                request.Quantity,
                request.Weight,
                request.MovementType,
                request.ReferenceNumber,
                request.UnitCost,
                userId,
                request.Notes
            );

            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to add inventory"));
            }

            _logger.LogInformation("Inventory added for product {ProductId} at branch {BranchId} by user {UserId}", 
                request.ProductId, request.BranchId, userId);

            return Ok(ApiResponse.SuccessResponse("Inventory added successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding inventory for product {ProductId}", request.ProductId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while adding inventory"));
        }
    }

    /// <summary>
    /// Adjust inventory manually
    /// </summary>
    /// <param name="request">Inventory adjustment request</param>
    /// <returns>Success message</returns>
    [HttpPost("adjust")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustInventory([FromBody] AdjustInventoryRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var success = await _inventoryService.AdjustInventoryAsync(
                request.ProductId,
                request.BranchId,
                request.NewQuantity,
                request.NewWeight,
                request.Reason,
                userId
            );

            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to adjust inventory"));
            }

            _logger.LogInformation("Inventory adjusted for product {ProductId} at branch {BranchId} by user {UserId}", 
                request.ProductId, request.BranchId, userId);

            return Ok(ApiResponse.SuccessResponse("Inventory adjusted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting inventory for product {ProductId}", request.ProductId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while adjusting inventory"));
        }
    }

    /// <summary>
    /// Transfer inventory between branches
    /// </summary>
    /// <param name="request">Inventory transfer request</param>
    /// <returns>Success message</returns>
    [HttpPost("transfer")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransferInventory([FromBody] TransferInventoryRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var success = await _inventoryService.TransferInventoryAsync(
                request.ProductId,
                request.FromBranchId,
                request.ToBranchId,
                request.Quantity,
                request.Weight,
                request.TransferNumber,
                userId,
                request.Notes
            );

            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to transfer inventory"));
            }

            _logger.LogInformation("Inventory transferred for product {ProductId} from branch {FromBranchId} to branch {ToBranchId} by user {UserId}", 
                request.ProductId, request.FromBranchId, request.ToBranchId, userId);

            return Ok(ApiResponse.SuccessResponse("Inventory transferred successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring inventory for product {ProductId}", request.ProductId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while transferring inventory"));
        }
    }

    /// <summary>
    /// Get inventory movement history
    /// </summary>
    /// <param name="request">Movement history request</param>
    /// <returns>List of inventory movements</returns>
    [HttpGet("movements")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryMovementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryMovements([FromQuery] InventoryMovementSearchRequestDto request)
    {
        try
        {
            var (movements, totalCount) = await _inventoryService.GetInventoryMovementsAsync(
                request.ProductId,
                request.BranchId,
                request.FromDate,
                request.ToDate,
                request.MovementType,
                request.PageNumber,
                request.PageSize
            );

            var movementDtos = movements.Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                ProductId = m.Inventory.ProductId,
                ProductName = m.Inventory.Product?.Name ?? "",
                ProductCode = m.Inventory.Product?.ProductCode ?? "",
                BranchId = m.Inventory.BranchId,
                BranchName = m.Inventory.Branch?.Name ?? "",
                MovementType = m.MovementType,
                ReferenceNumber = m.ReferenceNumber,
                QuantityChange = m.QuantityChange,
                WeightChange = m.WeightChange,
                QuantityBalance = m.QuantityBalance,
                WeightBalance = m.WeightBalance,
                UnitCost = m.UnitCost,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            }).ToList();

            var result = new PagedResult<InventoryMovementDto>
            {
                Items = movementDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(ApiResponse<PagedResult<InventoryMovementDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory movements");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving inventory movements"));
        }
    }
}

/// <summary>
/// Inventory DTO
/// </summary>
public class InventoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal WeightOnHand { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public decimal MaximumStockLevel { get; set; }
    public decimal ReorderPoint { get; set; }
    public DateTime LastCountDate { get; set; }
    public bool IsLowStock { get; set; }
}

/// <summary>
/// Stock availability DTO
/// </summary>
public class StockAvailabilityDto
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public decimal ShortfallQuantity { get; set; }
}

/// <summary>
/// Add inventory request DTO
/// </summary>
public class AddInventoryRequestDto
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Weight { get; set; }
    public string MovementType { get; set; } = "Purchase";
    public string? ReferenceNumber { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Adjust inventory request DTO
/// </summary>
public class AdjustInventoryRequestDto
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public decimal NewQuantity { get; set; }
    public decimal NewWeight { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Transfer inventory request DTO
/// </summary>
public class TransferInventoryRequestDto
{
    public int ProductId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Weight { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// Inventory movement search request DTO
/// </summary>
public class InventoryMovementSearchRequestDto
{
    public int? ProductId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? MovementType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Inventory movement DTO
/// </summary>
public class InventoryMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public decimal QuantityChange { get; set; }
    public decimal WeightChange { get; set; }
    public decimal QuantityBalance { get; set; }
    public decimal WeightBalance { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}