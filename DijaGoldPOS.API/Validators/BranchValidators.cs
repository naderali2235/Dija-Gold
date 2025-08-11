using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateBranchRequestDtoValidator : AbstractValidator<CreateBranchRequestDto>
{
    public CreateBranchRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.ManagerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ManagerName));
    }
}

public class UpdateBranchRequestDtoValidator : AbstractValidator<UpdateBranchRequestDto>
{
    public UpdateBranchRequestDtoValidator()
    {
        Include(new CreateBranchRequestDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0);
    }
}


