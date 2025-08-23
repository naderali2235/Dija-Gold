using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Customer purchase DTO for display
/// </summary>
public class CustomerPurchaseDto
{
    public int Id { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public int PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<CustomerPurchaseItemDto> Items { get; set; } = new();
}

/// <summary>
/// Customer purchase item DTO
/// </summary>
public class CustomerPurchaseItemDto
{
    public int Id { get; set; }
    public int CustomerPurchaseId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Weight { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Create customer purchase request DTO
/// </summary>
public class CreateCustomerPurchaseRequest
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    public int BranchId { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal AmountPaid { get; set; }
    
    [Required]
    public int PaymentMethodId { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [MinLength(1)]
    public List<CreateCustomerPurchaseItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Create customer purchase item request DTO
/// </summary>
public class CreateCustomerPurchaseItemRequest
{
    [Required]
    public int ProductId { get; set; }
    
    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
    
    [Range(0.001, double.MaxValue)]
    public decimal Weight { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Customer purchase search request DTO
/// </summary>
public class CustomerPurchaseSearchRequest
{
    public string? PurchaseNumber { get; set; }
    public int? CustomerId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
