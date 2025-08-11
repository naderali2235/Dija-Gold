using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.EmployeeCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeCode));

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);

        RuleFor(x => x.Roles)
            .NotEmpty();
    }
}

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
        RuleFor(x => x.EmployeeCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeCode));
    }
}

public class UpdateUserRoleRequestDtoValidator : AbstractValidator<UpdateUserRoleRequestDto>
{
    public UpdateUserRoleRequestDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public class UpdateUserStatusRequestDtoValidator : AbstractValidator<UpdateUserStatusRequestDto>
{
    public UpdateUserStatusRequestDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);
    }
}

public class UpdateUserPermissionsRequestDtoValidator : AbstractValidator<UpdateUserPermissionsRequestDto>
{
    public UpdateUserPermissionsRequestDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}


