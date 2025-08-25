using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateRepairJobRequestDtoValidator : AbstractValidator<CreateRepairJobRequestDto>
{
    public CreateRepairJobRequestDtoValidator()
    {
        RuleFor(x => x.FinancialTransactionId)
            .GreaterThan(0);

        RuleFor(x => x.PriorityId)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.AssignedTechnicianId)
            .GreaterThan(0)
            .When(x => x.AssignedTechnicianId.HasValue);

        RuleFor(x => x.TechnicianNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TechnicianNotes));
    }
}

public class UpdateRepairJobStatusRequestDtoValidator : AbstractValidator<UpdateRepairJobStatusRequestDto>
{
    public UpdateRepairJobStatusRequestDtoValidator()
    {
        RuleFor(x => x.StatusId)
            .GreaterThan(0);

        RuleFor(x => x.TechnicianNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TechnicianNotes));

        RuleFor(x => x.ActualCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ActualCost.HasValue);

        RuleFor(x => x.MaterialsUsed)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.MaterialsUsed));

        RuleFor(x => x.HoursSpent)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HoursSpent.HasValue);

        RuleFor(x => x.AdditionalPaymentAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AdditionalPaymentAmount.HasValue);

        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0)
            .When(x => x.PaymentMethodId.HasValue);
    }
}

public class AssignTechnicianRequestDtoValidator : AbstractValidator<AssignTechnicianRequestDto>
{
    public AssignTechnicianRequestDtoValidator()
    {
        RuleFor(x => x.TechnicianId)
            .GreaterThan(0);

        RuleFor(x => x.TechnicianNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TechnicianNotes));
    }
}

public class CompleteRepairRequestDtoValidator : AbstractValidator<CompleteRepairRequestDto>
{
    public CompleteRepairRequestDtoValidator()
    {
        RuleFor(x => x.ActualCost)
            .GreaterThan(0);

        RuleFor(x => x.TechnicianNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TechnicianNotes));

        RuleFor(x => x.MaterialsUsed)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.MaterialsUsed));

        RuleFor(x => x.HoursSpent)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HoursSpent.HasValue);

        RuleFor(x => x.AdditionalPaymentAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AdditionalPaymentAmount.HasValue);

        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0)
            .When(x => x.PaymentMethodId.HasValue);
    }
}

public class QualityCheckRequestDtoValidator : AbstractValidator<QualityCheckRequestDto>
{
    public QualityCheckRequestDtoValidator()
    {
        RuleFor(x => x.QualityCheckNotes)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Passed)
            .NotNull();
    }
}

public class DeliverRepairRequestDtoValidator : AbstractValidator<DeliverRepairRequestDto>
{
    public DeliverRepairRequestDtoValidator()
    {
        RuleFor(x => x.DeliveryNotes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.DeliveryNotes));

        RuleFor(x => x.CustomerNotified)
            .NotNull();

        RuleFor(x => x.AdditionalPaymentAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AdditionalPaymentAmount.HasValue);

        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0)
            .When(x => x.PaymentMethodId.HasValue);
    }
}

public class RepairJobSearchRequestDtoValidator : AbstractValidator<RepairJobSearchRequestDto>
{
    public RepairJobSearchRequestDtoValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.StatusId)
            .GreaterThan(0)
            .When(x => x.StatusId.HasValue);

        RuleFor(x => x.PriorityId)
            .InclusiveBetween(1, 5)
            .When(x => x.PriorityId.HasValue);

        RuleFor(x => x.AssignedTechnicianId)
            .GreaterThan(0)
            .When(x => x.AssignedTechnicianId.HasValue);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .When(x => x.CustomerId.HasValue);

        RuleFor(x => x.TransactionNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.TransactionNumber));

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
