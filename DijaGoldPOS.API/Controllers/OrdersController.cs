using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Orders controller for handling sales operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly IAuditService _auditService;
    private readonly ILogger<OrdersController> _logger;
    private readonly IMapper _mapper;

    public OrdersController(
        IOrderService orderService,
        IFinancialTransactionService financialTransactionService,
        IAuditService auditService,
        ILogger<OrdersController> logger,
        IMapper mapper)
    {
        _orderService = orderService;
        _financialTransactionService = financialTransactionService;
        _auditService = auditService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Create a new sale order (completed by default)
    /// </summary>
    /// <param name="request">Sale order request</param>
    /// <returns>Created order with financial transaction</returns>
    [HttpPost("sale")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSaleOrder([FromBody] CreateSaleOrderRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Convert DTO to service request
            var orderRequest = new CreateOrderRequest
            {
                BranchId = request.BranchId,
                OrderTypeId = LookupTableConstants.OrderTypeSale, // Always Sale for current workflow
                CustomerId = request.CustomerId,
                GoldRateId = request.GoldRateId,
                Notes = request.Notes,
                Items = request.Items.Select(i => new CreateOrderItemRequest
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CustomDiscountPercentage = i.CustomDiscountPercentage,
                    Notes = i.Notes
                }).ToList()
            };

            // Create the order
            var order = await _orderService.CreateOrderAsync(orderRequest, userId);

            // Process payment immediately (since sales are completed by default)
            var paymentRequest = new ProcessOrderPaymentRequest
            {
                AmountPaid = request.AmountPaid,
                PaymentMethodId = request.PaymentMethodId,
                Notes = request.PaymentNotes
            };

            var paymentResult = await _orderService.ProcessOrderPaymentAsync(order.Id, paymentRequest, userId);

            if (!paymentResult.IsSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(paymentResult.ErrorMessage ?? "Failed to process payment"));
            }

            var orderDto = _mapper.Map<OrderDto>(paymentResult.Order);

            _logger.LogInformation("Sale order {OrderNumber} created and completed successfully by user {UserId}", 
                order.OrderNumber, userId);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, 
                ApiResponse<OrderDto>.SuccessResponse(orderDto, "Sale order created and completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sale order");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the sale order"));
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(int id)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id);

            if (order == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Order not found"));
            }

            var orderDto = _mapper.Map<OrderDto>(order);

            return Ok(ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the order"));
        }
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Order details</returns>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByNumber(string orderNumber, [FromQuery] int branchId)
    {
        try
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber, branchId);

            if (order == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Order not found"));
            }

            var orderDto = _mapper.Map<OrderDto>(order);

            return Ok(ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderNumber}", orderNumber);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the order"));
        }
    }

    /// <summary>
    /// Search orders
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchOrders([FromQuery] OrderSearchRequestDto request)
    {
        try
        {
            // Convert DTO to service request
            var searchRequest = new OrderSearchRequest
            {
                BranchId = request.BranchId,
                OrderTypeId = request.OrderTypeId,
                StatusId = request.StatusId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                OrderNumber = request.OrderNumber,
                CustomerId = request.CustomerId,
                CashierId = request.CashierId,
                Page = request.Page,
                PageSize = request.PageSize
            };

            var (orders, totalCount) = await _orderService.SearchOrdersAsync(searchRequest);

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            var paginatedResponse = new PaginatedResponse<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(ApiResponse<PaginatedResponse<OrderDto>>.SuccessResponse(paginatedResponse, "Orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching orders");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while searching orders"));
        }
    }

    /// <summary>
    /// Get order summary
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Order summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderSummary(
        [FromQuery] int? branchId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var summary = await _orderService.GetOrderSummaryAsync(branchId, fromDate, toDate);

            var summaryDto = _mapper.Map<OrderSummaryDto>(summary);

            return Ok(ApiResponse<OrderSummaryDto>.SuccessResponse(summaryDto, "Order summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order summary");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the order summary"));
        }
    }

    /// <summary>
    /// Get orders by customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of customer orders</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(
        int customerId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var searchRequest = new OrderSearchRequest
            {
                CustomerId = customerId,
                FromDate = fromDate,
                ToDate = toDate,
                Page = 1,
                PageSize = 100 // Get more orders for customer history
            };

            var (orders, _) = await _orderService.SearchOrdersAsync(searchRequest);

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orderDtos, "Customer orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving customer orders"));
        }
    }

    /// <summary>
    /// Get orders by cashier
    /// </summary>
    /// <param name="cashierId">Cashier ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of cashier orders</returns>
    [HttpGet("cashier/{cashierId}")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCashierOrders(
        string cashierId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var searchRequest = new OrderSearchRequest
            {
                CashierId = cashierId,
                FromDate = fromDate,
                ToDate = toDate,
                Page = 1,
                PageSize = 100 // Get more orders for cashier history
            };

            var (orders, _) = await _orderService.SearchOrdersAsync(searchRequest);

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orderDtos, "Cashier orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for cashier {CashierId}", cashierId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving cashier orders"));
        }
    }

    /// <summary>
    /// Update order notes
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated order</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Convert DTO to service request
            var updateRequest = new UpdateOrderRequest
            {
                StatusId = request.StatusId,
                Notes = request.Notes,
                EstimatedCompletionDate = request.EstimatedCompletionDate,
                ReturnReason = request.ReturnReason
            };

            var order = await _orderService.UpdateOrderAsync(id, updateRequest, userId);

            if (order == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Order not found"));
            }

            var orderDto = _mapper.Map<OrderDto>(order);

            _logger.LogInformation("Order {OrderNumber} updated successfully by user {UserId}", 
                order.OrderNumber, userId);

            return Ok(ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the order"));
        }
    }

    /// <summary>
    /// Create a new repair order with repair job
    /// </summary>
    /// <param name="request">Repair order request</param>
    /// <returns>Created repair order with repair job</returns>
    [HttpPost("repair")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<RepairOrderResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRepairOrder([FromBody] CreateRepairOrderRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Convert DTO to service request
            var repairOrderRequest = new CreateRepairOrderRequest
            {
                BranchId = request.BranchId,
                CustomerId = request.CustomerId,
                RepairDescription = request.RepairDescription,
                RepairAmount = request.RepairAmount,
                AmountPaid = request.AmountPaid,
                PaymentMethodId = request.PaymentMethodId,
                EstimatedCompletionDate = request.EstimatedCompletionDate,
                PriorityId = request.PriorityId,
                AssignedTechnicianId = request.AssignedTechnicianId,
                TechnicianNotes = request.TechnicianNotes
            };

            var result = await _orderService.CreateRepairOrderAsync(repairOrderRequest, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(result.ErrorMessage ?? "Failed to create repair order"));
            }

            var resultDto = new RepairOrderResultDto
            {
                Order = _mapper.Map<OrderDto>(result.Order),
                FinancialTransaction = _mapper.Map<FinancialTransactionDto>(result.FinancialTransaction),
                RepairJob = result.RepairJob
            };

            _logger.LogInformation("Repair order {OrderNumber} with repair job {RepairJobId} created successfully by user {UserId}", 
                result.Order?.OrderNumber, result.RepairJob?.Id, userId);

            return CreatedAtAction(nameof(GetOrder), new { id = result.Order?.Id }, 
                ApiResponse<RepairOrderResultDto>.SuccessResponse(resultDto, "Repair order created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repair order");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the repair order"));
        }
    }
}

/// <summary>
/// Request for creating a sale order
/// </summary>
public class CreateSaleOrderRequestDto
{
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public int? GoldRateId { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemRequestDto> Items { get; set; } = new();
    public decimal AmountPaid { get; set; }
    public int PaymentMethodId { get; set; }
    public string? PaymentNotes { get; set; }
}
