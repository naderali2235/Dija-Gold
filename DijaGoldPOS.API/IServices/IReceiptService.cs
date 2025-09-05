using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.FinancialModels;

namespace DijaGoldPOS.API.IServices;

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
    Task<string> GenerateReceiptContentAsync(FinancialTransaction transaction);

    /// <summary>
    /// Generate receipt content for a repair transaction
    /// </summary>
    /// <param name="transaction">Repair transaction</param>
    /// <returns>Repair receipt content as string</returns>
    Task<string> GenerateRepairReceiptContentAsync(FinancialTransaction transaction);

    /// <summary>
    /// Generate receipt content for a return transaction
    /// </summary>
    /// <param name="transaction">Return transaction</param>
    /// <returns>Return receipt content as string</returns>
    Task<string> GenerateReturnReceiptContentAsync(FinancialTransaction transaction);

    /// <summary>
    /// Get receipt template by transaction type
    /// </summary>
    /// <param name="transactionTypeId">Transaction type ID</param>
    /// <returns>Receipt template</returns>
    Task<ReceiptTemplate> GetReceiptTemplateAsync(int transactionTypeId);

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

    /// <summary>
    /// Generate receipt data and template for browser printing
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Receipt data and template for frontend rendering</returns>
    Task<BrowserReceiptDataDto> GenerateBrowserReceiptAsync(int transactionId);

    /// <summary>
    /// Generate HTML receipt template
    /// </summary>
    /// <param name="receiptData">Receipt data</param>
    /// <param name="template">Receipt template</param>
    /// <returns>HTML template string</returns>
    Task<string> GenerateHtmlReceiptTemplateAsync(ReceiptData receiptData, ReceiptTemplate template);

    /// <summary>
    /// Generate CSS styles for receipt printing
    /// </summary>
    /// <param name="template">Receipt template</param>
    /// <returns>CSS styles string</returns>
    Task<string> GenerateReceiptCssStylesAsync(ReceiptTemplate template);
}
