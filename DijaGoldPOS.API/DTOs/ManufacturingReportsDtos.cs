

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Date range DTO
/// </summary>
public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Raw Gold Utilization Report DTO
/// </summary>
public class RawGoldUtilizationReportDto
{
    public DateRangeDto ReportPeriod { get; set; } = new();
    public decimal TotalRawGoldPurchased { get; set; }
    public decimal TotalRawGoldConsumed { get; set; }
    public decimal TotalWastage { get; set; }
    public decimal RawGoldUtilizationRate { get; set; }
    public int TotalProductsManufactured { get; set; }
    public decimal AverageWastageRate { get; set; }
    public List<SupplierRawGoldUtilizationDto> BySupplier { get; set; } = new();
    public List<KaratTypeUtilizationDto> ByKaratType { get; set; } = new();
    public List<ProductTypeUtilizationDto> ByProductType { get; set; } = new();
}

/// <summary>
/// Supplier Raw Gold Utilization DTO
/// </summary>
public class SupplierRawGoldUtilizationDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal RawGoldPurchased { get; set; }
    public decimal RawGoldConsumed { get; set; }
    public decimal Wastage { get; set; }
    public decimal UtilizationRate { get; set; }
}

/// <summary>
/// Karat Type Utilization DTO
/// </summary>
public class KaratTypeUtilizationDto
{
    public string KaratType { get; set; } = string.Empty;
    public decimal RawGoldPurchased { get; set; }
    public decimal RawGoldConsumed { get; set; }
    public decimal Wastage { get; set; }
    public decimal UtilizationRate { get; set; }
}

/// <summary>
/// Product Type Utilization DTO
/// </summary>
public class ProductTypeUtilizationDto
{
    public string ProductType { get; set; } = string.Empty;
    public decimal RawGoldConsumed { get; set; }
    public int ProductsManufactured { get; set; }
    public decimal AverageConsumptionPerProduct { get; set; }
}

/// <summary>
/// Manufacturing Efficiency Report DTO
/// </summary>
public class ManufacturingEfficiencyReportDto
{
    public DateRangeDto ReportPeriod { get; set; } = new();
    public int TotalManufacturingRecords { get; set; }
    public int CompletedRecords { get; set; }
    public int InProgressRecords { get; set; }
    public int PendingRecords { get; set; }
    public int RejectedRecords { get; set; }
    public decimal OverallCompletionRate { get; set; }
    public double AverageManufacturingTime { get; set; } // Hours
    public decimal AverageWastageRate { get; set; }
    public decimal AverageEfficiencyRating { get; set; }
    public List<TechnicianEfficiencyDto> ByTechnician { get; set; } = new();
    public List<BranchEfficiencyDto> ByBranch { get; set; } = new();
    public List<PriorityEfficiencyDto> ByPriority { get; set; } = new();
    public List<MonthlyEfficiencyDto> EfficiencyTrend { get; set; } = new();
}

/// <summary>
/// Technician Efficiency DTO
/// </summary>
public class TechnicianEfficiencyDto
{
    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int CompletedRecords { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageEfficiencyRating { get; set; }
}

/// <summary>
/// Branch Efficiency DTO
/// </summary>
public class BranchEfficiencyDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int CompletedRecords { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageEfficiencyRating { get; set; }
}

/// <summary>
/// Priority Efficiency DTO
/// </summary>
public class PriorityEfficiencyDto
{
    public string Priority { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int CompletedRecords { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageEfficiencyRating { get; set; }
}

/// <summary>
/// Monthly Efficiency DTO
/// </summary>
public class MonthlyEfficiencyDto
{
    public DateTime Month { get; set; }
    public int TotalRecords { get; set; }
    public int CompletedRecords { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageEfficiencyRating { get; set; }
}

/// <summary>
/// Cost Analysis Report DTO
/// </summary>
public class CostAnalysisReportDto
{
    public DateRangeDto ReportPeriod { get; set; } = new();
    public decimal TotalRawGoldCost { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public decimal TotalWastageCost { get; set; }
    public decimal AverageCostPerGram { get; set; }
    public List<ProductTypeCostDto> CostBreakdownByProductType { get; set; } = new();
    public List<MonthlyCostDto> CostTrend { get; set; } = new();
    public List<TopCostProductDto> TopCostProducts { get; set; } = new();
}

/// <summary>
/// Product Type Cost DTO
/// </summary>
public class ProductTypeCostDto
{
    public string ProductType { get; set; } = string.Empty;
    public decimal RawGoldCost { get; set; }
    public decimal ManufacturingCost { get; set; }
    public decimal TotalCost { get; set; }
    public int ProductsManufactured { get; set; }
    public decimal AverageCostPerProduct { get; set; }
}

/// <summary>
/// Monthly Cost DTO
/// </summary>
public class MonthlyCostDto
{
    public DateTime Month { get; set; }
    public decimal RawGoldCost { get; set; }
    public decimal ManufacturingCost { get; set; }
    public decimal TotalCost { get; set; }
    public int ProductsManufactured { get; set; }
}

/// <summary>
/// Top Cost Product DTO
/// </summary>
public class TopCostProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public DateTime ManufactureDate { get; set; }
    public string? BatchNumber { get; set; }
}

/// <summary>
/// Workflow Performance Report DTO
/// </summary>
public class WorkflowPerformanceReportDto
{
    public DateRangeDto ReportPeriod { get; set; } = new();
    public int TotalWorkflowTransitions { get; set; }
    public double AverageTimeInDraft { get; set; } // Hours
    public double AverageTimeInProgress { get; set; } // Hours
    public double AverageTimeInQualityCheck { get; set; } // Hours
    public decimal ApprovalRate { get; set; }
    public decimal RejectionRate { get; set; }
    public decimal QualityPassRate { get; set; }
    public List<WorkflowStepAnalysisDto> WorkflowStepAnalysis { get; set; } = new();
    public List<WorkflowTransitionAnalysisDto> TransitionAnalysis { get; set; } = new();
}

/// <summary>
/// Workflow Step Analysis DTO
/// </summary>
public class WorkflowStepAnalysisDto
{
    public string StepName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public double AverageTimeInStep { get; set; } // Hours
    public decimal CompletionRate { get; set; }
}

/// <summary>
/// Workflow Transition Analysis DTO
/// </summary>
public class WorkflowTransitionAnalysisDto
{
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public int TransitionCount { get; set; }
    public double AverageTransitionTime { get; set; } // Hours
    public decimal SuccessRate { get; set; }
}

/// <summary>
/// Manufacturing Dashboard DTO
/// </summary>
public class ManufacturingDashboardDto
{
    public DateRangeDto ReportPeriod { get; set; } = new();
    public ManufacturingSummaryDto Summary { get; set; } = new();
    public RawGoldUtilizationReportDto RawGoldUtilization { get; set; } = new();
    public ManufacturingEfficiencyReportDto Efficiency { get; set; } = new();
    public CostAnalysisReportDto CostAnalysis { get; set; } = new();
    public WorkflowPerformanceReportDto WorkflowPerformance { get; set; } = new();
}

/// <summary>
/// Manufacturing Summary DTO
/// </summary>
public class ManufacturingSummaryDto
{
    public decimal TotalRawGoldPurchased { get; set; }
    public decimal TotalRawGoldConsumed { get; set; }
    public decimal TotalWastage { get; set; }
    public int TotalProductsManufactured { get; set; }
    public decimal RawGoldUtilizationRate { get; set; }
    public decimal OverallCompletionRate { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public decimal TotalRawGoldCost { get; set; }
    public decimal AverageCostPerGram { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal QualityPassRate { get; set; }
}
