using DijaGoldPOS.API.DTOs;

using DijaGoldPOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Reports controller for generating business reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        IAuditService auditService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Generate daily sales summary report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Report date</param>
    /// <returns>Daily sales summary</returns>
    [HttpGet("daily-sales-summary")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<DailySalesSummaryReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySalesSummary([FromQuery] int branchId, [FromQuery] DateTime date)
    {
        try
        {
            var report = await _reportService.GetDailySalesSummaryAsync(branchId, date);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "DailySalesSummary",
                $"Generated daily sales summary for branch {branchId} on {date:yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<DailySalesSummaryReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily sales summary report for branch {BranchId} on {Date}", branchId, date);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the daily sales summary report"));
        }
    }

    /// <summary>
    /// Generate cash reconciliation report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Report date</param>
    /// <returns>Cash reconciliation report</returns>
    [HttpGet("cash-reconciliation")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CashReconciliationReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCashReconciliation([FromQuery] int branchId, [FromQuery] DateTime date)
    {
        try
        {
            var report = await _reportService.GetCashReconciliationAsync(branchId, date);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "CashReconciliation",
                $"Generated cash reconciliation for branch {branchId} on {date:yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<CashReconciliationReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cash reconciliation report for branch {BranchId} on {Date}", branchId, date);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the cash reconciliation report"));
        }
    }

    /// <summary>
    /// Generate inventory movement report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Inventory movement report</returns>
    [HttpGet("inventory-movement")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<InventoryMovementReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryMovementReport(
        [FromQuery] int branchId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var report = await _reportService.GetInventoryMovementReportAsync(branchId, fromDate, toDate);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "InventoryMovement",
                $"Generated inventory movement report for branch {branchId} from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<InventoryMovementReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory movement report for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the inventory movement report"));
        }
    }

    /// <summary>
    /// Generate profit analysis report
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="categoryTypeId">Category type ID (optional)</param>
    /// <returns>Profit analysis report</returns>
    [HttpGet("profit-analysis")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProfitAnalysisReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfitAnalysisReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int? branchId = null,
        [FromQuery] int? categoryTypeId = null)
    {
        try
        {
            var report = await _reportService.GetProfitAnalysisReportAsync(branchId, fromDate, toDate, categoryTypeId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "ProfitAnalysis",
                $"Generated profit analysis report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<ProfitAnalysisReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating profit analysis report");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the profit analysis report"));
        }
    }

    /// <summary>
    /// Generate customer analysis report
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="topCustomersCount">Number of top customers to show</param>
    /// <returns>Customer analysis report</returns>
    [HttpGet("customer-analysis")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAnalysisReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAnalysisReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int? branchId = null,
        [FromQuery] int topCustomersCount = 20)
    {
        try
        {
            var report = await _reportService.GetCustomerAnalysisReportAsync(branchId, fromDate, toDate, topCustomersCount);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "CustomerAnalysis",
                $"Generated customer analysis report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<CustomerAnalysisReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer analysis report");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the customer analysis report"));
        }
    }

    /// <summary>
    /// Generate supplier balance report
    /// </summary>
    /// <param name="asOfDate">As of date (optional, defaults to today)</param>
    /// <returns>Supplier balance report</returns>
    [HttpGet("supplier-balance")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<SupplierBalanceReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierBalanceReport([FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var report = await _reportService.GetSupplierBalanceReportAsync(asOfDate);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "SupplierBalance",
                $"Generated supplier balance report as of {(asOfDate ?? DateTime.UtcNow):yyyy-MM-dd}"
            );

            return Ok(ApiResponse<SupplierBalanceReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating supplier balance report");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the supplier balance report"));
        }
    }

    /// <summary>
    /// Generate inventory valuation report
    /// </summary>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="asOfDate">As of date (optional, defaults to today)</param>
    /// <returns>Inventory valuation report</returns>
    [HttpGet("inventory-valuation")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<InventoryValuationReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryValuationReport(
        [FromQuery] int? branchId = null,
        [FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var report = await _reportService.GetInventoryValuationReportAsync(branchId, asOfDate);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "InventoryValuation",
                $"Generated inventory valuation report as of {(asOfDate ?? DateTime.UtcNow):yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<InventoryValuationReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory valuation report");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the inventory valuation report"));
        }
    }

    /// <summary>
    /// Generate tax report
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <returns>Tax report</returns>
    [HttpGet("tax-report")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<TaxReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int? branchId = null)
    {
        try
        {
            var report = await _reportService.GetTaxReportAsync(branchId, fromDate, toDate);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "TaxReport",
                $"Generated tax report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<TaxReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tax report");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the tax report"));
        }
    }

    /// <summary>
    /// Generate transaction log report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Report date</param>
    /// <returns>Transaction log report</returns>
    [HttpGet("transaction-log")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TransactionLogReport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionLogReport([FromQuery] int branchId, [FromQuery] DateTime date)
    {
        try
        {
            var report = await _reportService.GetTransactionLogReportAsync(branchId, date);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "GENERATE_REPORT",
                "Report",
                "TransactionLog",
                $"Generated transaction log report for branch {branchId} on {date:yyyy-MM-dd}",
                branchId: branchId
            );

            return Ok(ApiResponse<TransactionLogReport>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transaction log report for branch {BranchId} on {Date}", branchId, date);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the transaction log report"));
        }
    }



    /// <summary>
    /// Get available report types
    /// </summary>
    /// <returns>List of available report types</returns>
    [HttpGet("types")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<ReportTypeDto>>), StatusCodes.Status200OK)]
    public IActionResult GetReportTypes()
    {
        try
        {
            var reportTypes = new List<ReportTypeDto>
            {
                new ReportTypeDto { Code = "daily-sales-summary", Name = "Daily Sales Summary", RequiresManagerRole = false },
                new ReportTypeDto { Code = "cash-reconciliation", Name = "Cash Reconciliation", RequiresManagerRole = false },
                new ReportTypeDto { Code = "inventory-movement", Name = "Inventory Movement", RequiresManagerRole = false },
                new ReportTypeDto { Code = "transaction-log", Name = "Transaction Log", RequiresManagerRole = false },
                new ReportTypeDto { Code = "profit-analysis", Name = "Profit Analysis", RequiresManagerRole = true },
                new ReportTypeDto { Code = "customer-analysis", Name = "Customer Analysis", RequiresManagerRole = true },
                new ReportTypeDto { Code = "supplier-balance", Name = "Supplier Balance", RequiresManagerRole = true },
                new ReportTypeDto { Code = "inventory-valuation", Name = "Inventory Valuation", RequiresManagerRole = true },
                new ReportTypeDto { Code = "tax-report", Name = "Tax Report", RequiresManagerRole = true }
            };

            return Ok(ApiResponse<List<ReportTypeDto>>.SuccessResponse(reportTypes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report types");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving report types"));
        }
    }
}

/// <summary>
/// Report type DTO
/// </summary>
public class ReportTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool RequiresManagerRole { get; set; }
}
