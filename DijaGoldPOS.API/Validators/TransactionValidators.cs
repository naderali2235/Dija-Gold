using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class SaleItemRequestDtoValidator : AbstractValidator<SaleItemRequestDto>
{
    public SaleItemRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.CustomDiscountPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.CustomDiscountPercentage.HasValue);
    }
}

public class SaleTransactionRequestDtoValidator : AbstractValidator<SaleTransactionRequestDto>
{
    public SaleTransactionRequestDtoValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.AmountPaid).GreaterThan(0);
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(child => child.SetValidator(new SaleItemRequestDtoValidator()));
    }
}

public class ReturnItemRequestDtoValidator : AbstractValidator<ReturnItemRequestDto>
{
    public ReturnItemRequestDtoValidator()
    {
        RuleFor(x => x.OriginalTransactionItemId).GreaterThan(0);
        RuleFor(x => x.ReturnQuantity).GreaterThan(0);
    }
}

public class ReturnTransactionRequestDtoValidator : AbstractValidator<ReturnTransactionRequestDto>
{
    public ReturnTransactionRequestDtoValidator()
    {
        RuleFor(x => x.OriginalTransactionId).GreaterThan(0);
        RuleFor(x => x.ReturnReason)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.ReturnAmount).GreaterThan(0);
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(child => child.SetValidator(new ReturnItemRequestDtoValidator()));
    }
}

public class RepairTransactionRequestDtoValidator : AbstractValidator<RepairTransactionRequestDto>
{
    public RepairTransactionRequestDtoValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.RepairDescription)
            .NotEmpty()
            .MaximumLength(1000);
        RuleFor(x => x.RepairAmount).GreaterThan(0);
        RuleFor(x => x.AmountPaid).GreaterThanOrEqualTo(0);
    }
}

public class CancelTransactionRequestDtoValidator : AbstractValidator<CancelTransactionRequestDto>
{
    public CancelTransactionRequestDtoValidator()
    {
        RuleFor(x => x.TransactionId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}

public class ReprintReceiptRequestDtoValidator : AbstractValidator<ReprintReceiptRequestDto>
{
    public ReprintReceiptRequestDtoValidator()
    {
        RuleFor(x => x.TransactionId).GreaterThan(0);
        RuleFor(x => x.Copies).InclusiveBetween(1, 10);
    }
}


