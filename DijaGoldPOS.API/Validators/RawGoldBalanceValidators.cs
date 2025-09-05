using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

/// <summary>
/// Validator for WaiveGoldToSupplierRequest
/// </summary>
public class WaiveGoldToSupplierRequestValidator : AbstractValidator<WaiveGoldToSupplierRequest>
{
    public WaiveGoldToSupplierRequestValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.ToSupplierId)
            .GreaterThan(0)
            .WithMessage("Supplier ID must be greater than 0");

        RuleFor(x => x.FromKaratTypeId)
            .GreaterThan(0)
            .WithMessage("From Karat Type ID must be greater than 0");

        RuleFor(x => x.ToKaratTypeId)
            .GreaterThan(0)
            .WithMessage("To Karat Type ID must be greater than 0");

        RuleFor(x => x.FromWeight)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Weight cannot exceed 10,000 grams")
            .ScalePrecision(3, 10)
            .WithMessage("Weight can have maximum 3 decimal places");

        RuleFor(x => x.CustomerPurchaseId)
            .GreaterThan(0)
            .When(x => x.CustomerPurchaseId.HasValue)
            .WithMessage("Customer Purchase ID must be greater than 0 when provided");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters");

        // Business rule: Cannot waive to same karat type from merchant gold (no conversion needed)
        RuleFor(x => x)
            .Must(x => x.FromKaratTypeId != x.ToKaratTypeId)
            .WithMessage("Karat conversion is required when waiving gold to supplier. From and To karat types cannot be the same")
            .When(x => x.FromKaratTypeId > 0 && x.ToKaratTypeId > 0);
    }
}

/// <summary>
/// Validator for ConvertGoldKaratRequest
/// </summary>
public class ConvertGoldKaratRequestValidator : AbstractValidator<ConvertGoldKaratRequest>
{
    public ConvertGoldKaratRequestValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .When(x => x.SupplierId.HasValue)
            .WithMessage("Supplier ID must be greater than 0 when provided");

        RuleFor(x => x.FromKaratTypeId)
            .GreaterThan(0)
            .WithMessage("From Karat Type ID must be greater than 0");

        RuleFor(x => x.ToKaratTypeId)
            .GreaterThan(0)
            .WithMessage("To Karat Type ID must be greater than 0");

        RuleFor(x => x.FromWeight)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Weight cannot exceed 10,000 grams")
            .ScalePrecision(3, 10)
            .WithMessage("Weight can have maximum 3 decimal places");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters");

        // Business rule: Must convert between different karat types
        RuleFor(x => x)
            .Must(x => x.FromKaratTypeId != x.ToKaratTypeId)
            .WithMessage("From and To karat types must be different for conversion")
            .When(x => x.FromKaratTypeId > 0 && x.ToKaratTypeId > 0);
    }
}

/// <summary>
/// Validator for GoldTransferSearchRequest
/// </summary>
public class GoldTransferSearchRequestValidator : AbstractValidator<GoldTransferSearchRequest>
{
    public GoldTransferSearchRequestValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue)
            .WithMessage("Branch ID must be greater than 0 when provided");

        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .When(x => x.SupplierId.HasValue)
            .WithMessage("Supplier ID must be greater than 0 when provided");

        RuleFor(x => x.KaratTypeId)
            .GreaterThan(0)
            .When(x => x.KaratTypeId.HasValue)
            .WithMessage("Karat Type ID must be greater than 0 when provided");

        RuleFor(x => x.TransferType)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "Waive", "Credit", "Convert" }.Contains(x))
            .WithMessage("Transfer type must be one of: Waive, Credit, Convert");

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("From date must be less than or equal to To date");

        RuleFor(x => x.ToDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.ToDate.HasValue)
            .WithMessage("To date cannot be in the future");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        // Business rule: Date range should not exceed 1 year for performance
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || 
                      (x.ToDate.Value - x.FromDate.Value).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

/// <summary>
/// Validator for SupplierGoldBalanceDto (for responses)
/// </summary>
public class SupplierGoldBalanceDtoValidator : AbstractValidator<SupplierGoldBalanceDto>
{
    public SupplierGoldBalanceDtoValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .WithMessage("Supplier ID must be greater than 0");

        RuleFor(x => x.SupplierName)
            .NotEmpty()
            .WithMessage("Supplier name is required")
            .MaximumLength(200)
            .WithMessage("Supplier name cannot exceed 200 characters");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(100)
            .WithMessage("Branch name cannot exceed 100 characters");

