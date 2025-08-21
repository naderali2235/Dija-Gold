using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Transactions controller for POS transaction operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IReceiptService _receiptService;
    private readonly IAuditService _auditService;
    private readonly ILogger<TransactionsController> _logger;
    private readonly IMapper _mapper;

    public TransactionsController(
        ITransactionService transactionService,
        IReceiptService receiptService,
        IAuditService auditService,
        ILogger<TransactionsController> logger,
        IMapper mapper)
    {
        _transactionService = transactionService;
        _receiptService = receiptService;
        _auditService = auditService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Process a sale transaction
    /// </summary>
    /// <param name="request">Sale transaction request</param>
    /// <returns>Transaction result with receipt</returns>
    [HttpPost("sale")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessSale([FromBody] SaleTransactionRequestDto request)
        {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Convert DTO to service request
            var saleRequest = new SaleTransactionRequest
            {
                BranchId = request.BranchId,
                CustomerId = request.CustomerId,
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod,
                Items = request.Items.Select(i => new SaleItemRequest
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CustomDiscountPercentage = i.CustomDiscountPercentage
                }).ToList()
            };

            var result = await _transactionService.ProcessSaleAsync(saleRequest, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(result.ErrorMessage ?? "Transaction failed"));
            }

            if (result.Transaction == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Transaction processing failed"));
            }

            var transactionDto = _mapper.Map<TransactionDto>(result.Transaction);

            _logger.LogInformation("Sale transaction {TransactionNumber} processed successfully by user {UserId}", 
                result.Transaction.TransactionNumber, userId);

            return CreatedAtAction(nameof(GetTransaction), new { id = result.Transaction.Id }, 
                ApiResponse<TransactionDto>.SuccessResponse(transactionDto, "Sale transaction completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sale transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing the sale"));
        }
    }

    /// <summary>
    /// Process a return transaction
    /// </summary>
    /// <param name="request">Return transaction request</param>
    /// <returns>Return transaction result</returns>
    [HttpPost("return")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessReturn([FromBody] ReturnTransactionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var managerId = userId; // Manager is processing the return

            // Convert DTO to service request
            var returnRequest = new ReturnTransactionRequest
            {
                OriginalTransactionId = request.OriginalTransactionId,
                ReturnReason = request.ReturnReason,
                ReturnAmount = request.ReturnAmount,
                Items = request.Items.Select(i => new ReturnItemRequest
                {
                    OriginalTransactionItemId = i.OriginalTransactionItemId,
                    ReturnQuantity = i.ReturnQuantity
                }).ToList()
            };

            var result = await _transactionService.ProcessReturnAsync(returnRequest, userId, managerId);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(result.ErrorMessage ?? "Return transaction failed"));
            }

            if (result.Transaction == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Return transaction processing failed"));
            }

            var transactionDto = _mapper.Map<TransactionDto>(result.Transaction);

            _logger.LogInformation("Return transaction {TransactionNumber} processed successfully by user {UserId}", 
                result.Transaction.TransactionNumber, userId);

            return CreatedAtAction(nameof(GetTransaction), new { id = result.Transaction.Id }, 
                ApiResponse<TransactionDto>.SuccessResponse(transactionDto, "Return transaction completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing return transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing the return"));
        }
    }

    /// <summary>
    /// Process a repair transaction
    /// </summary>
    /// <param name="request">Repair transaction request</param>
    /// <returns>Repair transaction result</returns>
    [HttpPost("repair")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRepair([FromBody] RepairTransactionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Convert DTO to service request
            var repairRequest = new RepairTransactionRequest
            {
                BranchId = request.BranchId,
                CustomerId = request.CustomerId,
                RepairDescription = request.RepairDescription,
                RepairAmount = request.RepairAmount,
                EstimatedCompletionDate = request.EstimatedCompletionDate,
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod
            };

            var result = await _transactionService.ProcessRepairAsync(repairRequest, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(result.ErrorMessage ?? "Repair transaction failed"));
            }

            if (result.Transaction == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Repair transaction processing failed"));
            }

            var transactionDto = _mapper.Map<TransactionDto>(result.Transaction);

            _logger.LogInformation("Repair transaction {TransactionNumber} processed successfully by user {UserId}", 
                result.Transaction.TransactionNumber, userId);

            return CreatedAtAction(nameof(GetTransaction), new { id = result.Transaction.Id }, 
                ApiResponse<TransactionDto>.SuccessResponse(transactionDto, "Repair transaction completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing repair transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing the repair"));
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Transaction details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(int id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionAsync(id);

            if (transaction == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Transaction not found"));
            }

            var transactionDto = _mapper.Map<TransactionDto>(transaction);

            return Ok(ApiResponse<TransactionDto>.SuccessResponse(transactionDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the transaction"));
        }
    }

    /// <summary>
    /// Get transaction by transaction number
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Transaction details</returns>
    [HttpGet("by-number/{transactionNumber}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionByNumber(string transactionNumber, [FromQuery] int branchId)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByNumberAsync(transactionNumber, branchId);

            if (transaction == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Transaction not found"));
            }

            var transactionDto = _mapper.Map<TransactionDto>(transaction);

            return Ok(ApiResponse<TransactionDto>.SuccessResponse(transactionDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction by number {TransactionNumber}", transactionNumber);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the transaction"));
        }
    }

    /// <summary>
    /// Search transactions with filtering and pagination
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>List of transactions</returns>
    [HttpGet("search")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTransactions([FromQuery] TransactionSearchRequestDto searchRequest)
    {
        try
        {
            // Convert DTO to service request
            var serviceSearchRequest = new Services.TransactionSearchRequest
            {
                BranchId = searchRequest.BranchId,
                TransactionNumber = searchRequest.TransactionNumber,
                TransactionType = searchRequest.TransactionType,
                Status = searchRequest.Status,
                CustomerId = searchRequest.CustomerId,
                CashierId = searchRequest.CashierId,
                FromDate = searchRequest.FromDate,
                ToDate = searchRequest.ToDate,
                MinAmount = searchRequest.MinAmount,
                MaxAmount = searchRequest.MaxAmount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize
            };

            var (transactions, totalCount) = await _transactionService.SearchTransactionsAsync(serviceSearchRequest);

            var transactionDtos = transactions.Select(t => _mapper.Map<TransactionDto>(t)).ToList();

            var result = new PagedResult<TransactionDto>
            {
                Items = transactionDtos,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
            };

            return Ok(ApiResponse<PagedResult<TransactionDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching transactions");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while searching transactions"));
        }
    }

    /// <summary>
    /// Cancel a transaction
    /// </summary>
    /// <param name="request">Cancellation request</param>
    /// <returns>Success message</returns>
    [HttpPost("cancel")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelTransaction([FromBody] CancelTransactionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var managerId = userId; // Manager is cancelling the transaction

            var success = await _transactionService.CancelTransactionAsync(
                request.TransactionId, request.Reason, userId, managerId);

            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResponse("Failed to cancel transaction"));
            }

            _logger.LogInformation("Transaction {TransactionId} cancelled by user {UserId}", 
                request.TransactionId, userId);

            return Ok(ApiResponse.SuccessResponse("Transaction cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling transaction {TransactionId}", request.TransactionId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while cancelling the transaction"));
        }
    }

    /// <summary>
    /// Reprint transaction receipt
    /// </summary>
    /// <param name="request">Reprint request</param>
    /// <returns>Receipt content and print status</returns>
    [HttpPost("reprint-receipt")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TransactionReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReprintReceipt([FromBody] ReprintReceiptRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (success, receiptContent) = await _receiptService.ReprintReceiptAsync(
                request.TransactionId, userId, request.Copies);

            if (!success || string.IsNullOrEmpty(receiptContent))
            {
                return NotFound(ApiResponse.ErrorResponse("Failed to reprint receipt or transaction not found"));
            }

            var receiptDto = new TransactionReceiptDto
            {
                ReceiptContent = receiptContent,
                PrintedSuccessfully = success
            };

            _logger.LogInformation("Receipt reprinted for transaction {TransactionId} by user {UserId}", 
                request.TransactionId, userId);

            return Ok(ApiResponse<TransactionReceiptDto>.SuccessResponse(receiptDto, "Receipt reprinted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprinting receipt for transaction {TransactionId}", request.TransactionId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while reprinting the receipt"));
        }
    }

    /// <summary>
    /// Get transaction summary for a date range
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Transaction summary</returns>
    [HttpGet("summary")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TransactionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionSummary(
        [FromQuery] int? branchId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var searchRequest = new Services.TransactionSearchRequest
            {
                BranchId = branchId,
                FromDate = fromDate ?? DateTime.Today,
                ToDate = toDate ?? DateTime.Today.AddDays(1).AddTicks(-1),
                PageSize = int.MaxValue
            };

            var (transactions, totalCount) = await _transactionService.SearchTransactionsAsync(searchRequest);

            var summary = new TransactionSummaryDto
            {
                TransactionCount = totalCount,
                TotalAmount = transactions.Sum(t => t.TotalAmount),
                TotalTax = transactions.Sum(t => t.TotalTaxAmount),
                AverageTransactionValue = totalCount > 0 ? transactions.Sum(t => t.TotalAmount) / totalCount : 0,
                TransactionsByType = transactions
                    .GroupBy(t => t.TransactionType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AmountsByPaymentMethod = transactions
                    .GroupBy(t => t.PaymentMethod)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalAmount))
            };

            return Ok(ApiResponse<TransactionSummaryDto>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transaction summary");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating transaction summary"));
        }
    }

    /// <summary>
    /// Debug endpoint to test transaction calculation without processing
    /// </summary>
    [HttpPost("debug-calculation")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DebugTransactionCalculation([FromBody] SaleTransactionRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Convert DTO to service request
            var saleRequest = new SaleTransactionRequest
            {
                BranchId = request.BranchId,
                CustomerId = request.CustomerId,
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod,
                Items = request.Items.Select(i => new SaleItemRequest
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CustomDiscountPercentage = i.CustomDiscountPercentage
                }).ToList()
            };

            // Just calculate totals without saving
            var debugInfo = await _transactionService.DebugCalculateTransactionTotalsAsync(saleRequest, userId);
            
            return Ok(ApiResponse<object>.SuccessResponse(debugInfo, "Debug calculation completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debug calculation");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred during debug calculation"));
        }
    }

    /// <summary>
    /// Void a pending transaction (Manager only)
    /// </summary>
    [HttpPut("{id}/void")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VoidTransaction(int id, [FromBody] VoidTransactionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var result = await _transactionService.VoidTransactionAsync(id, request.Reason, userId);

            if (!result.Success)
            {
                return BadRequest(ApiResponse.ErrorResponse(result.Message ?? "Failed to void transaction"));
            }

            _logger.LogInformation("Transaction {TransactionId} voided by user {UserId}: {Reason}", 
                id, userId, request.Reason);

            return Ok(ApiResponse.SuccessResponse("Transaction voided successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding transaction {TransactionId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while voiding the transaction"));
        }
    }

    /// <summary>
    /// Refund a completed transaction (Manager only)
    /// </summary>
    [HttpPost("refund")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefundTransaction([FromBody] RefundTransactionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var result = await _transactionService.CreateReverseTransactionAsync(request.OriginalTransactionId, request.Reason, userId, userId);

            if (!result.Success)
            {
                return BadRequest(ApiResponse.ErrorResponse(result.Message ?? "Failed to process refund"));
            }

            var transactionDto = _mapper.Map<TransactionDto>(result.Transaction);

            _logger.LogInformation("Refund transaction {TransactionNumber} created for original transaction {OriginalTransactionId} by user {UserId}", 
                result.TransactionNumber, request.OriginalTransactionId, userId);

            return CreatedAtAction(nameof(GetTransaction), new { id = result.TransactionId }, 
                ApiResponse<TransactionDto>.SuccessResponse(transactionDto, "Refund transaction created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", request.OriginalTransactionId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing the refund"));
        }
    }

    /// <summary>
    /// Check if transaction can be voided
    /// </summary>
    [HttpGet("{id}/can-void")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<CanVoidResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CanVoidTransaction(int id)
    {
        try
        {
            var (canVoid, errorMessage) = await _transactionService.CanVoidTransactionAsync(id);

            var response = new CanVoidResponseDto
            {
                CanVoid = canVoid,
                ErrorMessage = errorMessage
            };

            return Ok(ApiResponse<CanVoidResponseDto>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking void eligibility for transaction {TransactionId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while checking void eligibility"));
        }
    }

    #region Private Helper Methods

    // AutoMapper now handles entity-to-DTO mapping

    #endregion
}