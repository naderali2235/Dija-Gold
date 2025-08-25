using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service interface for supplier credit management and alerts
/// </summary>
public interface ISupplierService
{
    /// <summary>
    /// Get suppliers near credit limit
    /// </summary>
    Task<List<SupplierCreditAlertDto>> GetSuppliersNearCreditLimitAsync(decimal warningPercentage = 0.8m);

    /// <summary>
    /// Get suppliers over credit limit
    /// </summary>
    Task<List<SupplierCreditAlertDto>> GetSuppliersOverCreditLimitAsync();

    /// <summary>
    /// Get all supplier credit alerts
    /// </summary>
    Task<List<SupplierCreditAlertDto>> GetAllSupplierCreditAlertsAsync(decimal warningPercentage = 0.8m);

    /// <summary>
    /// Check if supplier can make additional purchases
    /// </summary>
    Task<SupplierCreditValidationResult> ValidateSupplierCreditAsync(int supplierId, decimal additionalAmount);
}
