using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Financial transactions controller for handling monetary operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialTransactionsController : ControllerBase
{
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly IAuditService _auditService;
    private readonly ILogger<FinancialTransactionsController> _logger;
    private readonly IMapper _mapper;

    public FinancialTransactionsController(
        IFinancialTransactionService financialTransactionService,
        IAuditService auditService,
        ILogger<FinancialTransactionsController> logger,
        IMapper mapper)
    {
        _financialTransactionService = financialTransactionService;
        _auditService = auditService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Create a new financial transaction
    /// </summary>
    /// <param name="request">Financial transaction request</param>
    /// <returns>Created financial transaction</returns>
    [HttpPost]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<FinancialTransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFinancialTransaction([FromBody] CreateFinancialTransactionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Convert DTO to service request
            var serviceRequest = new CreateFinancialTransactionRequest
            {
                BranchId = request.BranchId,
                TransactionTypeId = request.TransactionTypeId,
                BusinessEntityId = request.BusinessEntityId,
                BusinessEntityTypeId = request.BusinessEntityTypeId,
                Subtotal = request.Subtotal,
                TotalTaxAmount = request.TotalTaxAmount,
                TotalDiscountAmount = request.TotalDiscountAmount,
                TotalAmount = request.TotalAmount,
                AmountPaid = request.AmountPaid,
                ChangeGiven = request.ChangeGiven,
                PaymentMethodId = request.PaymentMethodId,
                Notes = request.Notes,
                ApprovedByUserId = request.ApprovedByUserId
            };

            var transaction = await _financialTransactionService.CreateFinancialTransactionAsync(serviceRequest, userId);

            var transactionDto = _mapper.Map<FinancialTransactionDto>(transaction);

            _logger.LogInformation("Financial transaction {TransactionNumber} created successfully by user {UserId}", 
                transaction.TransactionNumber, userId);

            return CreatedAtAction(nameof(GetFinancialTransaction), new { id = transaction.Id }, 
                ApiResponse<FinancialTransactionDto>.SuccessResponse(transactionDto, "Financial transaction created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating financial transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the financial transaction"));
        }
    }

    /// <summary>
    /// Get financial transaction by ID
    /// </summary>
    /// <param name="id">Financial transaction ID</param>
    /// <returns>Financial transaction details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<FinancialTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinancialTransaction(int id)
    {
        try
        {
            var transaction = await _financialTransactionService.GetFinancialTransactionAsync(id);

            if (transaction == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Financial transaction not found"));
            }

            var transactionDto = _mapper.Map<FinancialTransactionDto>(transaction);

            return Ok(ApiResponse<FinancialTransactionDto>.SuccessResponse(transactionDto, "Financial transaction retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial transaction {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the financial transaction"));
        }
    }

    /// <summary>
    /// Get financial transaction by transaction number
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Financial transaction details</returns>
    [HttpGet("number/{transactionNumber}")]
    [ProducesResponseType(typeof(ApiResponse<FinancialTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinancialTransactionByNumber(string transactionNumber, [FromQuery] int branchId)
    {
        try
        {
            var transaction = await _financialTransactionService.GetFinancialTransactionByNumberAsync(transactionNumber, branchId);

            if (transaction == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Financial transaction not found"));
            }

            var transactionDto = _mapper.Map<FinancialTransactionDto>(transaction);

            return Ok(ApiResponse<FinancialTransactionDto>.SuccessResponse(transactionDto, "Financial transaction retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial transaction {TransactionNumber}", transactionNumber);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the financial transaction"));
        }
    }

    /// <summary>
    /// Search financial transactions
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <returns>Paginated list of financial transactions</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<FinancialTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchFinancialTransactions([FromQuery] FinancialTransactionSearchRequestDto request)
    {
        try
        {
            // Convert DTO to service request
            var searchRequest = new FinancialTransactionSearchRequest
            {
                BranchId = request.BranchId,
                TransactionTypeId = request.TransactionTypeId,
                StatusId = request.StatusId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TransactionNumber = request.TransactionNumber,
                ProcessedByUserId = request.ProcessedByUserId,
                BusinessEntityId = request.BusinessEntityId,
                BusinessEntityTypeId = request.BusinessEntityTypeId,
                Page = request.Page,
                PageSize = request.PageSize
            };

            var (transactions, totalCount) = await _financialTransactionService.SearchFinancialTransactionsAsync(searchRequest);

            var transactionDtos = _mapper.Map<List<FinancialTransactionDto>>(transactions);

            var paginatedResponse = new PaginatedResponse<FinancialTransactionDto>
            {
                Items = transactionDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(ApiResponse<PaginatedResponse<FinancialTransactionDto>>.SuccessResponse(paginatedResponse, "Financial transactions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching financial transactions");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while searching financial transactions"));
        }
    }

    /// <summary>
    /// Get financial transaction summary
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Financial transaction summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<FinancialTransactionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinancialTransactionSummary(
        [FromQuery] int? branchId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var summary = await _financialTransactionService.GetFinancialTransactionSummaryAsync(branchId, fromDate, toDate);

            var summaryDto = _mapper.Map<FinancialTransactionSummaryDto>(summary);

            return Ok(ApiResponse<FinancialTransactionSummaryDto>.SuccessResponse(summaryDto, "Financial transaction summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial transaction summary");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the financial transaction summary"));
        }
    }

    /// <summary>
    /// Void a financial transaction
    /// </summary>
    /// <param name="id">Financial transaction ID</param>
    /// <param name="request">Void request</param>
    /// <returns>Void result</returns>
    [HttpPost("{id}/void")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(ApiResponse<FinancialTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VoidFinancialTransaction(int id, [FromBody] VoidFinancialTransactionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var result = await _financialTransactionService.VoidFinancialTransactionAsync(id, request.Reason, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(result.ErrorMessage ?? "Failed to void financial transaction"));
            }

            var transactionDto = _mapper.Map<FinancialTransactionDto>(result.Transaction);

            _logger.LogInformation("Financial transaction {TransactionNumber} voided successfully by user {UserId}", 
                result.Transaction?.TransactionNumber, userId);

            return Ok(ApiResponse<FinancialTransactionDto>.SuccessResponse(transactionDto, "Financial transaction voided successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding financial transaction {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while voiding the financial transaction"));
        }
    }

    /// <summary>
    /// Mark financial transaction as receipt printed
    /// </summary>
    /// <param name="id">Financial transaction ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/mark-receipt-printed")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkReceiptPrinted(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var success = await _financialTransactionService.MarkReceiptPrintedAsync(id, userId);

            if (!success)
            {
                return NotFound(ApiResponse.ErrorResponse("Financial transaction not found"));
            }

            _logger.LogInformation("Financial transaction {Id} marked as receipt printed by user {UserId}", id, userId);

            return Ok(ApiResponse.SuccessResponse("Receipt printed status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking financial transaction {Id} as receipt printed", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating receipt printed status"));
        }
    }

    /// <summary>
    /// Mark financial transaction as general ledger posted
    /// </summary>
    /// <param name="id">Financial transaction ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/mark-gl-posted")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkGeneralLedgerPosted(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var success = await _financialTransactionService.MarkGeneralLedgerPostedAsync(id, userId);

            if (!success)
            {
                return NotFound(ApiResponse.ErrorResponse("Financial transaction not found"));
            }

            _logger.LogInformation("Financial transaction {Id} marked as GL posted by user {UserId}", id, userId);

            return Ok(ApiResponse.SuccessResponse("General ledger posted status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking financial transaction {Id} as GL posted", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating general ledger posted status"));
        }
    }
}

/// <summary>
/// Request for voiding a financial transaction
/// </summary>
public class VoidFinancialTransactionRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
