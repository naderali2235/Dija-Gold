using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateTechnicianRequestDtoValidator : AbstractValidator<CreateTechnicianRequestDto>
{
    public CreateTechnicianRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Specialization)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Specialization));

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);
    }
}

public class UpdateTechnicianRequestDtoValidator : AbstractValidator<UpdateTechnicianRequestDto>
{
    public UpdateTechnicianRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Specialization)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Specialization));

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}

public class TechnicianSearchRequestDtoValidator : AbstractValidator<TechnicianSearchRequestDto>
{
    public TechnicianSearchRequestDtoValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
