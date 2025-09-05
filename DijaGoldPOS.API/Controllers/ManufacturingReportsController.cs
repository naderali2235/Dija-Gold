using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for manufacturing reports and analytics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ManufacturingReportsController : ControllerBase
{
    private readonly IManufacturingReportsService _reportsService;
    private readonly ILogger<ManufacturingReportsController> _logger;

    public ManufacturingReportsController(
        IManufacturingReportsService reportsService,
        ILogger<ManufacturingReportsController> logger)
    {
        _reportsService = reportsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets raw gold utilization report
    /// </summary>
    [HttpGet("raw-gold-utilization")]
    public async Task<ActionResult<RawGoldUtilizationReportDto>> GetRawGoldUtilizationReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? supplierId = null)
    {
        try
        {
            var report = await _reportsService.GetRawGoldUtilizationReportAsync(startDate, endDate, supplierId);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating raw gold utilization report");
            return StatusCode(500, new { error = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Gets manufacturing efficiency report
    /// </summary>
    [HttpGet("efficiency")]
    public async Task<ActionResult<ManufacturingEfficiencyReportDto>> GetManufacturingEfficiencyReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? technicianId = null)
    {
        try
        {
            var report = await _reportsService.GetManufacturingEfficiencyReportAsync(startDate, endDate, technicianId);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating manufacturing efficiency report");
            return StatusCode(500, new { error = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Gets cost analysis report
    /// </summary>
    [HttpGet("cost-analysis")]
    public async Task<ActionResult<CostAnalysisReportDto>> GetCostAnalysisReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var report = await _reportsService.GetCostAnalysisReportAsync(startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cost analysis report");
            return StatusCode(500, new { error = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Gets workflow performance report
    /// </summary>
    [HttpGet("workflow-performance")]
    public async Task<ActionResult<WorkflowPerformanceReportDto>> GetWorkflowPerformanceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var report = await _reportsService.GetWorkflowPerformanceReportAsync(startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating workflow performance report");
            return StatusCode(500, new { error = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Gets comprehensive manufacturing dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ManufacturingDashboardDto>> GetManufacturingDashboard(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var rawGoldReport = await _reportsService.GetRawGoldUtilizationReportAsync(startDate, endDate);
            var efficiencyReport = await _reportsService.GetManufacturingEfficiencyReportAsync(startDate, endDate);
            var costReport = await _reportsService.GetCostAnalysisReportAsync(startDate, endDate);
            var workflowReport = await _reportsService.GetWorkflowPerformanceReportAsync(startDate, endDate);

            var dashboard = new ManufacturingDashboardDto
            {
                ReportPeriod = new DateRangeDto { StartDate = startDate, EndDate = endDate },
                Summary = new ManufacturingSummaryDto
                {
                    TotalRawGoldPurchased = rawGoldReport.TotalRawGoldPurchased,
                    TotalRawGoldConsumed = rawGoldReport.TotalRawGoldConsumed,
                    TotalWastage = rawGoldReport.TotalWastage,
                    TotalProductsManufactured = rawGoldReport.TotalProductsManufactured,
                    RawGoldUtilizationRate = rawGoldReport.RawGoldUtilizationRate,
                    OverallCompletionRate = efficiencyReport.OverallCompletionRate,
                    TotalManufacturingCost = costReport.TotalManufacturingCost,
                    TotalRawGoldCost = costReport.TotalRawGoldCost,
                    AverageCostPerGram = costReport.AverageCostPerGram,
                    ApprovalRate = workflowReport.ApprovalRate,
                    QualityPassRate = workflowReport.QualityPassRate
                },
                RawGoldUtilization = rawGoldReport,
                Efficiency = efficiencyReport,
                CostAnalysis = costReport,
                WorkflowPerformance = workflowReport
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating manufacturing dashboard");
            return StatusCode(500, new { error = "An error occurred while generating the dashboard" });
        }
    }
}
