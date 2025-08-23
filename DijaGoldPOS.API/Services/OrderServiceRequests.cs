using DijaGoldPOS.API.Models;

using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Request for creating an order
/// </summary>
public class CreateOrderRequest
{
    public int BranchId { get; set; }
    public int OrderTypeId { get; set; } // Changed from OrderType to OrderTypeId
    public int? CustomerId { get; set; }
    public int? GoldRateId { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Request for creating an order item
/// </summary>
public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? CustomDiscountPercentage { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request for updating an order
/// </summary>
public class UpdateOrderRequest
{
    public int StatusId { get; set; } // Changed from OrderStatus? to int
    public string? Notes { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public string? ReturnReason { get; set; }
}

/// <summary>
/// Request for processing order payment
/// </summary>
public class ProcessOrderPaymentRequest
{
    public decimal AmountPaid { get; set; }
    public int PaymentMethodId { get; set; } // Changed from PaymentMethod to PaymentMethodId
    public string? Notes { get; set; }
}

/// <summary>
/// Request for creating a return order
/// </summary>
public class CreateReturnOrderRequest
{
    public string ReturnReason { get; set; } = string.Empty;
    public List<ReturnOrderItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}

/// <summary>
/// Request for return order item
/// </summary>
public class ReturnOrderItemRequest
{
    public int OriginalOrderItemId { get; set; }
    public decimal QuantityToReturn { get; set; }
    public string? ReturnReason { get; set; }
}

/// <summary>
/// Request for creating an exchange order
/// </summary>
public class CreateExchangeOrderRequest
{
    public string ExchangeReason { get; set; } = string.Empty;
    public List<ExchangeOrderItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
}

/// <summary>
/// Request for exchange order item
/// </summary>
public class ExchangeOrderItemRequest
{
    public int OriginalOrderItemId { get; set; }
    public decimal QuantityToExchange { get; set; }
    public int NewProductId { get; set; }
    public decimal NewQuantity { get; set; }
    public string? ExchangeReason { get; set; }
}

/// <summary>
/// Request for searching orders
/// </summary>
public class OrderSearchRequest
{
    public int? BranchId { get; set; }
    public int? OrderTypeId { get; set; } // Changed from OrderType to OrderTypeId
    public int? StatusId { get; set; } // Changed from OrderStatus to StatusId
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? OrderNumber { get; set; }
    public int? CustomerId { get; set; }
    public string? CashierId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request for canceling an order
/// </summary>
public class CancelOrderRequest
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ManagerId { get; set; } = string.Empty;
}

/// <summary>
/// Request for updating order status
/// </summary>
public class UpdateOrderStatusRequest
{
    public int OrderId { get; set; }
    public int StatusId { get; set; } // Changed from OrderStatus to int
}

/// <summary>
/// Result of order operations
/// </summary>
public class OrderResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Order? Order { get; set; }
}

/// <summary>
/// Result of order payment operations
/// </summary>
public class OrderPaymentResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Order? Order { get; set; }
    public FinancialTransaction? FinancialTransaction { get; set; }
}

/// <summary>
/// Order summary
/// </summary>
public class OrderSummary
{
    public int TotalOrders { get; set; }
    public decimal TotalValue { get; set; }
    public Dictionary<int, int> OrderTypeCounts { get; set; } = new(); // Changed from OrderType to int
    public Dictionary<int, int> StatusCounts { get; set; } = new(); // Changed from OrderStatus to int
}

/// <summary>
/// Request for creating a repair order
/// </summary>
public class CreateRepairOrderRequest
{
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public string RepairDescription { get; set; } = string.Empty;
    public decimal RepairAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public int PaymentMethodId { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public int PriorityId { get; set; }
    public int? AssignedTechnicianId { get; set; }
    public string? TechnicianNotes { get; set; }
}

/// <summary>
/// Result of repair order operations
/// </summary>
public class RepairOrderResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Order? Order { get; set; }
    public FinancialTransaction? FinancialTransaction { get; set; }
    public RepairJobDto? RepairJob { get; set; }
}