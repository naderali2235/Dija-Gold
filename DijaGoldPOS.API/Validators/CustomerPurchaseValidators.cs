using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CustomerPurchaseDtoValidator : AbstractValidator<CustomerPurchaseDto>
{
    public CustomerPurchaseDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.PurchaseNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BranchId)
            .GreaterThan(0);

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PurchaseDate)
            .NotEmpty();

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0);

        RuleFor(x => x.PaymentMethodName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty();

        RuleFor(x => x.CreatedAt)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotNull();

        RuleForEach(x => x.Items)
            .SetValidator(new CustomerPurchaseItemDtoValidator());
    }
}

public class CustomerPurchaseItemDtoValidator : AbstractValidator<CustomerPurchaseItemDto>
{
    public CustomerPurchaseItemDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.CustomerPurchaseId)
            .GreaterThan(0);

        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Weight)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class CreateCustomerPurchaseRequestValidator : AbstractValidator<CreateCustomerPurchaseRequest>
{
    public CreateCustomerPurchaseRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.BranchId)
            .GreaterThan(0);

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateCustomerPurchaseItemRequestValidator());

        // Business rule validation
        RuleFor(x => x)
            .Must(x => x.AmountPaid <= x.TotalAmount)
            .WithMessage("Amount paid cannot exceed total amount");
    }
}

public class CreateCustomerPurchaseItemRequestValidator : AbstractValidator<CreateCustomerPurchaseItemRequest>
{
    public CreateCustomerPurchaseItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Weight)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        // Business rule validation
        RuleFor(x => x)
            .Must(x => x.TotalAmount == x.Quantity * x.UnitPrice)
            .WithMessage("Total amount must equal quantity times unit price");
    }
}

public class CustomerPurchaseSearchRequestValidator : AbstractValidator<CustomerPurchaseSearchRequest>
{
    public CustomerPurchaseSearchRequestValidator()
    {
        RuleFor(x => x.PurchaseNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.PurchaseNumber));

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .When(x => x.CustomerId.HasValue);

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
