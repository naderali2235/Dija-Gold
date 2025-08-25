using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class DateRangeDtoValidator : AbstractValidator<DateRangeDto>
{
    public DateRangeDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .LessThanOrEqualTo(x => x.EndDate);

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate);

        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 1 year");
    }
}

public class ManufacturingReportRequestDtoValidator : AbstractValidator<ManufacturingReportRequestDto>
{
    public ManufacturingReportRequestDtoValidator()
    {
        RuleFor(x => x.ReportPeriod)
            .NotNull()
            .SetValidator(new DateRangeDtoValidator());

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.KaratTypeId)
            .GreaterThan(0)
            .When(x => x.KaratTypeId.HasValue);

        RuleFor(x => x.ProductTypeId)
            .GreaterThan(0)
            .When(x => x.ProductTypeId.HasValue);

        RuleFor(x => x.TechnicianId)
            .GreaterThan(0)
            .When(x => x.TechnicianId.HasValue);
    }
}

public class RawGoldUtilizationReportDtoValidator : AbstractValidator<RawGoldUtilizationReportDto>
{
    public RawGoldUtilizationReportDtoValidator()
    {
        RuleFor(x => x.ReportPeriod)
            .NotNull()
            .SetValidator(new DateRangeDtoValidator());

        RuleFor(x => x.TotalRawGoldPurchased)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalRawGoldConsumed)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalWastage)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RawGoldUtilizationRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.TotalProductsManufactured)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageWastageRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.BySupplier)
            .NotNull();

        RuleForEach(x => x.BySupplier)
            .SetValidator(new SupplierRawGoldUtilizationDtoValidator());

        RuleFor(x => x.ByKaratType)
            .NotNull();

        RuleForEach(x => x.ByKaratType)
            .SetValidator(new KaratTypeUtilizationDtoValidator());

        RuleFor(x => x.ByProductType)
            .NotNull();

        RuleForEach(x => x.ByProductType)
            .SetValidator(new ProductTypeUtilizationDtoValidator());
    }
}

public class SupplierRawGoldUtilizationDtoValidator : AbstractValidator<SupplierRawGoldUtilizationDto>
{
    public SupplierRawGoldUtilizationDtoValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0);

        RuleFor(x => x.SupplierName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.RawGoldPurchased)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RawGoldConsumed)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Wastage)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.UtilizationRate)
            .InclusiveBetween(0, 1); // 0% to 100%
    }
}

public class KaratTypeUtilizationDtoValidator : AbstractValidator<KaratTypeUtilizationDto>
{
    public KaratTypeUtilizationDtoValidator()
    {
        RuleFor(x => x.KaratType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.RawGoldPurchased)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RawGoldConsumed)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Wastage)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.UtilizationRate)
            .InclusiveBetween(0, 1); // 0% to 100%
    }
}

public class ProductTypeUtilizationDtoValidator : AbstractValidator<ProductTypeUtilizationDto>
{
    public ProductTypeUtilizationDtoValidator()
    {
        RuleFor(x => x.ProductType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.RawGoldConsumed)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ProductsManufactured)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageConsumptionPerProduct)
            .GreaterThanOrEqualTo(0);
    }
}

public class ManufacturingEfficiencyReportDtoValidator : AbstractValidator<ManufacturingEfficiencyReportDto>
{
    public ManufacturingEfficiencyReportDtoValidator()
    {
        RuleFor(x => x.ReportPeriod)
            .NotNull()
            .SetValidator(new DateRangeDtoValidator());

        RuleFor(x => x.TotalManufacturingRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletedRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.InProgressRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PendingRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RejectedRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.OverallCompletionRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.AverageManufacturingTime)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageWastageRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.AverageEfficiencyRating)
            .InclusiveBetween(0, 5); // 0 to 5 star rating

        RuleFor(x => x.ByTechnician)
            .NotNull();

        RuleForEach(x => x.ByTechnician)
            .SetValidator(new TechnicianEfficiencyDtoValidator());

        RuleFor(x => x.ByBranch)
            .NotNull();

        RuleForEach(x => x.ByBranch)
            .SetValidator(new BranchEfficiencyDtoValidator());

        RuleFor(x => x.ByPriority)
            .NotNull();

        RuleForEach(x => x.ByPriority)
            .SetValidator(new PriorityEfficiencyDtoValidator());

        RuleFor(x => x.EfficiencyTrend)
            .NotNull();

        RuleForEach(x => x.EfficiencyTrend)
            .SetValidator(new MonthlyEfficiencyDtoValidator());
    }
}

public class TechnicianEfficiencyDtoValidator : AbstractValidator<TechnicianEfficiencyDto>
{
    public TechnicianEfficiencyDtoValidator()
    {
        RuleFor(x => x.TechnicianId)
            .GreaterThan(0);

        RuleFor(x => x.TechnicianName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TotalRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletedRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletionRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.AverageEfficiencyRating)
            .InclusiveBetween(0, 5); // 0 to 5 star rating
    }
}

public class BranchEfficiencyDtoValidator : AbstractValidator<BranchEfficiencyDto>
{
    public BranchEfficiencyDtoValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0);

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TotalRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletedRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletionRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.AverageEfficiencyRating)
            .InclusiveBetween(0, 5); // 0 to 5 star rating
    }
}

