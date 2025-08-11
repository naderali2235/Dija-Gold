using DijaGoldPOS.API.DTOs;
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
            .GreaterThan(0).WithMessage("Weight must be greater than 0");

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


