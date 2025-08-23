using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.CustomDiscountPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.CustomDiscountPercentage.HasValue);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.OrderTypeId).GreaterThan(0);
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId.HasValue);
        RuleFor(x => x.GoldRateId).GreaterThan(0).When(x => x.GoldRateId.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
        RuleFor(x => x.EstimatedCompletionDate).GreaterThan(DateTime.UtcNow).When(x => x.EstimatedCompletionDate.HasValue);
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(child => child.SetValidator(new CreateOrderItemRequestValidator()));
    }
}

public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.StatusId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
        RuleFor(x => x.EstimatedCompletionDate).GreaterThan(DateTime.UtcNow).When(x => x.EstimatedCompletionDate.HasValue);
        RuleFor(x => x.ReturnReason).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ReturnReason));
    }
}

public class OrderSearchRequestValidator : AbstractValidator<OrderSearchRequest>
{
    public OrderSearchRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue);
        RuleFor(x => x.OrderTypeId).GreaterThan(0).When(x => x.OrderTypeId.HasValue);
        RuleFor(x => x.StatusId).GreaterThan(0).When(x => x.StatusId.HasValue);
        RuleFor(x => x.FromDate).LessThanOrEqualTo(x => x.ToDate).When(x => x.FromDate.HasValue && x.ToDate.HasValue);
        RuleFor(x => x.OrderNumber).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.OrderNumber));
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId.HasValue);
        RuleFor(x => x.CashierId).MaximumLength(450).When(x => !string.IsNullOrEmpty(x.CashierId));
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class ProcessOrderPaymentRequestValidator : AbstractValidator<ProcessOrderPaymentRequest>
{
    public ProcessOrderPaymentRequestValidator()
    {
        RuleFor(x => x.AmountPaid).GreaterThan(0);
        RuleFor(x => x.PaymentMethodId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class ReturnOrderItemRequestValidator : AbstractValidator<ReturnOrderItemRequest>
{
    public ReturnOrderItemRequestValidator()
    {
        RuleFor(x => x.OriginalOrderItemId).GreaterThan(0);
        RuleFor(x => x.QuantityToReturn).GreaterThan(0);
        RuleFor(x => x.ReturnReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ReturnReason));
    }
}

public class CreateReturnOrderRequestValidator : AbstractValidator<CreateReturnOrderRequest>
{
    public CreateReturnOrderRequestValidator()
    {
        RuleFor(x => x.ReturnReason)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(child => child.SetValidator(new ReturnOrderItemRequestValidator()));
    }
}

public class ExchangeOrderItemRequestValidator : AbstractValidator<ExchangeOrderItemRequest>
{
    public ExchangeOrderItemRequestValidator()
    {
        RuleFor(x => x.OriginalOrderItemId).GreaterThan(0);
        RuleFor(x => x.QuantityToExchange).GreaterThan(0);
        RuleFor(x => x.NewProductId).GreaterThan(0);
        RuleFor(x => x.NewQuantity).GreaterThan(0);
        RuleFor(x => x.ExchangeReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ExchangeReason));
    }
}

public class CreateExchangeOrderRequestValidator : AbstractValidator<CreateExchangeOrderRequest>
{
    public CreateExchangeOrderRequestValidator()
    {
        RuleFor(x => x.ExchangeReason)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
        RuleFor(x => x.EstimatedCompletionDate).GreaterThan(DateTime.UtcNow).When(x => x.EstimatedCompletionDate.HasValue);
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(child => child.SetValidator(new ExchangeOrderItemRequestValidator()));
    }
}

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.ManagerId)
            .NotEmpty()
            .MaximumLength(450);
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.StatusId).GreaterThan(0);
    }
}
