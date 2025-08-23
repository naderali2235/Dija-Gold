using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Shared;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateProductRequestDtoValidator : AbstractValidator<CreateProductRequestDto>
{
    public CreateProductRequestDtoValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Weight)
            .InclusiveBetween(0.001m, 999999.999m).WithMessage("Weight must be between 0.001 and 999999.999 grams");

        RuleFor(x => x.Brand)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Brand));

        RuleFor(x => x.DesignStyle)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DesignStyle));

        RuleFor(x => x.SubCategory)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SubCategory));

        RuleFor(x => x.Shape)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Shape));

        RuleFor(x => x.PurityCertificateNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.PurityCertificateNumber));

        RuleFor(x => x.CountryOfOrigin)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CountryOfOrigin));

        RuleFor(x => x.YearOfMinting)
            .InclusiveBetween(1900, 2100)
            .When(x => x.YearOfMinting.HasValue);

        RuleFor(x => x.FaceValue)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FaceValue.HasValue);

        // Product-level making charges validation
        RuleFor(x => x.UseProductMakingCharges)
            .Must((product, useProductCharges) => 
            {
                // If using product making charges, both type and value must be provided
                if (useProductCharges)
                {
                    return product.ProductMakingChargesTypeId.HasValue && 
                           product.ProductMakingChargesValue.HasValue;
                }
                return true;
            })
            .WithMessage("When using product making charges, both charge type and value must be provided");

        RuleFor(x => x.ProductMakingChargesTypeId)
            .Must((product, chargeTypeId) => 
            {
                if (!chargeTypeId.HasValue) return false;
                return chargeTypeId.Value == LookupTableConstants.ChargeTypePercentage || 
                       chargeTypeId.Value == LookupTableConstants.ChargeTypeFixedAmount;
            })
            .When(x => x.UseProductMakingCharges)
            .WithMessage("Product making charges type must be 1 (Percentage) or 2 (Fixed)");

        RuleFor(x => x.ProductMakingChargesValue)
            .GreaterThan(0)
            .When(x => x.UseProductMakingCharges)
            .WithMessage("Product making charges value must be greater than 0");

        RuleFor(x => x.ProductMakingChargesValue)
            .LessThanOrEqualTo(100)
            .When(x => x.UseProductMakingCharges && x.ProductMakingChargesTypeId == LookupTableConstants.ChargeTypePercentage)
            .WithMessage("Percentage making charges cannot exceed 100%");
    }
}

public class UpdateProductRequestDtoValidator : AbstractValidator<UpdateProductRequestDto>
{
    public UpdateProductRequestDtoValidator()
    {
        Include(new CreateProductRequestDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0);
    }
}


