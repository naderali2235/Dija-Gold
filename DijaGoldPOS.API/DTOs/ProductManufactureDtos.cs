

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// DTO for creating a new product manufacturing record
/// </summary>
public class CreateProductManufactureDto
{
    /// <summary>
    /// The finished product that was manufactured
    /// </summary>

    public int ProductId { get; set; }
    
    /// <summary>
    /// The purchase order item (raw gold) that was used to manufacture this product
    /// </summary>

    public int SourcePurchaseOrderItemId { get; set; }
    
    /// <summary>
    /// Weight of raw gold consumed to manufacture this product (in grams)
    /// </summary>


    public decimal ConsumedWeight { get; set; }
    
    /// <summary>
    /// Weight lost during manufacturing process (wastage) in grams
    /// </summary>

    public decimal WastageWeight { get; set; } = 0;
    
    /// <summary>
    /// Date when the product was manufactured
    /// </summary>
    public DateTime ManufactureDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Manufacturing cost per gram (includes making charges, labor, etc.)
    /// </summary>

    public decimal ManufacturingCostPerGram { get; set; }
    
    /// <summary>
    /// Total manufacturing cost for this product
    /// </summary>

    public decimal TotalManufacturingCost { get; set; }
    
    /// <summary>
    /// Manufacturing batch number for grouping related products
    /// </summary>

    public string? BatchNumber { get; set; }
    
    /// <summary>
    /// Notes about the manufacturing process
    /// </summary>

    public string? ManufacturingNotes { get; set; }
    
    /// <summary>
    /// Manufacturing status
    /// </summary>

    public string Status { get; set; } = "Completed";

    /// <summary>
    /// Branch where the manufacturing takes place
    /// </summary>

    public int BranchId { get; set; }

    /// <summary>
    /// Technician performing the manufacturing
    /// </summary>
    public int? TechnicianId { get; set; }

    /// <summary>
    /// Manufacturing priority level
    /// </summary>

    public string? Priority { get; set; }

    /// <summary>
    /// Estimated completion date
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }
}

/// <summary>
/// DTO for updating an existing product manufacturing record
/// </summary>
public class UpdateProductManufactureDto
{
    /// <summary>
    /// Weight of raw gold consumed to manufacture this product (in grams)
    /// </summary>

    public decimal? ConsumedWeight { get; set; }
    
    /// <summary>
    /// Weight lost during manufacturing process (wastage) in grams
    /// </summary>

    public decimal? WastageWeight { get; set; }
    
    /// <summary>
    /// Manufacturing cost per gram (includes making charges, labor, etc.)
    /// </summary>

    public decimal? ManufacturingCostPerGram { get; set; }
    
    /// <summary>
    /// Total manufacturing cost for this product
    /// </summary>

    public decimal? TotalManufacturingCost { get; set; }
    
    /// <summary>
    /// Manufacturing batch number for grouping related products
    /// </summary>

    public string? BatchNumber { get; set; }
    
    /// <summary>
    /// Notes about the manufacturing process
    /// </summary>

    public string? ManufacturingNotes { get; set; }
    
    /// <summary>
    /// Manufacturing status
    /// </summary>

    public string? Status { get; set; }
}

/// <summary>
/// DTO for product manufacturing record response
/// </summary>
public class ProductManufactureDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int SourcePurchaseOrderItemId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal ConsumedWeight { get; set; }
    public decimal WastageWeight { get; set; }
    public DateTime ManufactureDate { get; set; }
    public decimal ManufacturingCostPerGram { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public string? BatchNumber { get; set; }
    public string? ManufacturingNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for manufacturing summary by purchase order
/// </summary>
public class ManufacturingSummaryByPurchaseOrderDto
{
    public int PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalRawGoldWeight { get; set; }
    public decimal TotalConsumedWeight { get; set; }
    public decimal TotalWastageWeight { get; set; }
    public decimal RemainingRawGoldWeight { get; set; }
    public int TotalProductsManufactured { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public List<ProductManufactureDto> ManufacturingRecords { get; set; } = new();
}

/// <summary>
/// DTO for manufacturing summary by product
/// </summary>
public class ManufacturingSummaryByProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal TotalConsumedWeight { get; set; }
    public decimal TotalWastageWeight { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public int TotalManufacturingRecords { get; set; }
    public List<ProductManufactureDto> ManufacturingRecords { get; set; } = new();
}

/// <summary>
/// DTO for workflow transitions
/// </summary>
public class WorkflowTransitionDto
{
    /// <summary>
    /// Target status for the transition
    /// </summary>


    public string TargetStatus { get; set; } = string.Empty;

    /// <summary>
    /// Notes about the transition
    /// </summary>

    public string? Notes { get; set; }
}

/// <summary>
/// DTO for quality check
/// </summary>
public class QualityCheckDto
{
    /// <summary>
    /// Whether the quality check passed
    /// </summary>

    public bool Passed { get; set; }

    /// <summary>
    /// Notes from the quality check
    /// </summary>

    public string? Notes { get; set; }
}

/// <summary>
/// DTO for final approval
/// </summary>
public class FinalApprovalDto
{
    /// <summary>
    /// Whether the manufacturing is approved
    /// </summary>

    public bool Approved { get; set; }

    /// <summary>
    /// Notes from the final approval
    /// </summary>

    public string? Notes { get; set; }
}

/// <summary>
/// DTO for workflow history response
/// </summary>
public class ManufacturingWorkflowHistoryDto
{
    public int Id { get; set; }
    public int ProductManufactureId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionByUserName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for product manufacture summary (used in service)
/// </summary>
public class ProductManufactureSummaryDto
{
    public int ProductManufactureId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal ConsumedWeight { get; set; }
    public decimal WastageWeight { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ManufactureDate { get; set; }
    public string? BatchNumber { get; set; }
}

/// <summary>
/// DTO for manufacturing batch (used in service)
/// </summary>
public class ManufacturingBatchDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public decimal TotalConsumedWeight { get; set; }
    public decimal TotalWastageWeight { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public List<ProductManufactureDto> Products { get; set; } = new();
    public DateTime CreatedDate { get; set; }
}
