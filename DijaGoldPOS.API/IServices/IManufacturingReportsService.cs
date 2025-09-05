using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Interface for manufacturing reports and analytics
/// </summary>
public interface IManufacturingReportsService
{
    /// <summary>
    /// Gets raw gold utilization report
    /// </summary>
    Task<RawGoldUtilizationReportDto> GetRawGoldUtilizationReportAsync(DateTime startDate, DateTime endDate, int? supplierId = null);

    /// <summary>
    /// Gets manufacturing efficiency report
    /// </summary>
    Task<ManufacturingEfficiencyReportDto> GetManufacturingEfficiencyReportAsync(DateTime startDate, DateTime endDate, int? technicianId = null);

    /// <summary>
    /// Gets cost analysis report
    /// </summary>
    Task<CostAnalysisReportDto> GetCostAnalysisReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets workflow performance report
    /// </summary>
    Task<WorkflowPerformanceReportDto> GetWorkflowPerformanceReportAsync(DateTime startDate, DateTime endDate);
}
