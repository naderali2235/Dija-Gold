using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateFinancialTransactionRequestValidator : AbstractValidator<CreateFinancialTransactionRequest>
{
    public CreateFinancialTransactionRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.TransactionTypeId).GreaterThan(0);
        RuleFor(x => x.Subtotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalTaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalDiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleFor(x => x.AmountPaid).GreaterThanOrEqualTo(x => x.TotalAmount);
        RuleFor(x => x.ChangeGiven).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentMethodId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).When(x => x.BusinessEntityTypeId > 0);
        RuleFor(x => x.BusinessEntityTypeId).GreaterThan(0);
    }
}

public class UpdateFinancialTransactionRequestValidator : AbstractValidator<UpdateFinancialTransactionRequest>
{
    public UpdateFinancialTransactionRequestValidator()
    {
        RuleFor(x => x.Subtotal).GreaterThanOrEqualTo(0).When(x => x.Subtotal.HasValue);
        RuleFor(x => x.TotalTaxAmount).GreaterThanOrEqualTo(0).When(x => x.TotalTaxAmount.HasValue);
        RuleFor(x => x.TotalDiscountAmount).GreaterThanOrEqualTo(0).When(x => x.TotalDiscountAmount.HasValue);
        RuleFor(x => x.TotalAmount).GreaterThan(0).When(x => x.TotalAmount.HasValue);
        RuleFor(x => x.AmountPaid).GreaterThanOrEqualTo(0).When(x => x.AmountPaid.HasValue);
        RuleFor(x => x.ChangeGiven).GreaterThanOrEqualTo(0).When(x => x.ChangeGiven.HasValue);
        RuleFor(x => x.PaymentMethodId).GreaterThan(0).When(x => x.PaymentMethodId.HasValue);
        RuleFor(x => x.StatusId).GreaterThan(0).When(x => x.StatusId.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class FinancialTransactionSearchRequestValidator : AbstractValidator<FinancialTransactionSearchRequest>
{
    public FinancialTransactionSearchRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue);
        RuleFor(x => x.TransactionTypeId).GreaterThan(0).When(x => x.TransactionTypeId.HasValue);
        RuleFor(x => x.StatusId).GreaterThan(0).When(x => x.StatusId.HasValue);
        RuleFor(x => x.FromDate).LessThanOrEqualTo(x => x.ToDate).When(x => x.FromDate.HasValue && x.ToDate.HasValue);
        RuleFor(x => x.TransactionNumber).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.TransactionNumber));
        RuleFor(x => x.ProcessedByUserId).MaximumLength(450).When(x => !string.IsNullOrEmpty(x.ProcessedByUserId));
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).When(x => x.BusinessEntityId.HasValue);
        RuleFor(x => x.BusinessEntityTypeId).GreaterThan(0).When(x => x.BusinessEntityTypeId.HasValue);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class VoidFinancialTransactionRequestValidator : AbstractValidator<VoidFinancialTransactionRequest>
{
    public VoidFinancialTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}

public class CreateReversalTransactionRequestValidator : AbstractValidator<CreateReversalTransactionRequest>
{
    public CreateReversalTransactionRequestValidator()
    {
        RuleFor(x => x.OriginalTransactionId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.ManagerId)
            .NotEmpty()
            .MaximumLength(450);
    }
}

public class MarkReceiptPrintedRequestValidator : AbstractValidator<MarkReceiptPrintedRequest>
{
    public MarkReceiptPrintedRequestValidator()
    {
        RuleFor(x => x.TransactionId).GreaterThan(0);
    }
}

public class MarkGeneralLedgerPostedRequestValidator : AbstractValidator<MarkGeneralLedgerPostedRequest>
{
    public MarkGeneralLedgerPostedRequestValidator()
    {
        RuleFor(x => x.TransactionId).GreaterThan(0);
    }
}
