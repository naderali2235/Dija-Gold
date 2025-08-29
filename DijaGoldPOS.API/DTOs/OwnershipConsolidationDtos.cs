namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// DTO for consolidation result
/// </summary>
public class ConsolidationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ConsolidatedRecords { get; set; }
    public int? NewOwnershipId { get; set; }
    public WeightedAverageCostDto? WeightedAverageCost { get; set; }
}

/// <summary>
/// DTO for consolidation opportunity
/// </summary>
public class ConsolidationOpportunityDto
{
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalCost { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal PotentialSavings { get; set; }
}

/// <summary>
/// DTO for weighted average cost calculation
/// </summary>
public class WeightedAverageCostDto
{
    public decimal WeightedAverageCostPerGram { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalCost { get; set; }
    public int RecordCount { get; set; }
    public List<CostBreakdownDto> CostBreakdown { get; set; } = new();
}

/// <summary>
/// DTO for cost breakdown details
/// </summary>
public class CostBreakdownDto
{
    public int OwnershipId { get; set; }
    public decimal Weight { get; set; }
    public decimal Cost { get; set; }
    public decimal CostPerGram { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
}
