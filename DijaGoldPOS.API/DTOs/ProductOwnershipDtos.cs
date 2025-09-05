

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Product ownership DTO for display
/// </summary>
public class ProductOwnershipDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public int? CustomerPurchaseId { get; set; }
    public string? CustomerPurchaseNumber { get; set; }
    
    public decimal TotalQuantity { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal OwnedQuantity { get; set; }
    public decimal OwnedWeight { get; set; }
    public decimal OwnershipPercentage { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating/updating product ownership
/// </summary>
public class ProductOwnershipRequest
{

    public int ProductId { get; set; }
    

    public int BranchId { get; set; }
    
    public int? SupplierId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public int? CustomerPurchaseId { get; set; }
    

    public decimal TotalQuantity { get; set; }
    

    public decimal TotalWeight { get; set; }
    

    public decimal OwnedQuantity { get; set; }
    

    public decimal OwnedWeight { get; set; }
    

    public decimal TotalCost { get; set; }
    

    public decimal AmountPaid { get; set; }
}

/// <summary>
/// Ownership movement DTO
/// </summary>
public class OwnershipMovementDto
{
    public int Id { get; set; }
    public int ProductOwnershipId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal QuantityChange { get; set; }
    public decimal WeightChange { get; set; }
    public decimal AmountChange { get; set; }
    public decimal OwnedQuantityAfter { get; set; }
    public decimal OwnedWeightAfter { get; set; }
    public decimal AmountPaidAfter { get; set; }
    public decimal OwnershipPercentageAfter { get; set; }
    public string? Notes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating ownership movement
/// </summary>
public class CreateOwnershipMovementRequest
{

    public int ProductOwnershipId { get; set; }
    

    public string MovementType { get; set; } = string.Empty;
    
    public decimal QuantityChange { get; set; }
    public decimal WeightChange { get; set; }
    public decimal AmountChange { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Ownership validation result
/// </summary>
public class OwnershipValidationResult
{
    public bool CanSell { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal OwnedQuantity { get; set; }
    public decimal OwnedWeight { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal OwnershipPercentage { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Ownership alert DTO
/// </summary>
public class OwnershipAlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public decimal OwnershipPercentage { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}


/// <summary>
/// Request DTO for ownership validation
/// </summary>
public class ValidateOwnershipRequest
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public decimal RequestedQuantity { get; set; }
}

/// <summary>
/// Request DTO for product sale validation
/// </summary>
public class ValidateProductSaleRequest
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public decimal RequestedQuantity { get; set; }
}

/// <summary>
/// Product sale validation result - ALERTS ONLY, NO BLOCKING
/// </summary>
public class ProductSaleValidationResult
{
    public bool CanSell { get; set; } = true; // Always true - only alerts, no blocking
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Request DTO for updating ownership after payment
/// </summary>
public class UpdateOwnershipPaymentRequest
{
    public int ProductOwnershipId { get; set; }
    public decimal PaymentAmount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating ownership after sale
/// </summary>
public class UpdateOwnershipSaleRequest
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public decimal SoldQuantity { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}

