namespace DijaGoldPOS.API.DTOs;

public class PurchaseOrderDto
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
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
    public List<string> AvailableStatuses { get; set; } = new(); // Available status transitions
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal WeightOrdered { get; set; }
    public decimal WeightReceived { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsReceived { get; set; } // Helper property for frontend
    public bool CanEdit { get; set; } // Helper property to determine if item can be edited
}

public class CreatePurchaseOrderRequestDto
{

    public int SupplierId { get; set; }


    public int BranchId { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }


    public string? Terms { get; set; }


    public string? Notes { get; set; }


    public List<CreatePurchaseOrderItemRequestDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemRequestDto
{
    public int ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal WeightOrdered { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePurchaseOrderRequestDto
{

    public int SupplierId { get; set; }


    public int BranchId { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }


    public string? Terms { get; set; }


    public string? Notes { get; set; }


    public List<UpdatePurchaseOrderItemRequestDto> Items { get; set; } = new();
}

public class UpdatePurchaseOrderItemRequestDto
{
    public int? Id { get; set; } // null for new items, existing ID for updates


    public int ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }


    public decimal WeightOrdered { get; set; }


    public decimal UnitCost { get; set; }


    public string? Notes { get; set; }
}

public class ReceivePurchaseOrderRequestDto
{

    public int PurchaseOrderId { get; set; }


    public List<ReceivePurchaseOrderItemDto> Items { get; set; } = new();
}

public class ReceivePurchaseOrderItemDto
{

    public int PurchaseOrderItemId { get; set; }


    public decimal QuantityReceived { get; set; }


    public decimal WeightReceived { get; set; }
}

public class PurchaseOrderSearchRequestDto
{
    public string? PurchaseOrderNumber { get; set; }
    public int? SupplierId { get; set; }
    public int? BranchId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class UpdatePurchaseOrderStatusRequestDto
{

    public int PurchaseOrderId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    

    public string? StatusNotes { get; set; }
}

public class PurchaseOrderStatusTransitionDto
{
    public string CurrentStatus { get; set; } = string.Empty;
    public List<string> AvailableTransitions { get; set; } = new();
    public string? ValidationMessage { get; set; }
}

public class PurchaseOrderStatusTransitionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public PurchaseOrderDto? PurchaseOrder { get; set; }
}

public class ProcessPurchaseOrderPaymentRequestDto
{
    public int PurchaseOrderId { get; set; }
    public decimal PaymentAmount { get; set; }
    public int PaymentMethodId { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
    public int? ProcessedByUserId { get; set; }
}

public class PurchaseOrderPaymentResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public PurchaseOrderDto? PurchaseOrder { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
}
