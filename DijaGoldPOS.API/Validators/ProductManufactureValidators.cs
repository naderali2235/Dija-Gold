using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateProductManufactureDtoValidator : AbstractValidator<CreateProductManufactureDto>
{
    public CreateProductManufactureDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.SourcePurchaseOrderItemId)
            .GreaterThan(0);

        RuleFor(x => x.ConsumedWeight)
            .InclusiveBetween(0.001m, 999999.999m);

        RuleFor(x => x.WastageWeight)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ManufactureDate)
            .NotEmpty();

        RuleFor(x => x.ManufacturingCostPerGram)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalManufacturingCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BatchNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.BatchNumber));

        RuleFor(x => x.ManufacturingNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.ManufacturingNotes));

        RuleFor(x => x.Status)
            .MaximumLength(20);

        RuleFor(x => x.BranchId)
            .GreaterThan(0);

        RuleFor(x => x.TechnicianId)
            .GreaterThan(0)
            .When(x => x.TechnicianId.HasValue);

        RuleFor(x => x.Priority)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));

        RuleFor(x => x.EstimatedCompletionDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.EstimatedCompletionDate.HasValue);

        // Business rule validation
        RuleFor(x => x)
            .Must(x => x.ConsumedWeight >= x.WastageWeight)
            .WithMessage("Consumed weight must be greater than or equal to wastage weight");
    }
}

public class UpdateProductManufactureDtoValidator : AbstractValidator<UpdateProductManufactureDto>
{
    public UpdateProductManufactureDtoValidator()
    {
        RuleFor(x => x.ConsumedWeight)
            .InclusiveBetween(0.001m, 999999.999m)
            .When(x => x.ConsumedWeight.HasValue);

        RuleFor(x => x.WastageWeight)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WastageWeight.HasValue);

        RuleFor(x => x.ManufacturingCostPerGram)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ManufacturingCostPerGram.HasValue);

        RuleFor(x => x.TotalManufacturingCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalManufacturingCost.HasValue);

        RuleFor(x => x.BatchNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.BatchNumber));

        RuleFor(x => x.ManufacturingNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.ManufacturingNotes));

        RuleFor(x => x.Status)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        // Business rule validation
        RuleFor(x => x)
            .Must(x =>
            {
                if (!x.ConsumedWeight.HasValue || !x.WastageWeight.HasValue)
                    return true;
                return x.ConsumedWeight.Value >= x.WastageWeight.Value;
            })
            .WithMessage("Consumed weight must be greater than or equal to wastage weight");
    }
}

public class WorkflowTransitionDtoValidator : AbstractValidator<WorkflowTransitionDto>
{
    public WorkflowTransitionDtoValidator()
    {
        RuleFor(x => x.TargetStatus)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class QualityCheckDtoValidator : AbstractValidator<QualityCheckDto>
{
    public QualityCheckDtoValidator()
    {
        RuleFor(x => x.Passed)
            .NotNull();

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class FinalApprovalDtoValidator : AbstractValidator<FinalApprovalDto>
{
    public FinalApprovalDtoValidator()
    {
        RuleFor(x => x.Approved)
            .NotNull();

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
