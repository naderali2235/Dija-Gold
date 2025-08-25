namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Process purchase order delivery request DTO
/// </summary>
public class ProcessPurchaseOrderDeliveryRequestDto
{
    public int PurchaseOrderId { get; set; }
    public DateTime ActualDeliveryDate { get; set; }
    public List<PurchaseOrderItemDeliveryDto> Items { get; set; } = new List<PurchaseOrderItemDeliveryDto>();
    public string? Notes { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
}

/// <summary>
/// Purchase order item delivery DTO
/// </summary>
public class PurchaseOrderItemDeliveryDto
{
    public int PurchaseOrderItemId { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal WeightReceived { get; set; }
    public string? Notes { get; set; }
}


