using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for supplier credit management and alerts
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        ISupplierRepository supplierRepository,
        IMapper mapper,
        ILogger<SupplierService> logger)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get suppliers near credit limit
    /// </summary>
    public async Task<List<SupplierCreditAlertDto>> GetSuppliersNearCreditLimitAsync(decimal warningPercentage = 0.8m)
    {
        try
        {
            _logger.LogInformation("Getting suppliers near credit limit. Warning percentage: {WarningPercentage}%",
                warningPercentage * 100);

            var suppliers = await _supplierRepository.GetSuppliersNearCreditLimitAsync(warningPercentage);
            var alerts = new List<SupplierCreditAlertDto>();

            foreach (var supplier in suppliers)
            {
                var utilizationPercentage = supplier.CreditLimit > 0
                    ? (supplier.CurrentBalance / supplier.CreditLimit) * 100
                    : 0;

                var availableCredit = supplier.CreditLimit - supplier.CurrentBalance;

                var alert = new SupplierCreditAlertDto
                {
                    SupplierId = supplier.Id,
                    SupplierName = supplier.CompanyName,
                    CompanyName = supplier.CompanyName,
                    CreditLimit = supplier.CreditLimit,
                    CurrentBalance = supplier.CurrentBalance,
                    AvailableCredit = availableCredit,
                    CreditUtilizationPercentage = utilizationPercentage,
                    AlertType = "near_limit",
                    Severity = utilizationPercentage >= 95 ? "high" : "medium",
                    Message = $"Credit utilization at {utilizationPercentage:F1}%. Available credit: {availableCredit:C}",
                    ContactPersonName = supplier.ContactPersonName,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    LastTransactionDate = supplier.LastTransactionDate,
                    CreatedAt = supplier.CreatedAt
                };

                alerts.Add(alert);
            }

            _logger.LogInformation("Found {Count} suppliers near credit limit", alerts.Count);
            return alerts.OrderByDescending(a => a.CreditUtilizationPercentage).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suppliers near credit limit");
            throw;
        }
    }

    /// <summary>
    /// Get suppliers over credit limit
    /// </summary>
    public async Task<List<SupplierCreditAlertDto>> GetSuppliersOverCreditLimitAsync()
    {
        try
        {
            _logger.LogInformation("Getting suppliers over credit limit");

            var suppliers = await _supplierRepository.GetSuppliersOverCreditLimitAsync();
            var alerts = new List<SupplierCreditAlertDto>();

            foreach (var supplier in suppliers)
            {
                var utilizationPercentage = supplier.CreditLimit > 0
                    ? (supplier.CurrentBalance / supplier.CreditLimit) * 100
                    : 0;

                var overLimitAmount = supplier.CurrentBalance - supplier.CreditLimit;

                var alert = new SupplierCreditAlertDto
                {
                    SupplierId = supplier.Id,
                    SupplierName = supplier.CompanyName,
                    CompanyName = supplier.CompanyName,
                    CreditLimit = supplier.CreditLimit,
                    CurrentBalance = supplier.CurrentBalance,
                    AvailableCredit = 0,
                    CreditUtilizationPercentage = utilizationPercentage,
                    AlertType = "over_limit",
                    Severity = "critical",
                    Message = $"Over credit limit by {overLimitAmount:C}. Current balance: {supplier.CurrentBalance:C}, Limit: {supplier.CreditLimit:C}",
                    ContactPersonName = supplier.ContactPersonName,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    LastTransactionDate = supplier.LastTransactionDate,
                    CreatedAt = supplier.CreatedAt
                };

                alerts.Add(alert);
            }

            _logger.LogInformation("Found {Count} suppliers over credit limit", alerts.Count);
            return alerts.OrderByDescending(a => a.CurrentBalance - a.CreditLimit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suppliers over credit limit");
            throw;
        }
    }

    /// <summary>
    /// Get all supplier credit alerts
    /// </summary>
    public async Task<List<SupplierCreditAlertDto>> GetAllSupplierCreditAlertsAsync(decimal warningPercentage = 0.8m)
    {
        try
        {
            _logger.LogInformation("Getting all supplier credit alerts");

            var nearLimitAlerts = await GetSuppliersNearCreditLimitAsync(warningPercentage);
            var overLimitAlerts = await GetSuppliersOverCreditLimitAsync();

            var allAlerts = nearLimitAlerts.Concat(overLimitAlerts).ToList();

            _logger.LogInformation("Found {Count} total supplier credit alerts", allAlerts.Count);
            return allAlerts.OrderByDescending(a => a.Severity == "critical" ? 4 :
                                                  a.Severity == "high" ? 3 :
                                                  a.Severity == "medium" ? 2 : 1)
                           .ThenByDescending(a => a.CreditUtilizationPercentage)
                           .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all supplier credit alerts");
            throw;
        }
    }

    /// <summary>
    /// Check if supplier can make additional purchases
    /// </summary>
    public async Task<SupplierCreditValidationResult> ValidateSupplierCreditAsync(int supplierId, decimal additionalAmount)
    {
        try
        {
            _logger.LogInformation("Validating supplier credit. SupplierId: {SupplierId}, AdditionalAmount: {AdditionalAmount}",
                supplierId, additionalAmount);

            var supplier = await _supplierRepository.GetByIdAsync(supplierId);
            if (supplier == null)
            {
                return new SupplierCreditValidationResult
                {
                    CanPurchase = false,
                    Message = "Supplier not found",
                    RequestedAmount = additionalAmount,
                    Warnings = new List<string> { "Invalid supplier ID" }
                };
            }

            if (!supplier.IsActive)
            {
                return new SupplierCreditValidationResult
                {
                    CanPurchase = false,
                    Message = "Supplier account is inactive",
                    CurrentBalance = supplier.CurrentBalance,
                    CreditLimit = supplier.CreditLimit,
                    RequestedAmount = additionalAmount,
                    CreditLimitEnforced = supplier.CreditLimitEnforced,
                    Warnings = new List<string> { "Supplier account is inactive" }
                };
            }

            var availableCredit = supplier.CreditLimit - supplier.CurrentBalance;
            var newBalance = supplier.CurrentBalance + additionalAmount;
            var warnings = new List<string>();

            // Check if credit limit is enforced
            if (!supplier.CreditLimitEnforced)
            {
                _logger.LogInformation("Credit limit not enforced for supplier {SupplierId}", supplierId);
                return new SupplierCreditValidationResult
                {
                    CanPurchase = true,
                    Message = "Credit limit not enforced for this supplier",
                    AvailableCredit = availableCredit,
                    RequestedAmount = additionalAmount,
                    CurrentBalance = supplier.CurrentBalance,
                    CreditLimit = supplier.CreditLimit,
                    CreditLimitEnforced = false,
                    Warnings = warnings
                };
            }

            // Check if supplier would exceed credit limit
            if (newBalance > supplier.CreditLimit)
            {
                var overLimitAmount = newBalance - supplier.CreditLimit;
                warnings.Add($"Would exceed credit limit by {overLimitAmount:C}");

                return new SupplierCreditValidationResult
                {
                    CanPurchase = false,
                    Message = $"Purchase would exceed credit limit. Available credit: {availableCredit:C}, Requested: {additionalAmount:C}",
                    AvailableCredit = availableCredit,
                    RequestedAmount = additionalAmount,
                    CurrentBalance = supplier.CurrentBalance,
                    CreditLimit = supplier.CreditLimit,
                    CreditLimitEnforced = true,
                    Warnings = warnings
                };
            }

            // Check for warnings (near limit)
            if (supplier.CreditLimit > 0)
            {
                var utilizationAfterPurchase = (newBalance / supplier.CreditLimit) * 100;

                if (utilizationAfterPurchase >= 90)
                {
                    warnings.Add($"Credit utilization would reach {utilizationAfterPurchase:F1}% after this purchase");
                }
                else if (utilizationAfterPurchase >= 80)
                {
                    warnings.Add($"Credit utilization would reach {utilizationAfterPurchase:F1}% after this purchase");
                }
            }

            _logger.LogInformation("Supplier credit validation successful. SupplierId: {SupplierId}", supplierId);

            return new SupplierCreditValidationResult
            {
                CanPurchase = true,
                Message = "Credit validation successful",
                AvailableCredit = availableCredit,
                RequestedAmount = additionalAmount,
                CurrentBalance = supplier.CurrentBalance,
                CreditLimit = supplier.CreditLimit,
                CreditLimitEnforced = true,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating supplier credit. SupplierId: {SupplierId}", supplierId);
            throw;
        }
    }
}
