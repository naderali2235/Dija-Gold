namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// DTO for weighted average cost calculation result
/// </summary>
public class WeightedAverageCostResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal WeightedAverageCostPerGram { get; set; }
    public decimal WeightedAverageCostPerUnit { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalCost { get; set; }
    public List<CostSourceDto> CostSources { get; set; } = new();
}

/// <summary>
/// DTO for cost source information
/// </summary>
public class CostSourceDto
{
    public int SourceId { get; set; }
    public string SourceType { get; set; } = string.Empty; // ProductOwnership, RawMaterial
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Weight { get; set; }
    public decimal TotalCost { get; set; }
    public decimal CostPerGram { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal ContributionPercentage { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int SequenceOrder { get; set; }
    public string? KaratType { get; set; }
}

/// <summary>
/// DTO for product cost analysis
/// </summary>
public class ProductCostAnalysisDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal CurrentCostPrice { get; set; }
    public WeightedAverageCostResultDto WeightedAverageCost { get; set; } = new();
    public FifoCostResultDto FifoCost { get; set; } = new();
    public LifoCostResultDto LifoCost { get; set; } = new();
    public string RecommendedCostingMethod { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
}

/// <summary>
/// DTO for FIFO cost calculation result
/// </summary>
public class FifoCostResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal CostPerGram { get; set; }
    public List<CostSourceDto> UsedSources { get; set; } = new();
}

/// <summary>
/// DTO for LIFO cost calculation result
/// </summary>
public class LifoCostResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal CostPerGram { get; set; }
    public List<CostSourceDto> UsedSources { get; set; } = new();
}