public class PriorityEfficiencyDtoValidator : AbstractValidator<PriorityEfficiencyDto>
{
    public PriorityEfficiencyDtoValidator()
    {
        RuleFor(x => x.Priority)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.TotalRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletedRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletionRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.AverageEfficiencyRating)
            .InclusiveBetween(0, 5); // 0 to 5 star rating
    }
}

public class MonthlyEfficiencyDtoValidator : AbstractValidator<MonthlyEfficiencyDto>
{
    public MonthlyEfficiencyDtoValidator()
    {
        RuleFor(x => x.Month)
            .NotEmpty();

        RuleFor(x => x.TotalRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletedRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletionRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.AverageEfficiencyRating)
            .InclusiveBetween(0, 5); // 0 to 5 star rating
    }
}

public class CostAnalysisReportDtoValidator : AbstractValidator<CostAnalysisReportDto>
{
    public CostAnalysisReportDtoValidator()
    {
        RuleFor(x => x.ReportPeriod)
            .NotNull()
            .SetValidator(new DateRangeDtoValidator());

        RuleFor(x => x.TotalRawGoldCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalManufacturingCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalWastageCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageCostPerGram)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CostBreakdownByProductType)
            .NotNull();

        RuleForEach(x => x.CostBreakdownByProductType)
            .SetValidator(new ProductTypeCostDtoValidator());

        RuleFor(x => x.CostTrend)
            .NotNull();

        RuleForEach(x => x.CostTrend)
            .SetValidator(new MonthlyCostDtoValidator());

        RuleFor(x => x.TopCostProducts)
            .NotNull();

        RuleForEach(x => x.TopCostProducts)
            .SetValidator(new TopCostProductDtoValidator());
    }
}

public class ProductTypeCostDtoValidator : AbstractValidator<ProductTypeCostDto>
{
    public ProductTypeCostDtoValidator()
    {
        RuleFor(x => x.ProductType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.RawGoldCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ManufacturingCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ProductsManufactured)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageCostPerProduct)
            .GreaterThanOrEqualTo(0);
    }
}

public class MonthlyCostDtoValidator : AbstractValidator<MonthlyCostDto>
{
    public MonthlyCostDtoValidator()
    {
        RuleFor(x => x.Month)
            .NotEmpty();

        RuleFor(x => x.RawGoldCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ManufacturingCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ProductsManufactured)
            .GreaterThanOrEqualTo(0);
    }
}

public class TopCostProductDtoValidator : AbstractValidator<TopCostProductDto>
{
    public TopCostProductDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TotalCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ManufactureDate)
            .NotEmpty();

        RuleFor(x => x.BatchNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.BatchNumber));
    }
}

public class WorkflowPerformanceReportDtoValidator : AbstractValidator<WorkflowPerformanceReportDto>
{
    public WorkflowPerformanceReportDtoValidator()
    {
        RuleFor(x => x.ReportPeriod)
            .NotNull()
            .SetValidator(new DateRangeDtoValidator());

        RuleFor(x => x.TotalWorkflowTransitions)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageTimeInDraft)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageTimeInProgress)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageTimeInQualityCheck)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ApprovalRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.RejectionRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.QualityPassRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.WorkflowStepAnalysis)
            .NotNull();

        RuleForEach(x => x.WorkflowStepAnalysis)
            .SetValidator(new WorkflowStepAnalysisDtoValidator());

        RuleFor(x => x.TransitionAnalysis)
            .NotNull();

        RuleForEach(x => x.TransitionAnalysis)
            .SetValidator(new WorkflowTransitionAnalysisDtoValidator());
    }
}

public class WorkflowStepAnalysisDtoValidator : AbstractValidator<WorkflowStepAnalysisDto>
{
    public WorkflowStepAnalysisDtoValidator()
    {
        RuleFor(x => x.StepName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TotalRecords)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageTimeInStep)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CompletionRate)
            .InclusiveBetween(0, 1); // 0% to 100%
    }
}

public class WorkflowTransitionAnalysisDtoValidator : AbstractValidator<WorkflowTransitionAnalysisDto>
{
    public WorkflowTransitionAnalysisDtoValidator()
    {
        RuleFor(x => x.FromStatus)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.ToStatus)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.TransitionCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageTransitionTime)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SuccessRate)
            .InclusiveBetween(0, 1); // 0% to 100%
    }
}

public class ManufacturingSummaryDtoValidator : AbstractValidator<ManufacturingSummaryDto>
{
    public ManufacturingSummaryDtoValidator()
    {
        RuleFor(x => x.TotalRawGoldPurchased)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalRawGoldConsumed)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalWastage)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalProductsManufactured)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RawGoldUtilizationRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.OverallCompletionRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.TotalManufacturingCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalRawGoldCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageCostPerGram)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ApprovalRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.QualityPassRate)
            .InclusiveBetween(0, 1); // 0% to 100%
    }
}

// Placeholder class for the request DTO that would be used to request manufacturing reports
public class ManufacturingReportRequestDto
{
    public DateRangeDto ReportPeriod { get; set; } = new();
    public int? BranchId { get; set; }
    public int? SupplierId { get; set; }
    public int? KaratTypeId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? TechnicianId { get; set; }
    public string? Priority { get; set; }
}
