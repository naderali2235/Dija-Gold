using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class BaseLookupDtoValidator<T> : AbstractValidator<T> where T : BaseLookupDto
{
    public BaseLookupDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}

public class KaratTypeLookupDtoValidator : BaseLookupDtoValidator<KaratTypeLookupDto>
{
    public KaratTypeLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class FinancialTransactionTypeLookupDtoValidator : BaseLookupDtoValidator<FinancialTransactionTypeLookupDto>
{
    public FinancialTransactionTypeLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class PaymentMethodLookupDtoValidator : BaseLookupDtoValidator<PaymentMethodLookupDto>
{
    public PaymentMethodLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class FinancialTransactionStatusLookupDtoValidator : BaseLookupDtoValidator<FinancialTransactionStatusLookupDto>
{
    public FinancialTransactionStatusLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class ChargeTypeLookupDtoValidator : BaseLookupDtoValidator<ChargeTypeLookupDto>
{
    public ChargeTypeLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class ProductCategoryTypeLookupDtoValidator : BaseLookupDtoValidator<ProductCategoryTypeLookupDto>
{
    public ProductCategoryTypeLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class RepairStatusLookupDtoValidator : BaseLookupDtoValidator<RepairStatusLookupDto>
{
    public RepairStatusLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class RepairPriorityLookupDtoValidator : BaseLookupDtoValidator<RepairPriorityLookupDto>
{
    public RepairPriorityLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class OrderTypeLookupDtoValidator : BaseLookupDtoValidator<OrderTypeLookupDto>
{
    public OrderTypeLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class OrderStatusLookupDtoValidator : BaseLookupDtoValidator<OrderStatusLookupDto>
{
    public OrderStatusLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class BusinessEntityTypeLookupDtoValidator : BaseLookupDtoValidator<BusinessEntityTypeLookupDto>
{
    public BusinessEntityTypeLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}

public class SubCategoryLookupDtoValidator : BaseLookupDtoValidator<SubCategoryLookupDto>
{
    public SubCategoryLookupDtoValidator()
    {
        // Base validation rules are inherited from BaseLookupDtoValidator
    }
}
