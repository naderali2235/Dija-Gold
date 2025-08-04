using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for receipt generation and printing service
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// Generate receipt content for a transaction
    /// </summary>
    /// <param name="transaction">Transaction to generate receipt for</param>
    /// <returns>Receipt content as string</returns>
    Task<string> GenerateReceiptContentAsync(Transaction transaction);

    /// <summary>
    /// Generate receipt content for a repair transaction
    /// </summary>
    /// <param name="transaction">Repair transaction</param>
    /// <returns>Repair receipt content as string</returns>
    Task<string> GenerateRepairReceiptContentAsync(Transaction transaction);

    /// <summary>
    /// Generate receipt content for a return transaction
    /// </summary>
    /// <param name="transaction">Return transaction</param>
    /// <returns>Return receipt content as string</returns>
    Task<string> GenerateReturnReceiptContentAsync(Transaction transaction);

    /// <summary>
    /// Print receipt to default printer
    /// </summary>
    /// <param name="receiptContent">Receipt content to print</param>
    /// <param name="copies">Number of copies to print</param>
    /// <returns>Success status</returns>
    Task<bool> PrintReceiptAsync(string receiptContent, int copies = 1);

    /// <summary>
    /// Print receipt to specific printer
    /// </summary>
    /// <param name="receiptContent">Receipt content to print</param>
    /// <param name="printerName">Printer name</param>
    /// <param name="copies">Number of copies to print</param>
    /// <returns>Success status</returns>
    Task<bool> PrintReceiptAsync(string receiptContent, string printerName, int copies = 1);

    /// <summary>
    /// Generate receipt PDF
    /// </summary>
    /// <param name="transaction">Transaction to generate PDF for</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> GenerateReceiptPdfAsync(Transaction transaction);

    /// <summary>
    /// Get receipt template by transaction type
    /// </summary>
    /// <param name="transactionType">Transaction type</param>
    /// <returns>Receipt template</returns>
    Task<ReceiptTemplate> GetReceiptTemplateAsync(Models.Enums.TransactionType transactionType);

    /// <summary>
    /// Update receipt template
    /// </summary>
    /// <param name="template">Updated template</param>
    /// <param name="userId">User updating the template</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateReceiptTemplateAsync(ReceiptTemplate template, string userId);

    /// <summary>
    /// Reprint receipt for existing transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="userId">User requesting reprint</param>
    /// <param name="copies">Number of copies</param>
    /// <returns>Success status and receipt content</returns>
    Task<(bool Success, string? ReceiptContent)> ReprintReceiptAsync(int transactionId, string userId, int copies = 1);
}

/// <summary>
/// Receipt template model
/// </summary>
public class ReceiptTemplate
{
    public int Id { get; set; }
    public Models.Enums.TransactionType TransactionType { get; set; }
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
    public Models.Enums.TransactionType TransactionType { get; set; }
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