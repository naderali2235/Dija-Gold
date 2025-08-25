

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Order DTO
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int OrderTypeId { get; set; }
    public string OrderTypeDescription { get; set; } = string.Empty;
    public OrderTypeLookupDto? OrderType { get; set; }
    public DateTime OrderDate { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string CashierId { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public string? ApprovedByName { get; set; }
    public int StatusId { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public OrderStatusLookupDto? Status { get; set; }
    public int? OriginalOrderId { get; set; }
    public string? ReturnReason { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public int? GoldRateId { get; set; }
    public decimal? GoldRate { get; set; }
    public decimal? GoldRatePerGram { get; set; }
    public string? OriginalOrderNumber { get; set; }
    public string? FinancialTransactionNumber { get; set; }
    public int? FinancialTransactionId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Order item DTO
/// </summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal MakingCharges { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? KaratType { get; set; }
    public KaratTypeLookupDto? KaratTypeLookup { get; set; }
    public decimal? Weight { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Create order request DTO
/// </summary>
public class CreateOrderRequestDto
{

    public int BranchId { get; set; }
    

    public int OrderTypeId { get; set; }
    
    public int? CustomerId { get; set; }
    
    public int? GoldRateId { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime? EstimatedCompletionDate { get; set; }
    

    public List<CreateOrderItemRequestDto> Items { get; set; } = new();
}

/// <summary>
/// Create order item request DTO
/// </summary>
public class CreateOrderItemRequestDto
{

    public int ProductId { get; set; }
    


    public decimal Quantity { get; set; }
    
    public decimal? CustomDiscountPercentage { get; set; }
    
    public string? Notes { get; set; }
}

/// <summary>
/// Update order request DTO
/// </summary>
public class UpdateOrderRequestDto
{
    public int StatusId { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public string? ReturnReason { get; set; }
}

/// <summary>
/// Order search request DTO
/// </summary>
public class OrderSearchRequestDto
{
    public int? BranchId { get; set; }
    public int? OrderTypeId { get; set; }
    public int? StatusId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? OrderNumber { get; set; }
    public int? CustomerId { get; set; }
    public string? CashierId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Order summary DTO
/// </summary>
public class OrderSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalValue { get; set; }
    public Dictionary<int, int> OrderTypeCounts { get; set; } = new();
    public Dictionary<int, int> StatusCounts { get; set; } = new();
}

/// <summary>
/// Process order payment request DTO
/// </summary>
public class ProcessOrderPaymentRequestDto
{

    public int OrderId { get; set; }
    

    public decimal AmountPaid { get; set; }
    

    public int PaymentMethodId { get; set; }
    
    public string? Notes { get; set; }
}

/// <summary>
/// Create repair order request DTO
/// </summary>
public class CreateRepairOrderRequestDto
{

    public int BranchId { get; set; }
    
    public int? CustomerId { get; set; }
    


    public string RepairDescription { get; set; } = string.Empty;
    


    public decimal RepairAmount { get; set; }
    


    public decimal AmountPaid { get; set; }
    

    public int PaymentMethodId { get; set; }
    
    public DateTime? EstimatedCompletionDate { get; set; }
    
    public int PriorityId { get; set; } = 2; // Default to Medium
    
    public int? AssignedTechnicianId { get; set; }
    

    public string? TechnicianNotes { get; set; }
}

/// <summary>
/// Repair order result DTO
/// </summary>
public class RepairOrderResultDto
{
    public OrderDto? Order { get; set; }
    public FinancialTransactionDto? FinancialTransaction { get; set; }
    public RepairJobDto? RepairJob { get; set; }
}
