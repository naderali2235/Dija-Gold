using FluentValidation;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;

namespace DijaGoldPOS.API.Validators.Core;

/// <summary>
/// Validator for creating a new branch
/// </summary>
public class CreateBranchValidator : AbstractValidator<CreateBranchRequestDto>
{
    private readonly IBranchRepository _branchRepository;

    public CreateBranchValidator(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
        
        ConfigureValidationRules();
    }

    private void ConfigureValidationRules()
    {
        // Code validation
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Branch code is required.")
            .Length(2, 20)
            .WithMessage("Branch code must be between 2 and 20 characters.")
            .Matches("^[A-Z0-9_-]+$")
            .WithMessage("Branch code can only contain uppercase letters, numbers, hyphens, and underscores.")
            .MustAsync(BeUniqueCode)
            .WithMessage("Branch code already exists. Please choose a different code.");

        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Branch name is required.")
            .Length(2, 100)
            .WithMessage("Branch name must be between 2 and 100 characters.")
            .Matches("^[a-zA-Z0-9\\s\\-_\\.\\(\\)]+$")
            .WithMessage("Branch name contains invalid characters.");

        // Address validation (optional)
        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        // Phone validation (optional)
        RuleFor(x => x.Phone)
            .Matches("^[\\+]?[0-9\\s\\-\\(\\)]+$")
            .WithMessage("Phone number format is invalid.")
            .Length(7, 20)
            .WithMessage("Phone number must be between 7 and 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        // Manager name validation (optional)
        RuleFor(x => x.ManagerName)
            .MaximumLength(100)
            .WithMessage("Manager name cannot exceed 100 characters.")
            .Matches("^[a-zA-Z\\s\\.\\-']+$")
            .WithMessage("Manager name contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.ManagerName));






    }

    private async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            return true;

        var existingBranch = await _branchRepository.GetByCodeAsync(code);
        return existingBranch == null;
    }

    private bool BeValidJsonFormat(string json)
    {
        if (string.IsNullOrEmpty(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

}

/// <summary>
/// Validator for updating an existing branch
/// </summary>
public class UpdateBranchValidator : AbstractValidator<UpdateBranchRequestDto>
{
    private readonly IBranchRepository _branchRepository;

    public UpdateBranchValidator(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
        
        ConfigureValidationRules();
    }

    private void ConfigureValidationRules()
    {
        // ID validation
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Valid branch ID is required.")
            .MustAsync(BranchExists)
            .WithMessage("Branch not found.");

        // Code validation (if provided)
        RuleFor(x => x.Code)
            .Length(2, 20)
            .WithMessage("Branch code must be between 2 and 20 characters.")
            .Matches("^[A-Z0-9_-]+$")
            .WithMessage("Branch code can only contain uppercase letters, numbers, hyphens, and underscores.")
            .MustAsync(BeUniqueCodeForUpdate)
            .WithMessage("Branch code already exists. Please choose a different code.")
            .When(x => !string.IsNullOrEmpty(x.Code));

        // Name validation (if provided)
        RuleFor(x => x.Name)
            .Length(2, 100)
            .WithMessage("Branch name must be between 2 and 100 characters.")
            .Matches("^[a-zA-Z0-9\\s\\-_\\.\\(\\)]+$")
            .WithMessage("Branch name contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        // Address validation (optional)
        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        // Phone validation (optional)
        RuleFor(x => x.Phone)
            .Matches("^[\\+]?[0-9\\s\\-\\(\\)]+$")
            .WithMessage("Phone number format is invalid.")
            .Length(7, 20)
            .WithMessage("Phone number must be between 7 and 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        // Manager name validation (optional)
        RuleFor(x => x.ManagerName)
            .MaximumLength(100)
            .WithMessage("Manager name cannot exceed 100 characters.")
            .Matches("^[a-zA-Z\\s\\.\\-']+$")
            .WithMessage("Manager name contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.ManagerName));


    }

    private async Task<bool> BranchExists(int id, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(id);
        return branch != null && branch.IsActive;
    }

    private async Task<bool> BeUniqueCodeForUpdate(UpdateBranchRequestDto dto, string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            return true;

        var existingBranch = await _branchRepository.GetByCodeAsync(code);
        return existingBranch == null || existingBranch.Id == dto.Id;
    }

    private bool BeValidJsonFormat(string json)
    {
        if (string.IsNullOrEmpty(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }


}
