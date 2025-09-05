using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.IServices
{

    /// <summary>
    /// Service for managing raw gold purchase orders with specialized business logic
    /// </summary>
    public interface IRawGoldPurchaseOrderService
    {
        Task<IEnumerable<RawGoldPurchaseOrderDto>> GetAllAsync();
        Task<RawGoldPurchaseOrderDto?> GetByIdAsync(int id);
        Task<RawGoldPurchaseOrderDto> CreateAsync(CreateRawGoldPurchaseOrderDto createDto);
        Task<RawGoldPurchaseOrderDto> UpdateAsync(int id, UpdateRawGoldPurchaseOrderDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<RawGoldPurchaseOrderDto> ReceiveRawGoldAsync(int id, ReceiveRawGoldDto receiveDto);
        Task<IEnumerable<RawGoldInventoryDto>> GetRawGoldInventoryAsync(int? branchId = null);
        Task<RawGoldInventoryDto?> GetRawGoldInventoryByKaratAsync(int karatTypeId, int branchId);
        Task<RawGoldPurchaseOrderPaymentResult> ProcessPaymentAsync(ProcessRawGoldPurchaseOrderPaymentRequestDto request);
        Task<RawGoldPurchaseOrderDto> UpdateStatusAsync(int id, string newStatus, string? statusNotes = null);
    }
}
