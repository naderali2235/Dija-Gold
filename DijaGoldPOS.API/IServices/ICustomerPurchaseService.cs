using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Shared;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Service interface for customer purchase operations
/// </summary>
public interface ICustomerPurchaseService
{
    /// <summary>
    /// Create a new customer purchase
    /// </summary>
    Task<ApiResponse<CustomerPurchaseDto>> CreatePurchaseAsync(CreateCustomerPurchaseRequest request, string userId);

    /// <summary>
    /// Get customer purchase by ID with items
    /// </summary>
    Task<ApiResponse<CustomerPurchaseDto>> GetPurchaseByIdAsync(int id);

    /// <summary>
    /// Get customer purchase by purchase number
    /// </summary>
    Task<ApiResponse<CustomerPurchaseDto>> GetPurchaseByNumberAsync(string purchaseNumber);

    /// <summary>
    /// Get customer purchases with pagination and filtering
    /// </summary>
    Task<ApiResponse<PagedResult<CustomerPurchaseDto>>> GetPurchasesAsync(CustomerPurchaseSearchRequest searchRequest);

    /// <summary>
    /// Get customer purchases by customer ID
    /// </summary>
    Task<ApiResponse<List<CustomerPurchaseDto>>> GetPurchasesByCustomerAsync(int customerId);

    /// <summary>
    /// Get customer purchases by branch ID
    /// </summary>
    Task<ApiResponse<List<CustomerPurchaseDto>>> GetPurchasesByBranchAsync(int branchId);

    /// <summary>
    /// Get customer purchases within date range
    /// </summary>
    Task<ApiResponse<List<CustomerPurchaseDto>>> GetPurchasesByDateRangeAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Update customer purchase payment status
    /// </summary>
    Task<ApiResponse<CustomerPurchaseDto>> UpdatePaymentStatusAsync(int purchaseId, decimal amountPaid, string userId);

    /// <summary>
    /// Cancel customer purchase
    /// </summary>
    Task<ApiResponse<bool>> CancelPurchaseAsync(int purchaseId, string userId);

    /// <summary>
    /// Get customer purchase summary for a date range
    /// </summary>
    Task<ApiResponse<CustomerPurchaseSummaryDto>> GetPurchaseSummaryAsync(DateTime fromDate, DateTime toDate, int? branchId = null);
}

/// <summary>
/// Customer purchase summary DTO
/// </summary>
public class CustomerPurchaseSummaryDto
{
    public int TotalPurchases { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
