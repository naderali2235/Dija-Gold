namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Raw gold purchase order DTO for list/display operations
/// </summary>
public class RawGoldPurchaseOrderDto
{
    public int Id { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? Terms { get; set; }
    public string? Notes { get; set; }
    public decimal TotalWeightOrdered { get; set; }
    public decimal TotalWeightReceived { get; set; }
    public List<string> AvailableStatuses { get; set; } = new();
    public List<RawGoldPurchaseOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Raw gold purchase order item DTO
/// </summary>
public class RawGoldPurchaseOrderItemDto
{
    public int Id { get; set; }
    public int KaratTypeId { get; set; }
    public string? KaratTypeName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal WeightOrdered { get; set; }
    public decimal WeightReceived { get; set; }
    public decimal WeightConsumedInManufacturing { get; set; }
    public decimal AvailableWeightForManufacturing { get; set; }
    public decimal UnitCostPerGram { get; set; }
    public decimal LineTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? PurityPercentage { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public bool IsReceived { get; set; }
    public bool CanEdit { get; set; }
}

/// <summary>
/// Create raw gold purchase order request DTO
/// </summary>
public class CreateRawGoldPurchaseOrderRequestDto
{
    public int SupplierId { get; set; }
    public int BranchId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Terms { get; set; }
    public string? Notes { get; set; }
    public List<CreateRawGoldPurchaseOrderItemRequestDto> Items { get; set; } = new();
}

/// <summary>
/// Create raw gold purchase order item request DTO
/// </summary>
public class CreateRawGoldPurchaseOrderItemRequestDto
{
    public int KaratTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal WeightOrdered { get; set; }
    public decimal UnitCostPerGram { get; set; }
    public decimal? PurityPercentage { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Update raw gold purchase order request DTO
/// </summary>
public class UpdateRawGoldPurchaseOrderRequestDto
{
    public int SupplierId { get; set; }
    public int BranchId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Terms { get; set; }
    public string? Notes { get; set; }
    public List<UpdateRawGoldPurchaseOrderItemRequestDto> Items { get; set; } = new();
}

/// <summary>
/// Update raw gold purchase order item request DTO
/// </summary>
public class UpdateRawGoldPurchaseOrderItemRequestDto
{
    public int? Id { get; set; }
    public int KaratTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal WeightOrdered { get; set; }
    public decimal UnitCostPerGram { get; set; }
    public decimal? PurityPercentage { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Receive raw gold purchase order request DTO
/// </summary>
public class ReceiveRawGoldPurchaseOrderRequestDto
{
    public int RawGoldPurchaseOrderId { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public List<ReceiveRawGoldPurchaseOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Receive raw gold purchase order item DTO
/// </summary>
public class ReceiveRawGoldPurchaseOrderItemDto
{
    public int RawGoldPurchaseOrderItemId { get; set; }
    public decimal WeightReceived { get; set; }
    public decimal? ActualPurityPercentage { get; set; }
    public string? ActualCertificateNumber { get; set; }
    public string? ReceivingNotes { get; set; }
}

/// <summary>
/// Raw gold purchase order search request DTO
/// </summary>
public class RawGoldPurchaseOrderSearchRequestDto
{
    public string? PurchaseOrderNumber { get; set; }
    public int? SupplierId { get; set; }
    public int? BranchId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? KaratTypeId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Raw gold inventory DTO
/// </summary>
public class RawGoldInventoryDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int KaratTypeId { get; set; }
    public string? KaratTypeName { get; set; }
    public decimal WeightOnHand { get; set; }
    public decimal WeightReserved { get; set; }
    public decimal AvailableWeight { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public decimal MaximumStockLevel { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal AverageCostPerGram { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? LastCountDate { get; set; }
    public DateTime? LastMovementDate { get; set; }
    public string? Notes { get; set; }
    public bool IsLowStock { get; set; }
}

/// <summary>
/// Raw gold inventory movement DTO
/// </summary>
public class RawGoldInventoryMovementDto
{
    public int Id { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal WeightChange { get; set; }
    public decimal WeightBalance { get; set; }
    public DateTime MovementDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO aliases for controller compatibility
/// </summary>
public class CreateRawGoldPurchaseOrderDto : CreateRawGoldPurchaseOrderRequestDto { }
public class UpdateRawGoldPurchaseOrderDto : UpdateRawGoldPurchaseOrderRequestDto { }
public class ReceiveRawGoldDto : ReceiveRawGoldPurchaseOrderRequestDto { }
public class CreateRawGoldPurchaseOrderItemDto : CreateRawGoldPurchaseOrderItemRequestDto { }
public class UpdateRawGoldPurchaseOrderItemDto : UpdateRawGoldPurchaseOrderItemRequestDto { }
public class ReceiveRawGoldItemDto : ReceiveRawGoldPurchaseOrderItemDto { }

/// <summary>
/// Process payment request for raw gold purchase order
/// </summary>
public class ProcessRawGoldPurchaseOrderPaymentRequestDto
{
    public int RawGoldPurchaseOrderId { get; set; }
    public decimal PaymentAmount { get; set; }
    public int PaymentMethodId { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// Result of processing payment for raw gold purchase order
/// </summary>
public class RawGoldPurchaseOrderPaymentResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public RawGoldPurchaseOrderDto? PurchaseOrder { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
}

/// <summary>
/// Update raw gold purchase order status request DTO
/// </summary>
public class UpdateRawGoldPurchaseOrderStatusRequestDto
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
