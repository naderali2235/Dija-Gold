using System.ComponentModel.DataAnnotations;

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
    [Required]
    public int BranchId { get; set; }
    
    [Required]
    public int OrderTypeId { get; set; }
    
    public int? CustomerId { get; set; }
    
    public int? GoldRateId { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime? EstimatedCompletionDate { get; set; }
    
    [Required]
    public List<CreateOrderItemRequestDto> Items { get; set; } = new();
}

/// <summary>
/// Create order item request DTO
/// </summary>
public class CreateOrderItemRequestDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
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
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public decimal AmountPaid { get; set; }
    
    [Required]
    public int PaymentMethodId { get; set; }
    
    public string? Notes { get; set; }
}

/// <summary>
/// Create repair order request DTO
/// </summary>
public class CreateRepairOrderRequestDto
{
    [Required]
    public int BranchId { get; set; }
    
    public int? CustomerId { get; set; }
    
    [Required]
    [StringLength(1000, ErrorMessage = "Repair description cannot exceed 1000 characters")]
    public string RepairDescription { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Repair amount must be greater than 0")]
    public decimal RepairAmount { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Amount paid cannot be negative")]
    public decimal AmountPaid { get; set; }
    
    [Required]
    public int PaymentMethodId { get; set; }
    
    public DateTime? EstimatedCompletionDate { get; set; }
    
    public int PriorityId { get; set; } = 2; // Default to Medium
    
    public int? AssignedTechnicianId { get; set; }
    
    [StringLength(2000, ErrorMessage = "Technician notes cannot exceed 2000 characters")]
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
