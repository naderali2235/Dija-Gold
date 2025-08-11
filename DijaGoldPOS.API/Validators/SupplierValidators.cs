using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateSupplierRequestDtoValidator : AbstractValidator<CreateSupplierRequestDto>
{
    public CreateSupplierRequestDtoValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ContactPersonName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPersonName));

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^[0-9+\-() ]*$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.TaxRegistrationNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.TaxRegistrationNumber));

        RuleFor(x => x.CommercialRegistrationNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CommercialRegistrationNumber));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaymentTermsDays)
            .InclusiveBetween(0, 365);

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentTerms));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class UpdateSupplierRequestDtoValidator : AbstractValidator<UpdateSupplierRequestDto>
{
    public UpdateSupplierRequestDtoValidator()
    {
        Include(new CreateSupplierRequestDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0);
    }
}


