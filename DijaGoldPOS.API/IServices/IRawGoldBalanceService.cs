using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Shared;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Service for managing raw gold balances and supplier relationships
/// </summary>
public interface IRawGoldBalanceService
{
    /// <summary>
    /// Get supplier gold balances for a branch
    /// </summary>
    Task<ApiResponse<List<SupplierGoldBalanceDto>>> GetSupplierBalancesAsync(int branchId, int? supplierId = null);
    
    /// <summary>
    /// Waive customer-purchased gold to a supplier to reduce debt
    /// </summary>
    Task<ApiResponse<RawGoldTransferDto>> WaiveGoldToSupplierAsync(WaiveGoldToSupplierRequest request, string userId);
    
    /// <summary>
    /// Convert gold from one karat type to another
    /// </summary>
    Task<ApiResponse<RawGoldTransferDto>> ConvertGoldKaratAsync(ConvertGoldKaratRequest request, string userId);
    
    /// <summary>
    /// Get merchant's own raw gold balance (gold bought from customers not yet assigned to suppliers)
    /// </summary>
    Task<ApiResponse<List<MerchantRawGoldBalanceDto>>> GetMerchantRawGoldBalanceAsync(int branchId);
    
    /// <summary>
    /// Process customer purchase and add to raw gold inventory
    /// </summary>
    Task<ApiResponse<CustomerPurchaseDto>> ProcessCustomerPurchaseAsync(CreateCustomerPurchaseRequest request, string userId);
    
    /// <summary>
    /// Get gold transfer history
    /// </summary>
    Task<ApiResponse<PagedResult<RawGoldTransferDto>>> GetTransferHistoryAsync(GoldTransferSearchRequest searchRequest);
    
    /// <summary>
    /// Get comprehensive gold balance summary for a branch
    /// </summary>
    Task<ApiResponse<GoldBalanceSummaryDto>> GetGoldBalanceSummaryAsync(int branchId);
    
    /// <summary>
    /// Calculate karat conversion preview
    /// </summary>
    Task<ApiResponse<KaratConversionDto>> CalculateKaratConversionAsync(int fromKaratTypeId, int toKaratTypeId, decimal fromWeight);
    
    /// <summary>
    /// Get available merchant gold for waiving (by karat type)
    /// </summary>
    Task<ApiResponse<List<MerchantRawGoldBalanceDto>>> GetAvailableGoldForWaivingAsync(int branchId, int? karatTypeId = null);
}
