using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.Services;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequestDto request, string userId);
    Task<PurchaseOrderDto?> GetAsync(int id);
    Task<(List<PurchaseOrderDto> Items, int TotalCount)> SearchAsync(PurchaseOrderSearchRequestDto request);
    Task<bool> ReceiveAsync(ReceivePurchaseOrderRequestDto request, string userId);
}


