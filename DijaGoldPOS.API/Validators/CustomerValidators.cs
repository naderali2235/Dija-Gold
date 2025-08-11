using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateCustomerRequestDtoValidator : AbstractValidator<CreateCustomerRequestDto>
{
    public CreateCustomerRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.NationalId)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

        RuleFor(x => x.MobileNumber)
            .MaximumLength(15)
            .Matches(@"^[0-9+\-() ]*$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.LoyaltyTier)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.DefaultDiscountPercentage)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class UpdateCustomerRequestDtoValidator : AbstractValidator<UpdateCustomerRequestDto>
{
    public UpdateCustomerRequestDtoValidator()
    {
        Include(new CreateCustomerRequestDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0);
    }
}


