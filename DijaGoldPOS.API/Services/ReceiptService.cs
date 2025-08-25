using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Text;
// Remove Windows-specific printing imports
// #if WINDOWS
// using System.Drawing.Printing;
// #endif

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
    public async Task<string> GenerateReceiptContentAsync(FinancialTransaction transaction)
    {
        try
        {
            // Load transaction with all related data - using FinancialTransactions
            var fullTransaction = await _context.FinancialTransactions
                .Include(ft => ft.Branch)
                .Include(ft => ft.ProcessedByUser)
                .FirstOrDefaultAsync(ft => ft.Id == transaction.Id);

            if (fullTransaction == null)
            {
                throw new InvalidOperationException($"Transaction {transaction.Id} not found");
            }

            var receiptData = await BuildReceiptDataAsync(fullTransaction);
            var template = await GetReceiptTemplateAsync(fullTransaction.TransactionTypeId);
            
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
    public async Task<string> GenerateRepairReceiptContentAsync(FinancialTransaction transaction)
    {
        if (transaction.TransactionTypeId != LookupTableConstants.TransactionTypeRepair)
        {
            throw new ArgumentException("Transaction must be a repair transaction");
        }

        return await GenerateReceiptContentAsync(transaction);
    }

    /// <summary>
    /// Generate receipt content for a return transaction
    /// </summary>
    public async Task<string> GenerateReturnReceiptContentAsync(FinancialTransaction transaction)
    {
        if (transaction.TransactionTypeId != LookupTableConstants.TransactionTypeReturn)
        {
            throw new ArgumentException("Transaction must be a return transaction");
        }

        return await GenerateReceiptContentAsync(transaction);
    }





    /// <summary>
    /// Get receipt template by transaction type
    /// </summary>
    public Task<ReceiptTemplate> GetReceiptTemplateAsync(int transactionTypeId)
    {
        // In a full implementation, this would come from database
        // For now, return default templates
        
        var companyInfo = _configuration.GetSection("CompanyInfo");
        var receiptSettings = _configuration.GetSection("ReceiptSettings");
        
        var template = new ReceiptTemplate
        {
            Id = transactionTypeId,
            TransactionTypeId = transactionTypeId,
            TemplateName = $"Transaction Type {transactionTypeId} Receipt Template",
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

        return Task.FromResult(template);
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
                $"Updated receipt template for transaction type {template.TransactionTypeId}",
                newValues: System.Text.Json.JsonSerializer.Serialize(template)
            );

            _logger.LogInformation("Receipt template updated for transaction type {TransactionTypeId} by user {UserId}", 
                template.TransactionTypeId, userId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receipt template for transaction type {TransactionTypeId}", template.TransactionTypeId);
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
            var transaction = await _context.FinancialTransactions
                .Include(ft => ft.Branch)
                .Include(ft => ft.ProcessedByUser)
                .FirstOrDefaultAsync(ft => ft.Id == transactionId);

            if (transaction == null)
            {
                return (false, null);
            }

            var receiptContent = await GenerateReceiptContentAsync(transaction);
            
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

            return (true, receiptContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprinting receipt for transaction {TransactionId}", transactionId);
            return (false, null);
        }
    }

    /// <summary>
    /// Generate receipt data and template for browser printing
    /// </summary>
    public async Task<BrowserReceiptDataDto> GenerateBrowserReceiptAsync(int transactionId)
    {
        try
        {
            var transaction = await _context.FinancialTransactions
                .Include(ft => ft.Branch)
                .Include(ft => ft.ProcessedByUser)
                .FirstOrDefaultAsync(ft => ft.Id == transactionId);

            if (transaction == null)
            {
                throw new InvalidOperationException($"Transaction {transactionId} not found");
            }

            var receiptData = await BuildReceiptDataAsync(transaction);
            var template = await GetReceiptTemplateAsync(transaction.TransactionTypeId);
            var htmlTemplate = await GenerateHtmlReceiptTemplateAsync(receiptData, template);
            var cssStyles = await GenerateReceiptCssStylesAsync(template);

            var browserReceipt = new BrowserReceiptDataDto
            {
                ReceiptData = receiptData,
                Template = template,
                HtmlTemplate = htmlTemplate,
                CssStyles = cssStyles,
                TransactionNumber = transaction.TransactionNumber,
                TransactionDate = transaction.TransactionDate,
                TransactionTypeId = transaction.TransactionTypeId,
                TransactionTypeName = $"Transaction Type {transaction.TransactionTypeId}"
            };

            _logger.LogInformation("Browser receipt generated for transaction {TransactionId}", transactionId);
            return browserReceipt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating browser receipt for transaction {TransactionId}", transactionId);
            throw;
        }
    }

    /// <summary>
    /// Generate HTML receipt template
    /// </summary>
    public async Task<string> GenerateHtmlReceiptTemplateAsync(ReceiptData receiptData, ReceiptTemplate template)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>Receipt - " + receiptData.TransactionNumber + "</title>");
        html.AppendLine("    <style>");
        html.AppendLine(await GenerateReceiptCssStylesAsync(template));
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("    <div class=\"receipt-container\">");
        
        // Header
        html.AppendLine("        <div class=\"receipt-header\">");
        foreach (var headerLine in template.HeaderLines)
        {
            html.AppendLine($"            <div class=\"header-line\">{headerLine}</div>");
        }
        html.AppendLine("        </div>");
        
        html.AppendLine("        <div class=\"receipt-divider\"></div>");
        
        // Transaction Info
        html.AppendLine("        <div class=\"transaction-info\">");
        html.AppendLine($"            <div class=\"info-line\"><span class=\"label\">Receipt No:</span> <span class=\"value\">{receiptData.TransactionNumber}</span></div>");
        html.AppendLine($"            <div class=\"info-line\"><span class=\"label\">Date:</span> <span class=\"value\">{receiptData.TransactionDate.ToString(template.Settings.DateTimeFormat)}</span></div>");
        html.AppendLine($"            <div class=\"info-line\"><span class=\"label\">Cashier:</span> <span class=\"value\">{receiptData.CashierName}</span></div>");
        html.AppendLine($"            <div class=\"info-line\"><span class=\"label\">Branch:</span> <span class=\"value\">{receiptData.BranchName}</span></div>");
        
        if (!string.IsNullOrEmpty(receiptData.CustomerName))
        {
            html.AppendLine($"            <div class=\"info-line\"><span class=\"label\">Customer:</span> <span class=\"value\">{receiptData.CustomerName}</span></div>");
            if (!string.IsNullOrEmpty(receiptData.CustomerPhone))
                html.AppendLine($"            <div class=\"info-line\"><span class=\"label\">Phone:</span> <span class=\"value\">{receiptData.CustomerPhone}</span></div>");
        }
        html.AppendLine("        </div>");
        
        html.AppendLine("        <div class=\"receipt-divider\"></div>");
        
        // Items
        if (template.Settings.ShowItemDetails && receiptData.Items.Any())
        {
            html.AppendLine("        <div class=\"items-section\">");
            html.AppendLine("            <div class=\"section-title\">ITEMS:</div>");
            foreach (var item in receiptData.Items)
            {
                html.AppendLine("            <div class=\"item\">");
                html.AppendLine($"                <div class=\"item-description\">{item.Description}</div>");
                html.AppendLine($"                <div class=\"item-details\">{item.Quantity:F3} x {item.UnitPrice.ToString(template.Settings.CurrencyFormat)} = {item.Total.ToString(template.Settings.CurrencyFormat)}</div>");
                if (!string.IsNullOrEmpty(item.AdditionalInfo))
                    html.AppendLine($"                <div class=\"item-additional\">{item.AdditionalInfo}</div>");
                html.AppendLine("            </div>");
            }
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"receipt-divider\"></div>");
        }
        
        // Totals
        html.AppendLine("        <div class=\"totals-section\">");
        if (receiptData.Subtotal > 0)
            html.AppendLine($"            <div class=\"total-line\"><span class=\"label\">Subtotal:</span> <span class=\"value\">{receiptData.Subtotal.ToString(template.Settings.CurrencyFormat)}</span></div>");
        
        if (receiptData.MakingCharges > 0)
            html.AppendLine($"            <div class=\"total-line\"><span class=\"label\">Making Charges:</span> <span class=\"value\">{receiptData.MakingCharges.ToString(template.Settings.CurrencyFormat)}</span></div>");
        
        if (receiptData.DiscountAmount > 0)
            html.AppendLine($"            <div class=\"total-line\"><span class=\"label\">Discount:</span> <span class=\"value\">{(-receiptData.DiscountAmount).ToString(template.Settings.CurrencyFormat)}</span></div>");
        
        // Taxes
        if (template.Settings.ShowTaxBreakdown && receiptData.Taxes.Any())
        {
            foreach (var tax in receiptData.Taxes)
            {
                html.AppendLine($"            <div class=\"total-line\"><span class=\"label\">{tax.TaxName} ({tax.TaxRate:F2}%):</span> <span class=\"value\">{tax.TaxAmount.ToString(template.Settings.CurrencyFormat)}</span></div>");
            }
        }
        
        html.AppendLine("        </div>");
        
        html.AppendLine("        <div class=\"receipt-divider\"></div>");
        html.AppendLine($"        <div class=\"grand-total\"><span class=\"label\">TOTAL:</span> <span class=\"value\">{receiptData.TotalAmount.ToString(template.Settings.CurrencyFormat)}</span></div>");
        
        if (receiptData.TransactionTypeId != LookupTableConstants.TransactionTypeReturn)
        {
            html.AppendLine($"        <div class=\"payment-info\"><span class=\"label\">Paid:</span> <span class=\"value\">{receiptData.AmountPaid.ToString(template.Settings.CurrencyFormat)}</span></div>");
            
            if (receiptData.ChangeGiven > 0)
                html.AppendLine($"        <div class=\"payment-info\"><span class=\"label\">Change:</span> <span class=\"value\">{receiptData.ChangeGiven.ToString(template.Settings.CurrencyFormat)}</span></div>");
            else if (receiptData.ChangeGiven < 0)
                html.AppendLine($"        <div class=\"payment-info\"><span class=\"label\">Refund:</span> <span class=\"value\">{(-receiptData.ChangeGiven).ToString(template.Settings.CurrencyFormat)}</span></div>");
        }
        
        html.AppendLine($"        <div class=\"payment-method\">Payment: {receiptData.PaymentMethod}</div>");
        html.AppendLine("        <div class=\"receipt-divider\"></div>");
        
        // Transaction specific information
        switch (receiptData.TransactionTypeId)
        {
            case LookupTableConstants.TransactionTypeReturn:
                html.AppendLine("        <div class=\"transaction-type\">*** RETURN RECEIPT ***</div>");
                if (!string.IsNullOrEmpty(receiptData.OriginalTransactionNumber))
                    html.AppendLine($"        <div class=\"special-info\">Original Receipt: {receiptData.OriginalTransactionNumber}</div>");
                if (!string.IsNullOrEmpty(receiptData.ReturnReason))
                    html.AppendLine($"        <div class=\"special-info\">Reason: {receiptData.ReturnReason}</div>");
                break;
                
            case LookupTableConstants.TransactionTypeRepair:
                html.AppendLine("        <div class=\"transaction-type\">*** REPAIR RECEIPT ***</div>");
                if (!string.IsNullOrEmpty(receiptData.RepairDescription))
                    html.AppendLine($"        <div class=\"special-info\">Service: {receiptData.RepairDescription}</div>");
                if (receiptData.EstimatedCompletionDate.HasValue)
                    html.AppendLine($"        <div class=\"special-info\">Est. Completion: {receiptData.EstimatedCompletionDate.Value:dd/MM/yyyy}</div>");
                break;
        }
        
        // Footer
        html.AppendLine("        <div class=\"receipt-footer\">");
        foreach (var footerLine in template.FooterLines)
        {
            html.AppendLine($"            <div class=\"footer-line\">{footerLine}</div>");
        }
        html.AppendLine("        </div>");
        
        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    /// <summary>
    /// Generate CSS styles for receipt printing
    /// </summary>
    public Task<string> GenerateReceiptCssStylesAsync(ReceiptTemplate template)
    {
        var css = new StringBuilder();
        
        css.AppendLine("@media print {");
        css.AppendLine("    body { margin: 0; padding: 0; }");
        css.AppendLine("    .receipt-container { width: 80mm; margin: 0 auto; }");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine("@media screen {");
        css.AppendLine("    body { font-family: Arial, sans-serif; margin: 20px; }");
        css.AppendLine("    .receipt-container { width: 400px; margin: 0 auto; border: 1px solid #ccc; padding: 20px; }");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".receipt-container {");
        css.AppendLine("    font-family: 'Courier New', monospace;");
        css.AppendLine($"    font-size: {template.Settings.FontSize}px;");
        css.AppendLine("    line-height: 1.2;");
        css.AppendLine("    text-align: center;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".receipt-header {");
        css.AppendLine("    margin-bottom: 10px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".header-line {");
        css.AppendLine("    font-weight: bold;");
        css.AppendLine("    margin-bottom: 2px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".receipt-divider {");
        css.AppendLine("    border-top: 1px dashed #000;");
        css.AppendLine("    margin: 10px 0;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".transaction-info {");
        css.AppendLine("    text-align: left;");
        css.AppendLine("    margin-bottom: 10px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".info-line {");
        css.AppendLine("    margin-bottom: 2px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".label {");
        css.AppendLine("    font-weight: bold;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".value {");
        css.AppendLine("    margin-left: 5px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".items-section {");
        css.AppendLine("    text-align: left;");
        css.AppendLine("    margin-bottom: 10px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".section-title {");
        css.AppendLine("    font-weight: bold;");
        css.AppendLine("    text-align: center;");
        css.AppendLine("    margin-bottom: 5px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".item {");
        css.AppendLine("    margin-bottom: 5px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".item-description {");
        css.AppendLine("    font-weight: bold;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".item-details {");
        css.AppendLine("    margin-left: 10px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".item-additional {");
        css.AppendLine("    margin-left: 10px;");
        css.AppendLine("    font-size: 0.9em;");
        css.AppendLine("    color: #666;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".totals-section {");
        css.AppendLine("    text-align: left;");
        css.AppendLine("    margin-bottom: 10px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".total-line {");
        css.AppendLine("    display: flex;");
        css.AppendLine("    justify-content: space-between;");
        css.AppendLine("    margin-bottom: 2px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".grand-total {");
        css.AppendLine("    display: flex;");
        css.AppendLine("    justify-content: space-between;");
        css.AppendLine("    font-weight: bold;");
        css.AppendLine("    font-size: 1.1em;");
        css.AppendLine("    margin-bottom: 5px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".payment-info {");
        css.AppendLine("    display: flex;");
        css.AppendLine("    justify-content: space-between;");
        css.AppendLine("    margin-bottom: 2px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".payment-method {");
        css.AppendLine("    text-align: center;");
        css.AppendLine("    margin-bottom: 10px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".transaction-type {");
        css.AppendLine("    text-align: center;");
        css.AppendLine("    font-weight: bold;");
        css.AppendLine("    margin-bottom: 5px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".special-info {");
        css.AppendLine("    text-align: left;");
        css.AppendLine("    margin-bottom: 2px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".receipt-footer {");
        css.AppendLine("    margin-top: 10px;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine(".footer-line {");
        css.AppendLine("    font-size: 0.9em;");
        css.AppendLine("    margin-bottom: 2px;");
        css.AppendLine("}");
        
        return Task.FromResult(css.ToString());
    }

    #region Private Helper Methods

    /// <summary>
    /// Build receipt data from transaction
    /// </summary>
    private Task<ReceiptData> BuildReceiptDataAsync(FinancialTransaction transaction)
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
            TransactionTypeId = transaction.TransactionTypeId,
            TransactionTypeName = $"Transaction Type {transaction.TransactionTypeId}",
            CashierName = transaction.ProcessedByUser?.FullName ?? "",

            // Customer Information - Note: Customer info is now in the related Order
            CustomerName = null, // Will be populated from Order if available
            CustomerPhone = null, // Will be populated from Order if available
            CustomerEmail = null, // Will be populated from Order if available

            // Transaction Details
            Subtotal = transaction.Subtotal,
            MakingCharges = 0, // Making charges are now in Order items
            DiscountAmount = transaction.TotalDiscountAmount,
            TaxableAmount = transaction.Subtotal - transaction.TotalDiscountAmount,
            TotalAmount = transaction.TotalAmount,
            AmountPaid = transaction.AmountPaid,
            ChangeGiven = transaction.ChangeGiven,
            PaymentMethod = transaction.PaymentMethod?.ToString() ?? "Unknown",

            // Additional Information for specific transaction types
            ReturnPolicy = "Returns accepted within 7 days with receipt",
            WarrantyInfo = "Gold purity guaranteed as marked"
        };

        // Add items from Order - Note: Order items are now accessed through BusinessEntityId
        // For now, we'll leave items empty as they're in the related Order
        // TODO: Implement proper Order loading when needed

        // Add taxes - using total tax amount from FinancialTransaction
        if (transaction.TotalTaxAmount > 0)
        {
            var taxItem = new TaxLineItem
            {
                TaxName = "Tax",
                TaxRate = 0, // Rate not available in FinancialTransaction
                TaxAmount = transaction.TotalTaxAmount
            };
            receiptData.Taxes.Add(taxItem);
        }

        // Add specific information based on transaction type
        switch (transaction.TransactionTypeId)
        {
            case LookupTableConstants.TransactionTypeReturn:
#pragma warning disable CS8601 // Possible null reference assignment
                receiptData.OriginalTransactionNumber = transaction.OriginalTransaction?.TransactionNumber;
#pragma warning restore CS8601 // Possible null reference assignment
                receiptData.ReturnReason = transaction.ReversalReason;
                break;
                
            case LookupTableConstants.TransactionTypeRepair:
                // Repair-specific info is now in the related RepairJob
                // TODO: Implement proper RepairJob loading when needed
                break;
        }

        return Task.FromResult(receiptData);
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
        
        if (receiptData.TransactionTypeId != LookupTableConstants.TransactionTypeReturn)
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
        switch (receiptData.TransactionTypeId)
        {
            case LookupTableConstants.TransactionTypeReturn:
                receipt.AppendLine("*** RETURN RECEIPT ***");
                if (!string.IsNullOrEmpty(receiptData.OriginalTransactionNumber))
                    receipt.AppendLine($"Original Receipt: {receiptData.OriginalTransactionNumber}");
                if (!string.IsNullOrEmpty(receiptData.ReturnReason))
                    receipt.AppendLine($"Reason: {receiptData.ReturnReason}");
                break;
                
            case LookupTableConstants.TransactionTypeRepair:
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
