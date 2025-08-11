using System.ComponentModel.DataAnnotations;

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
}

public class CreatePurchaseOrderRequestDto
{
    [Required]
    public int SupplierId { get; set; }

    [Required]
    public int BranchId { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    [MaxLength(1000)]
    public string? Terms { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MinLength(1)]
    public List<CreatePurchaseOrderItemRequestDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemRequestDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Range(0.001, double.MaxValue)]
    public decimal QuantityOrdered { get; set; }

    [Range(0.001, double.MaxValue)]
    public decimal WeightOrdered { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitCost { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class ReceivePurchaseOrderRequestDto
{
    [Required]
    public int PurchaseOrderId { get; set; }

    [MinLength(1)]
    public List<ReceivePurchaseOrderItemDto> Items { get; set; } = new();
}

public class ReceivePurchaseOrderItemDto
{
    [Required]
    public int PurchaseOrderItemId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal QuantityReceived { get; set; }

    [Range(0, double.MaxValue)]
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


