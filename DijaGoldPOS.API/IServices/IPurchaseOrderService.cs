using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.IServices;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequestDto request, string userId);
    Task<PurchaseOrderDto?> GetAsync(int id);
    Task<(List<PurchaseOrderDto> Items, int TotalCount)> SearchAsync(PurchaseOrderSearchRequestDto request);
    Task<bool> ReceiveAsync(ReceivePurchaseOrderRequestDto request, string userId);
    Task<PurchaseOrderDto> UpdateAsync(int id, UpdatePurchaseOrderRequestDto request, string userId);
    Task<PurchaseOrderDto> UpdateStatusAsync(int id, UpdatePurchaseOrderStatusRequestDto request, string userId);
    Task<PurchaseOrderStatusTransitionDto> GetAvailableStatusTransitionsAsync(int id);
    Task<PurchaseOrderPaymentResult> ProcessPaymentAsync(ProcessPurchaseOrderPaymentRequestDto request, string userId);
    Task<List<PurchaseOrderDto>> GetOverduePurchaseOrdersAsync(int? branchId = null);
    Task<decimal> GetTotalOutstandingAmountAsync(int? supplierId = null, int? branchId = null);
}