        RuleFor(x => x.KaratTypeId)
            .GreaterThan(0)
            .WithMessage("Karat Type ID must be greater than 0");

        RuleFor(x => x.KaratTypeName)
            .NotEmpty()
            .WithMessage("Karat type name is required")
            .MaximumLength(50)
            .WithMessage("Karat type name cannot exceed 50 characters");

        RuleFor(x => x.TotalWeightReceived)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total weight received must be greater than or equal to 0")
            .ScalePrecision(3, 10)
            .WithMessage("Total weight received can have maximum 3 decimal places");

        RuleFor(x => x.TotalWeightPaidFor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total weight paid for must be greater than or equal to 0")
            .ScalePrecision(3, 10)
            .WithMessage("Total weight paid for can have maximum 3 decimal places");

        RuleFor(x => x.OutstandingMonetaryValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Outstanding monetary value must be greater than or equal to 0")
            .ScalePrecision(2, 18)
            .WithMessage("Outstanding monetary value can have maximum 2 decimal places");

        RuleFor(x => x.AverageCostPerGram)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Average cost per gram must be greater than or equal to 0")
            .ScalePrecision(2, 18)
            .WithMessage("Average cost per gram can have maximum 2 decimal places");

        // Business rule: Total weight paid for cannot exceed total weight received
        RuleFor(x => x.TotalWeightPaidFor)
            .LessThanOrEqualTo(x => x.TotalWeightReceived)
            .WithMessage("Total weight paid for cannot exceed total weight received");
    }
}

/// <summary>
/// Validator for MerchantRawGoldBalanceDto (for responses)
/// </summary>
public class MerchantRawGoldBalanceDtoValidator : AbstractValidator<MerchantRawGoldBalanceDto>
{
    public MerchantRawGoldBalanceDtoValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(100)
            .WithMessage("Branch name cannot exceed 100 characters");

        RuleFor(x => x.KaratTypeId)
            .GreaterThan(0)
            .WithMessage("Karat Type ID must be greater than 0");

        RuleFor(x => x.KaratTypeName)
            .NotEmpty()
            .WithMessage("Karat type name is required")
            .MaximumLength(50)
            .WithMessage("Karat type name cannot exceed 50 characters");

        RuleFor(x => x.AvailableWeight)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Available weight must be greater than or equal to 0")
            .ScalePrecision(3, 10)
            .WithMessage("Available weight can have maximum 3 decimal places");

        RuleFor(x => x.AverageCostPerGram)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Average cost per gram must be greater than or equal to 0")
            .ScalePrecision(2, 18)
            .WithMessage("Average cost per gram can have maximum 2 decimal places");

        RuleFor(x => x.TotalValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total value must be greater than or equal to 0")
            .ScalePrecision(2, 18)
            .WithMessage("Total value can have maximum 2 decimal places");
    }
}

/// <summary>
/// Validator for RawGoldTransferDto (for responses)
/// </summary>
public class RawGoldTransferDtoValidator : AbstractValidator<RawGoldTransferDto>
{
    public RawGoldTransferDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Transfer ID must be greater than 0");

        RuleFor(x => x.TransferNumber)
            .NotEmpty()
            .WithMessage("Transfer number is required")
            .MaximumLength(50)
            .WithMessage("Transfer number cannot exceed 50 characters");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("Branch ID must be greater than 0");

        RuleFor(x => x.FromKaratTypeId)
            .GreaterThan(0)
            .WithMessage("From Karat Type ID must be greater than 0");

        RuleFor(x => x.ToKaratTypeId)
            .GreaterThan(0)
            .WithMessage("To Karat Type ID must be greater than 0");

        RuleFor(x => x.FromWeight)
            .GreaterThan(0)
            .WithMessage("From weight must be greater than 0")
            .ScalePrecision(3, 10)
            .WithMessage("From weight can have maximum 3 decimal places");

        RuleFor(x => x.ToWeight)
            .GreaterThan(0)
            .WithMessage("To weight must be greater than 0")
            .ScalePrecision(3, 10)
            .WithMessage("To weight can have maximum 3 decimal places");

        RuleFor(x => x.TransferType)
            .NotEmpty()
            .WithMessage("Transfer type is required")
            .Must(x => new[] { "Waive", "Credit", "Convert" }.Contains(x))
            .WithMessage("Transfer type must be one of: Waive, Credit, Convert");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty()
            .WithMessage("Created by user ID is required")
            .MaximumLength(450)
            .WithMessage("Created by user ID cannot exceed 450 characters");
    }
}
