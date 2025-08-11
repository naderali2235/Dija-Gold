using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreatePurchaseOrderItemRequestDtoValidator : AbstractValidator<CreatePurchaseOrderItemRequestDto>
{
    public CreatePurchaseOrderItemRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.QuantityOrdered).GreaterThan(0);
        RuleFor(x => x.WeightOrdered).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class CreatePurchaseOrderRequestDtoValidator : AbstractValidator<CreatePurchaseOrderRequestDto>
{
    public CreatePurchaseOrderRequestDtoValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.Terms)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Terms));
        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(child => child.SetValidator(new CreatePurchaseOrderItemRequestDtoValidator()));
    }
}

public class ReceivePurchaseOrderItemDtoValidator : AbstractValidator<ReceivePurchaseOrderItemDto>
{
    public ReceivePurchaseOrderItemDtoValidator()
    {
        RuleFor(x => x.PurchaseOrderItemId).GreaterThan(0);
        RuleFor(x => x.QuantityReceived).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WeightReceived).GreaterThanOrEqualTo(0);
    }
}

public class ReceivePurchaseOrderRequestDtoValidator : AbstractValidator<ReceivePurchaseOrderRequestDto>
{
    public ReceivePurchaseOrderRequestDtoValidator()
    {
        RuleFor(x => x.PurchaseOrderId).GreaterThan(0);
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(child => child.SetValidator(new ReceivePurchaseOrderItemDtoValidator()));
    }
}


