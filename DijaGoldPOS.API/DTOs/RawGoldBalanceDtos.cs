using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// DTO for supplier gold balance information
/// </summary>
public class SupplierGoldBalanceDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int KaratTypeId { get; set; }
    public string KaratTypeName { get; set; } = string.Empty;
    public decimal KaratPurity { get; set; }
    
    public decimal TotalWeightReceived { get; set; }
    public decimal TotalWeightPaidFor { get; set; }
    public decimal OutstandingWeightDebt { get; set; }
    public decimal MerchantGoldBalance { get; set; }
    public decimal OutstandingMonetaryValue { get; set; }
    public decimal AverageCostPerGram { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

/// <summary>
/// DTO for merchant's own raw gold balance
/// </summary>
public class MerchantRawGoldBalanceDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int KaratTypeId { get; set; }
    public string KaratTypeName { get; set; } = string.Empty;
    public decimal KaratPurity { get; set; }
    public decimal AvailableWeight { get; set; }
    public decimal AverageCostPerGram { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? LastMovementDate { get; set; }
}

/// <summary>
/// Request DTO for waiving gold to supplier
/// </summary>
public class WaiveGoldToSupplierRequest
{
    [Required]
    public int BranchId { get; set; }
    
    [Required]
    public int ToSupplierId { get; set; }
    
    [Required]
    public int FromKaratTypeId { get; set; }
    
    [Required]
    public int ToKaratTypeId { get; set; }
    
    [Required]
    [Range(0.001, 10000, ErrorMessage = "Weight must be between 0.001 and 10,000 grams")]
    public decimal FromWeight { get; set; }
    
    public int? CustomerPurchaseId { get; set; }
    
    public string? Notes { get; set; }
}

/// <summary>
/// Request DTO for converting gold karat
/// </summary>
public class ConvertGoldKaratRequest
{
    [Required]
    public int BranchId { get; set; }
    
    public int? SupplierId { get; set; } // null for merchant's own gold
    
    [Required]
    public int FromKaratTypeId { get; set; }
    
    [Required]
    public int ToKaratTypeId { get; set; }
    
    [Required]
    [Range(0.001, 10000, ErrorMessage = "Weight must be between 0.001 and 10,000 grams")]
    public decimal FromWeight { get; set; }
    
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for raw gold transfer information
/// </summary>
public class RawGoldTransferDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    
    public int? FromSupplierId { get; set; }
    public string? FromSupplierName { get; set; }
    
    public int? ToSupplierId { get; set; }
    public string? ToSupplierName { get; set; }
    
    public int FromKaratTypeId { get; set; }
    public string FromKaratTypeName { get; set; } = string.Empty;
    
    public int ToKaratTypeId { get; set; }
    public string ToKaratTypeName { get; set; } = string.Empty;
    
    public decimal FromWeight { get; set; }
    public decimal ToWeight { get; set; }
    public decimal FromGoldRate { get; set; }
    public decimal ToGoldRate { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal TransferValue { get; set; }
    
    public DateTime TransferDate { get; set; }
    public string TransferType { get; set; } = string.Empty;
    
    public int? CustomerPurchaseId { get; set; }
    public string? CustomerPurchaseNumber { get; set; }
    
    public string? Notes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for gold balance summary across all suppliers
/// </summary>
public class GoldBalanceSummaryDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SupplierGoldBalanceDto> SupplierBalances { get; set; } = new();
    public List<MerchantRawGoldBalanceDto> MerchantBalances { get; set; } = new();
    public decimal TotalDebtValue { get; set; }
    public decimal TotalCreditValue { get; set; }
    public decimal NetBalance { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request DTO for searching gold transfers
/// </summary>
public class GoldTransferSearchRequest
{
    public int? BranchId { get; set; }
    public int? SupplierId { get; set; }
    public int? KaratTypeId { get; set; }
    public string? TransferType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO for karat conversion calculation
/// </summary>
public class KaratConversionDto
{
    public int FromKaratTypeId { get; set; }
    public string FromKaratTypeName { get; set; } = string.Empty;
    public int ToKaratTypeId { get; set; }
    public string ToKaratTypeName { get; set; } = string.Empty;
    public decimal FromWeight { get; set; }
    public decimal ToWeight { get; set; }
    public decimal FromRate { get; set; }
    public decimal ToRate { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal TransferValue { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
