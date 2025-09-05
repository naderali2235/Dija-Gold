using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.ManfacturingModels;
using DijaGoldPOS.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for generating manufacturing reports and analytics
/// </summary>
public class ManufacturingReportsService : IManufacturingReportsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ManufacturingReportsService> _logger;

    public ManufacturingReportsService(
        IUnitOfWork unitOfWork,
        ILogger<ManufacturingReportsService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Gets raw gold utilization report
    /// </summary>
    public async Task<RawGoldUtilizationReportDto> GetRawGoldUtilizationReportAsync(DateTime startDate, DateTime endDate, int? supplierId = null)
    {
        try
        {
            var query = _unitOfWork.Repository<ProductManufacture>().GetQueryable()
                .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                    .ThenInclude(rgpoi => rgpoi.RawGoldPurchaseOrder)
                        .ThenInclude(rgpo => rgpo.Supplier)
                .Include(pm => pm.Product)
                .Where(pm => pm.ManufactureDate >= startDate && pm.ManufactureDate <= endDate)
                .AsQueryable();

            if (supplierId.HasValue)
            {
                query = query.Where(pm => pm.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrder.SupplierId == supplierId.Value);
            }

            var records = await query.ToListAsync();

            var report = new RawGoldUtilizationReportDto
            {
                ReportPeriod = new DateRangeDto { StartDate = startDate, EndDate = endDate },
                TotalRawGoldPurchased = records.Sum(r => r.SourceRawGoldPurchaseOrderItem.WeightReceived),
                TotalRawGoldConsumed = records.Sum(r => r.ConsumedWeight),
                TotalWastage = records.Sum(r => r.WastageWeight),
                RawGoldUtilizationRate = 0,
                TotalProductsManufactured = records.Count,
                AverageWastageRate = 0,
                BySupplier = new List<SupplierRawGoldUtilizationDto>(),
                ByKaratType = new List<KaratTypeUtilizationDto>(),
                ByProductType = new List<ProductTypeUtilizationDto>()
            };

            // Calculate rates
            if (report.TotalRawGoldPurchased > 0)
            {
                report.RawGoldUtilizationRate = (report.TotalRawGoldConsumed / report.TotalRawGoldPurchased) * 100;
                report.AverageWastageRate = (report.TotalWastage / report.TotalRawGoldPurchased) * 100;
            }

            // Group by supplier
            report.BySupplier = records
                .GroupBy(r => new { r.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrder.SupplierId, r.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrder.Supplier.CompanyName })
                .Select(g => new SupplierRawGoldUtilizationDto
                {
                    SupplierId = g.Key.SupplierId,
                    SupplierName = g.Key.CompanyName,
                    RawGoldPurchased = g.Sum(r => r.SourceRawGoldPurchaseOrderItem.WeightReceived),
                    RawGoldConsumed = g.Sum(r => r.ConsumedWeight),
                    Wastage = g.Sum(r => r.WastageWeight),
                    UtilizationRate = g.Sum(r => r.SourceRawGoldPurchaseOrderItem.WeightReceived) > 0
                        ? (g.Sum(r => r.ConsumedWeight) / g.Sum(r => r.SourceRawGoldPurchaseOrderItem.WeightReceived)) * 100
                        : 0
                })
                .OrderByDescending(s => s.RawGoldPurchased)
                .ToList();

            // Group by karat type
            report.ByKaratType = records
                .GroupBy(r => "Unknown") // Raw gold karat types now handled by separate raw gold system
                .Select(g => new KaratTypeUtilizationDto
                {
                    KaratType = g.Key,
                    RawGoldPurchased = g.Sum(r => r.SourceRawGoldPurchaseOrderItem.WeightReceived),
                    RawGoldConsumed = g.Sum(r => r.ConsumedWeight),
                    Wastage = g.Sum(r => r.WastageWeight),
                    UtilizationRate = g.Sum(r => r.SourceRawGoldPurchaseOrderItem.WeightReceived) > 0
                        ? (g.Sum(r => r.ConsumedWeight) / g.Sum(r => r.SourceRawGoldPurchaseOrderItem.WeightReceived)) * 100
                        : 0
                })
                .OrderByDescending(k => k.RawGoldPurchased)
                .ToList();

            // Group by product type
            report.ByProductType = records
                .GroupBy(r => r.Product.CategoryType?.Name ?? "Unknown")
                .Select(g => new ProductTypeUtilizationDto
                {
                    ProductType = g.Key,
                    RawGoldConsumed = g.Sum(r => r.ConsumedWeight),
                    ProductsManufactured = g.Count(),
                    AverageConsumptionPerProduct = g.Count() > 0 ? g.Sum(r => r.ConsumedWeight) / g.Count() : 0
                })
                .OrderByDescending(p => p.RawGoldConsumed)
                .ToList();

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating raw gold utilization report");
            throw;
        }
    }

    /// <summary>
    /// Gets manufacturing efficiency report
    /// </summary>
    public async Task<ManufacturingEfficiencyReportDto> GetManufacturingEfficiencyReportAsync(DateTime startDate, DateTime endDate, int? technicianId = null)
    {
        try
        {
            var query = _unitOfWork.Repository<ProductManufacture>().GetQueryable()
                .Include(pm => pm.Product)
                .Include(pm => pm.Branch)
                .Include(pm => pm.Technician)
                .Where(pm => pm.ManufactureDate >= startDate && pm.ManufactureDate <= endDate)
                .AsQueryable();

            if (technicianId.HasValue)
            {
                query = query.Where(pm => pm.TechnicianId == technicianId.Value);
            }

            var records = await query.ToListAsync();

            var completedRecords = records.Where(r => r.Status == "Completed").ToList();

            var report = new ManufacturingEfficiencyReportDto
            {
                ReportPeriod = new DateRangeDto { StartDate = startDate, EndDate = endDate },
                TotalManufacturingRecords = records.Count,
                CompletedRecords = completedRecords.Count,
                InProgressRecords = records.Count(r => r.Status == "InProgress"),
                PendingRecords = records.Count(r => r.Status == "Draft"),
                RejectedRecords = records.Count(r => r.Status == "Rejected"),
                OverallCompletionRate = records.Count > 0 ? (completedRecords.Count / (decimal)records.Count) * 100 : 0,
                AverageManufacturingTime = 0,
                AverageWastageRate = 0,
                AverageEfficiencyRating = 0,
                ByTechnician = new List<TechnicianEfficiencyDto>(),
                ByBranch = new List<BranchEfficiencyDto>(),
                ByPriority = new List<PriorityEfficiencyDto>(),
                EfficiencyTrend = new List<MonthlyEfficiencyDto>()
            };

            // Calculate averages
            if (completedRecords.Count > 0)
            {
                var validTimeRecords = completedRecords
                    .Where(r => r.ActualCompletionDate.HasValue && r.ManufactureDate <= r.ActualCompletionDate)
                    .ToList();

                if (validTimeRecords.Any())
                {
                    report.AverageManufacturingTime = validTimeRecords
                        .Average(r => (r.ActualCompletionDate!.Value - r.ManufactureDate).TotalHours);
                }

                var validWastageRecords = completedRecords
                    .Where(r => r.ConsumedWeight > 0)
                    .ToList();

                if (validWastageRecords.Any())
                {
                    report.AverageWastageRate = (decimal)validWastageRecords
                        .Average(r => (r.WastageWeight / r.ConsumedWeight) * 100);
                }

                var validEfficiencyRecords = completedRecords
                    .Where(r => r.EfficiencyRating.HasValue)
                    .ToList();

                if (validEfficiencyRecords.Any())
                {
                    report.AverageEfficiencyRating = (decimal)validEfficiencyRecords
                        .Average(r => r.EfficiencyRating ?? 0);
                }
            }

            // Group by technician
            report.ByTechnician = records
                .Where(r => r.Technician != null)
                .GroupBy(r => new { r.TechnicianId, r.Technician!.FullName })
                .Select(g => new TechnicianEfficiencyDto
                {
                    TechnicianId = g.Key.TechnicianId,
                    TechnicianName = g.Key.FullName,
                    TotalRecords = g.Count(),
                    CompletedRecords = g.Count(r => r.Status == "Completed"),
                    CompletionRate = g.Count() > 0 ? (g.Count(r => r.Status == "Completed") / (decimal)g.Count()) * 100 : 0,
                    AverageEfficiencyRating = (decimal)(g.Any(r => r.EfficiencyRating.HasValue)
                        ? g.Where(r => r.EfficiencyRating.HasValue).Average(r => r.EfficiencyRating ?? 0)
                        : 0)
                })
                .OrderByDescending(t => t.TotalRecords)
                .ToList();

            // Group by branch
            report.ByBranch = records
                .Where(r => r.Branch != null)
                .GroupBy(r => new { r.BranchId, r.Branch!.Name })
                .Select(g => new BranchEfficiencyDto
                {
                    BranchId = g.Key.BranchId,
                    BranchName = g.Key.Name,
                    TotalRecords = g.Count(),
                    CompletedRecords = g.Count(r => r.Status == "Completed"),
                    CompletionRate = g.Count() > 0 ? (g.Count(r => r.Status == "Completed") / (decimal)g.Count()) * 100 : 0,
                    AverageEfficiencyRating = (decimal)(g.Any(r => r.EfficiencyRating.HasValue)
                        ? g.Where(r => r.EfficiencyRating.HasValue).Average(r => r.EfficiencyRating ?? 0)
                        : 0)
                })
                .OrderByDescending(b => b.TotalRecords)
                .ToList();

            // Group by priority
            report.ByPriority = records
                .GroupBy(r => r.Priority)
                .Select(g => new PriorityEfficiencyDto
                {
                    Priority = g.Key,
                    TotalRecords = g.Count(),
                    CompletedRecords = g.Count(r => r.Status == "Completed"),
                    CompletionRate = g.Count() > 0 ? (g.Count(r => r.Status == "Completed") / (decimal)g.Count()) * 100 : 0,
                    AverageEfficiencyRating = (decimal)(g.Any(r => r.EfficiencyRating.HasValue)
                        ? g.Where(r => r.EfficiencyRating.HasValue).Average(r => r.EfficiencyRating ?? 0)
                        : 0)
                })
                .OrderByDescending(p => p.TotalRecords)
                .ToList();

            // Generate monthly trend (last 12 months)
            var trendStartDate = DateTime.Now.AddMonths(-12);
            report.EfficiencyTrend = await GenerateMonthlyEfficiencyTrendAsync(trendStartDate, DateTime.Now);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating manufacturing efficiency report");
            throw;
        }
    }

    /// <summary>
    /// Gets cost analysis report
    /// </summary>
    public async Task<CostAnalysisReportDto> GetCostAnalysisReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _unitOfWork.Repository<ProductManufacture>().GetQueryable()
                .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                    .ThenInclude(rgpoi => rgpoi.RawGoldPurchaseOrder)
                .Include(pm => pm.Product)
                .Where(pm => pm.ManufactureDate >= startDate && pm.ManufactureDate <= endDate)
                .ToListAsync();

            var report = new CostAnalysisReportDto
            {
                ReportPeriod = new DateRangeDto { StartDate = startDate, EndDate = endDate },
                TotalRawGoldCost = records.Sum(r => r.SourceRawGoldPurchaseOrderItem.UnitCostPerGram * r.ConsumedWeight),
                TotalManufacturingCost = records.Sum(r => r.TotalManufacturingCost),
                TotalWastageCost = records.Sum(r => r.SourceRawGoldPurchaseOrderItem.UnitCostPerGram * r.WastageWeight),
                AverageCostPerGram = 0,
                CostBreakdownByProductType = new List<ProductTypeCostDto>(),
                CostTrend = new List<MonthlyCostDto>(),
                TopCostProducts = new List<TopCostProductDto>()
            };

            // Calculate average cost per gram
            var totalWeight = records.Sum(r => r.ConsumedWeight);
            if (totalWeight > 0)
            {
                report.AverageCostPerGram = (report.TotalRawGoldCost + report.TotalManufacturingCost) / totalWeight;
            }

            // Group by product type
            report.CostBreakdownByProductType = records
                .GroupBy(r => r.Product.CategoryType?.Name ?? "Unknown")
                .Select(g => new ProductTypeCostDto
                {
                    ProductType = g.Key,
                    RawGoldCost = g.Sum(r => r.SourceRawGoldPurchaseOrderItem.UnitCostPerGram * r.ConsumedWeight),
                    ManufacturingCost = g.Sum(r => r.TotalManufacturingCost),
                    TotalCost = g.Sum(r => r.SourceRawGoldPurchaseOrderItem.UnitCostPerGram * r.ConsumedWeight + r.TotalManufacturingCost),
                    ProductsManufactured = g.Count(),
                    AverageCostPerProduct = g.Count() > 0 ? g.Sum(r => r.SourceRawGoldPurchaseOrderItem.UnitCostPerGram * r.ConsumedWeight + r.TotalManufacturingCost) / g.Count() : 0
                })
                .OrderByDescending(p => p.TotalCost)
                .ToList();

            // Generate monthly cost trend
            var trendStartDate = DateTime.Now.AddMonths(-12);
            report.CostTrend = await GenerateMonthlyCostTrendAsync(trendStartDate, DateTime.Now);

            // Top cost products
            report.TopCostProducts = records
                .OrderByDescending(r => r.TotalManufacturingCost)
                .Take(10)
                .Select(r => new TopCostProductDto
                {
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name,
                    ProductCode = r.Product.ProductCode,
                    TotalCost = r.TotalManufacturingCost,
                    ManufactureDate = r.ManufactureDate,
                    BatchNumber = r.BatchNumber
                })
                .ToList();

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cost analysis report");
            throw;
        }
    }

    /// <summary>
    /// Gets workflow performance report
    /// </summary>
    public async Task<WorkflowPerformanceReportDto> GetWorkflowPerformanceReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var workflowHistory = await _unitOfWork.Repository<ManufacturingWorkflowHistory>().GetQueryable()
                .Where(wh => wh.CreatedAt >= startDate && wh.CreatedAt <= endDate)
                .OrderBy(wh => wh.CreatedAt)
                .ToListAsync();

            var report = new WorkflowPerformanceReportDto
            {
                ReportPeriod = new DateRangeDto { StartDate = startDate, EndDate = endDate },
                TotalWorkflowTransitions = workflowHistory.Count,
                AverageTimeInDraft = 0,
                AverageTimeInProgress = 0,
                AverageTimeInQualityCheck = 0,
                ApprovalRate = 0,
                RejectionRate = 0,
                QualityPassRate = 0,
                WorkflowStepAnalysis = new List<WorkflowStepAnalysisDto>(),
                TransitionAnalysis = new List<WorkflowTransitionAnalysisDto>()
            };

            // Calculate workflow metrics
            var completedWorkflows = workflowHistory
                .Where(wh => wh.ToStatus == "Completed")
                .GroupBy(wh => wh.ProductManufactureId)
                .ToList();

            if (completedWorkflows.Any())
            {
                // Calculate approval and rejection rates
                var approvedCount = workflowHistory.Count(wh => wh.ToStatus == "Approved");
                var rejectedCount = workflowHistory.Count(wh => wh.ToStatus == "Rejected");
                var totalDecisions = approvedCount + rejectedCount;

                if (totalDecisions > 0)
                {
                    report.ApprovalRate = (approvedCount / (decimal)totalDecisions) * 100;
                    report.RejectionRate = (rejectedCount / (decimal)totalDecisions) * 100;
                }

                // Calculate quality pass rate
                var qualityPassed = workflowHistory.Count(wh => wh.Action.Contains("Quality") && wh.ToStatus == "Approved");
                var qualityTotal = workflowHistory.Count(wh => wh.Action.Contains("Quality"));
                if (qualityTotal > 0)
                {
                    report.QualityPassRate = (qualityPassed / (decimal)qualityTotal) * 100;
                }
            }

            // Analyze workflow steps
            var stepGroups = workflowHistory
                .GroupBy(wh => wh.ToStatus)
                .Select(g => new WorkflowStepAnalysisDto
                {
                    StepName = g.Key,
                    TotalRecords = g.Count(),
                    AverageTimeInStep = 0, // Would need more complex calculation with start/end times
                    CompletionRate = g.Count() > 0 ? (g.Count(wh => wh.ToStatus == "Completed") / (decimal)g.Count()) * 100 : 0
                })
                .ToList();

            report.WorkflowStepAnalysis = stepGroups;

            // Analyze transitions
            report.TransitionAnalysis = workflowHistory
                .GroupBy(wh => new { wh.FromStatus, wh.ToStatus })
                .Select(g => new WorkflowTransitionAnalysisDto
                {
                    FromStatus = g.Key.FromStatus,
                    ToStatus = g.Key.ToStatus,
                    TransitionCount = g.Count(),
                    AverageTransitionTime = 0, // Would need more complex calculation
                    SuccessRate = g.Count() > 0 ? (g.Count(wh => !wh.ToStatus.Contains("Reject")) / (decimal)g.Count()) * 100 : 0
                })
                .OrderByDescending(t => t.TransitionCount)
                .ToList();

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating workflow performance report");
            throw;
        }
    }

    #region Helper Methods

    private async Task<List<MonthlyEfficiencyDto>> GenerateMonthlyEfficiencyTrendAsync(DateTime startDate, DateTime endDate)
    {
        var trend = new List<MonthlyEfficiencyDto>();

        for (var date = startDate; date <= endDate; date = date.AddMonths(1))
        {
            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var records = await _unitOfWork.Repository<ProductManufacture>().GetQueryable()
                .Where(pm => pm.ManufactureDate >= monthStart && pm.ManufactureDate <= monthEnd)
                .ToListAsync();

            var completedRecords = records.Where(r => r.Status == "Completed").ToList();

            trend.Add(new MonthlyEfficiencyDto
            {
                Month = monthStart,
                TotalRecords = records.Count,
                CompletedRecords = completedRecords.Count,
                CompletionRate = records.Count > 0 ? (completedRecords.Count / (decimal)records.Count) * 100 : 0,
                AverageEfficiencyRating = (decimal)(completedRecords.Any(r => r.EfficiencyRating.HasValue)
                    ? completedRecords.Where(r => r.EfficiencyRating.HasValue).Average(r => r.EfficiencyRating ?? 0)
                    : 0)
            });
        }

        return trend;
    }

    private async Task<List<MonthlyCostDto>> GenerateMonthlyCostTrendAsync(DateTime startDate, DateTime endDate)
    {
        var trend = new List<MonthlyCostDto>();

        for (var date = startDate; date <= endDate; date = date.AddMonths(1))
        {
            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var records = await _unitOfWork.Repository<ProductManufacture>().GetQueryable()
                .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .Where(pm => pm.ManufactureDate >= monthStart && pm.ManufactureDate <= monthEnd)
                .ToListAsync();

            trend.Add(new MonthlyCostDto
            {
                Month = monthStart,
                RawGoldCost = records.Sum(r => r.SourceRawGoldPurchaseOrderItem.UnitCostPerGram * r.ConsumedWeight),
                ManufacturingCost = records.Sum(r => r.TotalManufacturingCost),
                TotalCost = records.Sum(r => r.SourceRawGoldPurchaseOrderItem.UnitCostPerGram * r.ConsumedWeight + r.TotalManufacturingCost),
                ProductsManufactured = records.Count
            });
        }

        return trend;
    }

    #endregion
}
