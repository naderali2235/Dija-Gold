using DijaGoldPOS.API.DTOs;
using FluentValidation;

namespace DijaGoldPOS.API.Validators;

public class GenerateBrowserReceiptRequestDtoValidator : AbstractValidator<GenerateBrowserReceiptRequestDto>
{
    public GenerateBrowserReceiptRequestDtoValidator()
    {
        RuleFor(x => x.TransactionId)
            .GreaterThan(0);

        RuleFor(x => x.IncludeHtml)
            .NotNull();

        RuleFor(x => x.IncludeCss)
            .NotNull();
    }
}

public class ReceiptTemplateValidator : AbstractValidator<ReceiptTemplate>
{
    public ReceiptTemplateValidator()
    {
        RuleFor(x => x.TemplateName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.HeaderLines)
            .NotNull();

        RuleFor(x => x.FooterLines)
            .NotNull();

        RuleForEach(x => x.HeaderLines)
            .MaximumLength(200);

        RuleForEach(x => x.FooterLines)
            .MaximumLength(200);

        RuleFor(x => x.Settings)
            .NotNull()
            .SetValidator(new ReceiptSettingsValidator());
    }
}

public class ReceiptSettingsValidator : AbstractValidator<ReceiptSettings>
{
    public ReceiptSettingsValidator()
    {
        RuleFor(x => x.PaperWidth)
            .InclusiveBetween(40, 120);

        RuleFor(x => x.FontName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.FontSize)
            .InclusiveBetween(8, 16);

        RuleFor(x => x.DateTimeFormat)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CurrencyFormat)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.LogoPath)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoPath));
    }
}

public class ReceiptLineItemValidator : AbstractValidator<ReceiptLineItem>
{
    public ReceiptLineItemValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Total)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AdditionalInfo)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.AdditionalInfo));
    }
}

public class ReceiptDataValidator : AbstractValidator<ReceiptData>
{
    public ReceiptDataValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CompanyAddress)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.CompanyPhone)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.CompanyTaxNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CompanyTaxNumber));

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BranchAddress)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.BranchAddress));

        RuleFor(x => x.BranchPhone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.BranchPhone));

        RuleFor(x => x.TransactionNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TransactionTypeId)
            .GreaterThan(0);

        RuleFor(x => x.TransactionTypeName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CashierName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerName));

        RuleFor(x => x.CustomerPhone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerPhone));

        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));

        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ReceiptLineItemValidator());

        RuleFor(x => x.Subtotal)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MakingCharges)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TaxableAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ChangeGiven)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Taxes)
            .NotNull();

        RuleForEach(x => x.Taxes)
            .SetValidator(new TaxLineItemValidator());

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.SpecialInstructions));

        RuleFor(x => x.ReturnPolicy)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.ReturnPolicy));

        RuleFor(x => x.WarrantyInfo)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.WarrantyInfo));

        RuleFor(x => x.OriginalTransactionNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.OriginalTransactionNumber));

        RuleFor(x => x.ReturnReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.ReturnReason));

        RuleFor(x => x.RepairDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.RepairDescription));
    }
}

public class TaxLineItemValidator : AbstractValidator<TaxLineItem>
{
    public TaxLineItemValidator()
    {
        RuleFor(x => x.TaxName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 1); // 0% to 100%

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0);
    }
}
