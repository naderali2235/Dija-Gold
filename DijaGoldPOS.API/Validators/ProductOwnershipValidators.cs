using FluentValidation;
using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.Validators;

/// <summary>
/// Validator for ProductOwnershipRequest
/// </summary>
public class ProductOwnershipRequestValidator : AbstractValidator<ProductOwnershipRequest>
{
    public ProductOwnershipRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.TotalQuantity)
            .GreaterThan(0)
            .WithMessage("Total quantity must be greater than 0");

        RuleFor(x => x.TotalWeight)
            .GreaterThan(0)
            .WithMessage("Total weight must be greater than 0");

        RuleFor(x => x.OwnedQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Owned quantity must be greater than or equal to 0");

        RuleFor(x => x.OwnedWeight)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Owned weight must be greater than or equal to 0");

        RuleFor(x => x.TotalCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total cost must be greater than or equal to 0");

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount paid must be greater than or equal to 0");

        RuleFor(x => x.OwnedQuantity)
            .LessThanOrEqualTo(x => x.TotalQuantity)
            .WithMessage("Owned quantity cannot exceed total quantity");

        RuleFor(x => x.OwnedWeight)
            .LessThanOrEqualTo(x => x.TotalWeight)
            .WithMessage("Owned weight cannot exceed total weight");

        RuleFor(x => x.AmountPaid)
            .LessThanOrEqualTo(x => x.TotalCost)
            .WithMessage("Amount paid cannot exceed total cost");
    }
}

/// <summary>
/// Validator for ConvertRawGoldRequest
/// </summary>
public class ConvertRawGoldRequestValidator : AbstractValidator<ConvertRawGoldRequest>
{
    public ConvertRawGoldRequestValidator()
    {
        RuleFor(x => x.RawGoldProductId)
            .GreaterThan(0)
            .WithMessage("Raw gold product ID must be greater than 0");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.WeightToConvert)
            .GreaterThan(0)
            .WithMessage("Weight to convert must be greater than 0");

        RuleFor(x => x.QuantityToConvert)
            .GreaterThan(0)
            .WithMessage("Quantity to convert must be greater than 0");

        RuleFor(x => x.NewProducts)
            .NotEmpty()
            .WithMessage("At least one new product must be specified");

        RuleForEach(x => x.NewProducts)
            .SetValidator(new NewProductFromRawGoldValidator());
    }
}

/// <summary>
/// Validator for NewProductFromRawGold
/// </summary>
public class NewProductFromRawGoldValidator : AbstractValidator<NewProductFromRawGold>
{
    public NewProductFromRawGoldValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than 0");
    }
}

/// <summary>
/// Validator for CreateOwnershipMovementRequest
/// </summary>
public class CreateOwnershipMovementRequestValidator : AbstractValidator<CreateOwnershipMovementRequest>
{
    public CreateOwnershipMovementRequestValidator()
    {
        RuleFor(x => x.ProductOwnershipId)
            .GreaterThan(0)
            .WithMessage("Product ownership ID must be greater than 0");

        RuleFor(x => x.MovementType)
            .NotEmpty()
            .WithMessage("Movement type is required")
            .MaximumLength(50)
            .WithMessage("Movement type cannot exceed 50 characters");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100)
            .WithMessage("Reference number cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for ValidateOwnershipRequest
/// </summary>
public class ValidateOwnershipRequestValidator : AbstractValidator<ValidateOwnershipRequest>
{
    public ValidateOwnershipRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.RequestedQuantity)
            .GreaterThan(0)
            .WithMessage("Requested quantity must be greater than 0");
    }
}

/// <summary>
/// Validator for UpdateOwnershipPaymentRequest
/// </summary>
public class UpdateOwnershipPaymentRequestValidator : AbstractValidator<UpdateOwnershipPaymentRequest>
{
    public UpdateOwnershipPaymentRequestValidator()
    {
        RuleFor(x => x.ProductOwnershipId)
            .GreaterThan(0)
            .WithMessage("Product ownership ID must be greater than 0");

        RuleFor(x => x.PaymentAmount)
            .GreaterThan(0)
            .WithMessage("Payment amount must be greater than 0");

        RuleFor(x => x.ReferenceNumber)
            .NotEmpty()
            .WithMessage("Reference number is required")
            .MaximumLength(100)
            .WithMessage("Reference number cannot exceed 100 characters");
    }
}

/// <summary>
/// Validator for UpdateOwnershipSaleRequest
/// </summary>
public class UpdateOwnershipSaleRequestValidator : AbstractValidator<UpdateOwnershipSaleRequest>
{
    public UpdateOwnershipSaleRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.SoldQuantity)
            .GreaterThan(0)
            .WithMessage("Sold quantity must be greater than 0");

        RuleFor(x => x.ReferenceNumber)
            .NotEmpty()
            .WithMessage("Reference number is required")
            .MaximumLength(100)
            .WithMessage("Reference number cannot exceed 100 characters");
    }
}
