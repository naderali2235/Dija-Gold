using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Text;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Receipt generation and printing service implementation
/// </summary>
public class ReceiptService : IReceiptService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReceiptService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;

    public ReceiptService(
        ApplicationDbContext context,
        ILogger<ReceiptService> logger,
        IConfiguration configuration,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _auditService = auditService;
    }

    /// <summary>
    /// Generate receipt content for a transaction
    /// </summary>
    public async Task<string> GenerateReceiptContentAsync(Transaction transaction)
    {
        try
        {
            // Load transaction with all related data
            var fullTransaction = await _context.Transactions
                .Include(t => t.Branch)
                .Include(t => t.Customer)
                .Include(t => t.Cashier)
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
                .Include(t => t.TransactionTaxes)
                .ThenInclude(tt => tt.TaxConfiguration)
                .FirstOrDefaultAsync(t => t.Id == transaction.Id);

            if (fullTransaction == null)
            {
                throw new InvalidOperationException($"Transaction {transaction.Id} not found");
            }

            var receiptData = await BuildReceiptDataAsync(fullTransaction);
            var template = await GetReceiptTemplateAsync(fullTransaction.TransactionType);
            
            return GenerateReceiptFromTemplate(receiptData, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating receipt for transaction {TransactionId}", transaction.Id);
            throw;
        }
    }

    /// <summary>
    /// Generate receipt content for a repair transaction
    /// </summary>
    public async Task<string> GenerateRepairReceiptContentAsync(Transaction transaction)
    {
        if (transaction.TransactionType != TransactionType.Repair)
        {
            throw new ArgumentException("Transaction must be a repair transaction");
        }

        return await GenerateReceiptContentAsync(transaction);
    }

    /// <summary>
    /// Generate receipt content for a return transaction
    /// </summary>
    public async Task<string> GenerateReturnReceiptContentAsync(Transaction transaction)
    {
        if (transaction.TransactionType != TransactionType.Return)
        {
            throw new ArgumentException("Transaction must be a return transaction");
        }

        return await GenerateReceiptContentAsync(transaction);
    }

    /// <summary>
    /// Print receipt to default printer
    /// </summary>
    public async Task<bool> PrintReceiptAsync(string receiptContent, int copies = 1)
    {
        var defaultPrinter = _configuration["ReceiptSettings:PrinterName"] ?? "Default";
        return await PrintReceiptAsync(receiptContent, defaultPrinter, copies);
    }

    /// <summary>
    /// Print receipt to specific printer
    /// </summary>
    public async Task<bool> PrintReceiptAsync(string receiptContent, string printerName, int copies = 1)
    {
        try
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < copies; i++)
                {
                    var printDocument = new PrintDocument();
                    printDocument.PrinterSettings.PrinterName = printerName;
                    
                    printDocument.PrintPage += (sender, e) =>
                    {
                        if (e.Graphics != null)
                        {
                            var font = new System.Drawing.Font("Courier New", 8);
                            var brush = System.Drawing.Brushes.Black;
                            var lines = receiptContent.Split('\n');
                            
                            float yPosition = 0;
                            float lineHeight = font.GetHeight(e.Graphics);
                            
                            foreach (var line in lines)
                            {
                                e.Graphics.DrawString(line, font, brush, 0, yPosition);
                                yPosition += lineHeight;
                            }
                        }
                    };
                    
                    printDocument.Print();
                }
            });

            _logger.LogInformation("Receipt printed successfully to {PrinterName}, {Copies} copies", printerName, copies);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing receipt to {PrinterName}", printerName);
            return false;
        }
    }

    /// <summary>
    /// Generate receipt PDF
    /// </summary>
    public async Task<byte[]> GenerateReceiptPdfAsync(Transaction transaction)
    {
        try
        {
            var receiptContent = await GenerateReceiptContentAsync(transaction);
            
            // Simple PDF generation - in production, use a proper PDF library like iTextSharp or QuestPDF
            var pdfBytes = Encoding.UTF8.GetBytes($"PDF Receipt Content:\n\n{receiptContent}");
            
            _logger.LogInformation("PDF receipt generated for transaction {TransactionId}", transaction.Id);
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF receipt for transaction {TransactionId}", transaction.Id);
            throw;
        }
    }

    /// <summary>
    /// Get receipt template by transaction type
    /// </summary>
    public async Task<ReceiptTemplate> GetReceiptTemplateAsync(TransactionType transactionType)
    {
        // In a full implementation, this would come from database
        // For now, return default templates
        
        var companyInfo = _configuration.GetSection("CompanyInfo");
        var receiptSettings = _configuration.GetSection("ReceiptSettings");
        
        return new ReceiptTemplate
        {
            Id = (int)transactionType,
            TransactionType = transactionType,
            TemplateName = $"{transactionType} Receipt Template",
            HeaderLines = receiptSettings.GetSection("HeaderLines").Get<List<string>>() ?? new List<string>(),
            FooterLines = receiptSettings.GetSection("FooterLines").Get<List<string>>() ?? new List<string>(),
            Settings = new ReceiptSettings
            {
                PaperWidth = 80,
                FontName = "Courier New",
                FontSize = 10,
                DateTimeFormat = "dd/MM/yyyy HH:mm:ss",
                CurrencyFormat = "0.00 EGP",
                ShowItemDetails = true,
                ShowTaxBreakdown = true
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    /// <summary>
    /// Update receipt template
    /// </summary>
    public async Task<bool> UpdateReceiptTemplateAsync(ReceiptTemplate template, string userId)
    {
        try
        {
            // In a full implementation, this would update the database
            // For now, log the update
            
            await _auditService.LogAsync(
                userId,
                "UPDATE_RECEIPT_TEMPLATE",
                "ReceiptTemplate",
                template.Id.ToString(),
                $"Updated receipt template for {template.TransactionType}",
                newValues: System.Text.Json.JsonSerializer.Serialize(template)
            );

            _logger.LogInformation("Receipt template updated for {TransactionType} by user {UserId}", 
                template.TransactionType, userId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receipt template for {TransactionType}", template.TransactionType);
            return false;
        }
    }

    /// <summary>
    /// Reprint receipt for existing transaction
    /// </summary>
    public async Task<(bool Success, string? ReceiptContent)> ReprintReceiptAsync(int transactionId, string userId, int copies = 1)
    {
        try
        {
            var transaction = await _context.Transactions
                .Include(t => t.Branch)
                .Include(t => t.Customer)
                .Include(t => t.Cashier)
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
                .Include(t => t.TransactionTaxes)
                .ThenInclude(tt => tt.TaxConfiguration)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
            {
                return (false, null);
            }

            var receiptContent = await GenerateReceiptContentAsync(transaction);
            var printSuccess = await PrintReceiptAsync(receiptContent, copies);

            if (printSuccess)
            {
                // Log the reprint action
                await _auditService.LogAsync(
                    userId,
                    "REPRINT_RECEIPT",
                    "Transaction",
                    transactionId.ToString(),
                    $"Receipt reprinted for transaction {transaction.TransactionNumber}",
                    branchId: transaction.BranchId,
                    transactionId: transactionId
                );

                _logger.LogInformation("Receipt reprinted for transaction {TransactionNumber} by user {UserId}", 
                    transaction.TransactionNumber, userId);
            }

            return (printSuccess, receiptContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprinting receipt for transaction {TransactionId}", transactionId);
            return (false, null);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Build receipt data from transaction
    /// </summary>
    private async Task<ReceiptData> BuildReceiptDataAsync(Transaction transaction)
    {
        var companyInfo = _configuration.GetSection("CompanyInfo");
        
        var receiptData = new ReceiptData
        {
            // Company Information
            CompanyName = companyInfo["Name"] ?? "Dija Gold",
            CompanyAddress = companyInfo["Address"] ?? "",
            CompanyPhone = companyInfo["Phone"] ?? "",
            CompanyTaxNumber = companyInfo["TaxRegistrationNumber"],

            // Branch Information
            BranchName = transaction.Branch?.Name ?? "",
            BranchAddress = transaction.Branch?.Address,
            BranchPhone = transaction.Branch?.Phone,

            // Transaction Information
            TransactionNumber = transaction.TransactionNumber,
            TransactionDate = transaction.TransactionDate,
            TransactionType = transaction.TransactionType,
            CashierName = transaction.Cashier?.FullName ?? "",

            // Customer Information
            CustomerName = transaction.Customer?.FullName,
            CustomerPhone = transaction.Customer?.MobileNumber,
            CustomerEmail = transaction.Customer?.Email,

            // Transaction Details
            Subtotal = transaction.Subtotal,
            MakingCharges = transaction.TotalMakingCharges,
            DiscountAmount = transaction.DiscountAmount,
            TaxableAmount = transaction.Subtotal + transaction.TotalMakingCharges - transaction.DiscountAmount,
            TotalAmount = transaction.TotalAmount,
            AmountPaid = transaction.AmountPaid,
            ChangeGiven = transaction.ChangeGiven,
            PaymentMethod = transaction.PaymentMethod.ToString(),

            // Additional Information for specific transaction types
            ReturnPolicy = "Returns accepted within 7 days with receipt",
            WarrantyInfo = "Gold purity guaranteed as marked"
        };

        // Add items
        foreach (var item in transaction.TransactionItems)
        {
            var receiptItem = new ReceiptLineItem
            {
                Description = $"{item.Product?.Name} ({item.Product?.KaratType}K)",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.LineTotal,
                AdditionalInfo = $"Weight: {item.TotalWeight:F3}g"
            };
            receiptData.Items.Add(receiptItem);
        }

        // Add taxes
        foreach (var tax in transaction.TransactionTaxes)
        {
            var taxItem = new TaxLineItem
            {
                TaxName = tax.TaxConfiguration?.TaxName ?? "",
                TaxRate = tax.TaxRate,
                TaxAmount = tax.TaxAmount
            };
            receiptData.Taxes.Add(taxItem);
        }

        // Add specific information based on transaction type
        switch (transaction.TransactionType)
        {
            case TransactionType.Return:
                receiptData.OriginalTransactionNumber = transaction.OriginalTransaction?.TransactionNumber;
                receiptData.ReturnReason = transaction.ReturnReason;
                break;
                
            case TransactionType.Repair:
                receiptData.RepairDescription = transaction.RepairDescription;
                receiptData.EstimatedCompletionDate = transaction.EstimatedCompletionDate;
                break;
        }

        return receiptData;
    }

    /// <summary>
    /// Generate receipt from template and data
    /// </summary>
    private string GenerateReceiptFromTemplate(ReceiptData receiptData, ReceiptTemplate template)
    {
        var receipt = new StringBuilder();
        var width = template.Settings.PaperWidth;
        
        // Header
        foreach (var headerLine in template.HeaderLines)
        {
            receipt.AppendLine(CenterText(headerLine, width));
        }
        
        receipt.AppendLine(new string('=', width));
        
        // Transaction Info
        receipt.AppendLine($"Receipt No: {receiptData.TransactionNumber}");
        receipt.AppendLine($"Date: {receiptData.TransactionDate.ToString(template.Settings.DateTimeFormat)}");
        receipt.AppendLine($"Cashier: {receiptData.CashierName}");
        receipt.AppendLine($"Branch: {receiptData.BranchName}");
        
        if (!string.IsNullOrEmpty(receiptData.CustomerName))
        {
            receipt.AppendLine($"Customer: {receiptData.CustomerName}");
            if (!string.IsNullOrEmpty(receiptData.CustomerPhone))
                receipt.AppendLine($"Phone: {receiptData.CustomerPhone}");
        }
        
        receipt.AppendLine(new string('-', width));
        
        // Items
        if (template.Settings.ShowItemDetails && receiptData.Items.Any())
        {
            receipt.AppendLine("ITEMS:");
            foreach (var item in receiptData.Items)
            {
                receipt.AppendLine($"{item.Description}");
                receipt.AppendLine($"  {item.Quantity:F3} x {item.UnitPrice.ToString(template.Settings.CurrencyFormat)} = {item.Total.ToString(template.Settings.CurrencyFormat)}");
                if (!string.IsNullOrEmpty(item.AdditionalInfo))
                    receipt.AppendLine($"  {item.AdditionalInfo}");
            }
            receipt.AppendLine(new string('-', width));
        }
        
        // Totals
        if (receiptData.Subtotal > 0)
            receipt.AppendLine($"{"Subtotal:",-20} {receiptData.Subtotal.ToString(template.Settings.CurrencyFormat),20}");
        
        if (receiptData.MakingCharges > 0)
            receipt.AppendLine($"{"Making Charges:",-20} {receiptData.MakingCharges.ToString(template.Settings.CurrencyFormat),20}");
        
        if (receiptData.DiscountAmount > 0)
            receipt.AppendLine($"{"Discount:",-20} {(-receiptData.DiscountAmount).ToString(template.Settings.CurrencyFormat),20}");
        
        // Taxes
        if (template.Settings.ShowTaxBreakdown && receiptData.Taxes.Any())
        {
            foreach (var tax in receiptData.Taxes)
            {
                receipt.AppendLine($"{$"{tax.TaxName} ({tax.TaxRate:F2}%):",-20} {tax.TaxAmount.ToString(template.Settings.CurrencyFormat),20}");
            }
        }
        
        receipt.AppendLine(new string('=', width));
        receipt.AppendLine($"{"TOTAL:",-20} {receiptData.TotalAmount.ToString(template.Settings.CurrencyFormat),20}");
        
        if (receiptData.TransactionType != TransactionType.Return)
        {
            receipt.AppendLine($"{"Paid:",-20} {receiptData.AmountPaid.ToString(template.Settings.CurrencyFormat),20}");
            
            if (receiptData.ChangeGiven > 0)
                receipt.AppendLine($"{"Change:",-20} {receiptData.ChangeGiven.ToString(template.Settings.CurrencyFormat),20}");
            else if (receiptData.ChangeGiven < 0)
                receipt.AppendLine($"{"Refund:",-20} {(-receiptData.ChangeGiven).ToString(template.Settings.CurrencyFormat),20}");
        }
        
        receipt.AppendLine($"Payment: {receiptData.PaymentMethod}");
        receipt.AppendLine(new string('=', width));
        
        // Transaction specific information
        switch (receiptData.TransactionType)
        {
            case TransactionType.Return:
                receipt.AppendLine("*** RETURN RECEIPT ***");
                if (!string.IsNullOrEmpty(receiptData.OriginalTransactionNumber))
                    receipt.AppendLine($"Original Receipt: {receiptData.OriginalTransactionNumber}");
                if (!string.IsNullOrEmpty(receiptData.ReturnReason))
                    receipt.AppendLine($"Reason: {receiptData.ReturnReason}");
                break;
                
            case TransactionType.Repair:
                receipt.AppendLine("*** REPAIR RECEIPT ***");
                if (!string.IsNullOrEmpty(receiptData.RepairDescription))
                    receipt.AppendLine($"Service: {receiptData.RepairDescription}");
                if (receiptData.EstimatedCompletionDate.HasValue)
                    receipt.AppendLine($"Est. Completion: {receiptData.EstimatedCompletionDate.Value:dd/MM/yyyy}");
                break;
        }
        
        // Footer
        foreach (var footerLine in template.FooterLines)
        {
            receipt.AppendLine(CenterText(footerLine, width));
        }
        
        return receipt.ToString();
    }

    /// <summary>
    /// Center text within specified width
    /// </summary>
    private string CenterText(string text, int width)
    {
        if (text.Length >= width) return text;
        
        var padding = (width - text.Length) / 2;
        return text.PadLeft(text.Length + padding).PadRight(width);
    }

    #endregion
}