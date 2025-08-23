

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Receipt template model
/// </summary>
public class ReceiptTemplate
{
    public int Id { get; set; }
    public int TransactionTypeId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public List<string> HeaderLines { get; set; } = new();
    public List<string> FooterLines { get; set; } = new();
    public ReceiptSettings Settings { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}

/// <summary>
/// Receipt settings
/// </summary>
public class ReceiptSettings
{
    public int PaperWidth { get; set; } = 80; // mm
    public string FontName { get; set; } = "Arial";
    public int FontSize { get; set; } = 10;
    public bool PrintCompanyLogo { get; set; } = true;
    public bool PrintBorder { get; set; } = false;
    public string DateTimeFormat { get; set; } = "dd/MM/yyyy HH:mm:ss";
    public string CurrencyFormat { get; set; } = "0.00";
    public bool ShowItemDetails { get; set; } = true;
    public bool ShowTaxBreakdown { get; set; } = true;
    public bool ShowBarcode { get; set; } = false;
    public string? LogoPath { get; set; }
}

/// <summary>
/// Receipt line item
/// </summary>
public class ReceiptLineItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public string? AdditionalInfo { get; set; }
}

/// <summary>
/// Receipt data model for template rendering
/// </summary>
public class ReceiptData
{
    // Company Information
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string? CompanyTaxNumber { get; set; }

    // Branch Information
    public string BranchName { get; set; } = string.Empty;
    public string? BranchAddress { get; set; }
    public string? BranchPhone { get; set; }

    // Transaction Information
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public int TransactionTypeId { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;

    // Customer Information
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    // Transaction Details
    public List<ReceiptLineItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal MakingCharges { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public List<TaxLineItem> Taxes { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;

    // Additional Information
    public string? SpecialInstructions { get; set; }
    public string? ReturnPolicy { get; set; }
    public string? WarrantyInfo { get; set; }

    // For Returns
    public string? OriginalTransactionNumber { get; set; }
    public string? ReturnReason { get; set; }

    // For Repairs
    public string? RepairDescription { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
}

/// <summary>
/// Tax line item for receipt
/// </summary>
public class TaxLineItem
{
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}

/// <summary>
/// Browser receipt data DTO - contains all data needed for frontend receipt generation
/// </summary>
public class BrowserReceiptDataDto
{
    public ReceiptData ReceiptData { get; set; } = new();
    public ReceiptTemplate Template { get; set; } = new();
    public string HtmlTemplate { get; set; } = string.Empty;
    public string CssStyles { get; set; } = string.Empty;
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public int TransactionTypeId { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
}

/// <summary>
/// Generate receipt for browser printing request DTO
/// </summary>
public class GenerateBrowserReceiptRequestDto
{
    public int TransactionId { get; set; }
    
    public bool IncludeHtml { get; set; } = true;
    public bool IncludeCss { get; set; } = true;
}
