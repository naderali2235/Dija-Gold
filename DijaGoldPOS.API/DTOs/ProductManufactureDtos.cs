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
    /// Number of pieces to produce in this manufacturing batch
    /// </summary>
    public int QuantityToProduce { get; set; } = 1;
    
    /// <summary>
    /// The raw gold purchase order item that was used to manufacture this product
    /// </summary>

    public int SourceRawGoldPurchaseOrderItemId { get; set; }
    
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
    public int TechnicianId { get; set; }

    /// <summary>
    /// Manufacturing priority level
    /// </summary>
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// Current workflow step
    /// </summary>
    public string WorkflowStep { get; set; } = "Draft";

    /// <summary>
    /// Quality check status
    /// </summary>
    public string? QualityCheckStatus { get; set; }

    /// <summary>
    /// Quality checked by user ID
    /// </summary>
    public string? QualityCheckedByUserId { get; set; }

    /// <summary>
    /// Quality check date
    /// </summary>
    public DateTime? QualityCheckDate { get; set; }

    /// <summary>
    /// Quality check notes
    /// </summary>
    public string? QualityCheckNotes { get; set; }

    /// <summary>
    /// Final approval status
    /// </summary>
    public string? FinalApprovalStatus { get; set; }

    /// <summary>
    /// Final approved by user ID
    /// </summary>
    public string? FinalApprovedByUserId { get; set; }

    /// <summary>
    /// Final approval date
    /// </summary>
    public DateTime? FinalApprovalDate { get; set; }

    /// <summary>
    /// Final approval notes
    /// </summary>
    public string? FinalApprovalNotes { get; set; }

    /// <summary>
    /// Rejection reason if rejected at any stage
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Actual completion date
    /// </summary>
    public DateTime? ActualCompletionDate { get; set; }

    /// <summary>
    /// Manufacturing efficiency rating (0-100)
    /// </summary>
    public int? EfficiencyRating { get; set; }

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
    /// Number of pieces to produce (if updating quantity)
    /// </summary>
    public int? QuantityProduced { get; set; }
    
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

    /// <summary>
    /// Current workflow step
    /// </summary>
    public string? WorkflowStep { get; set; }

    /// <summary>
    /// Manufacturing priority level
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Quality check status
    /// </summary>
    public string? QualityCheckStatus { get; set; }

    /// <summary>
    /// Quality check notes
    /// </summary>
    public string? QualityCheckNotes { get; set; }

    /// <summary>
    /// Final approval status
    /// </summary>
    public string? FinalApprovalStatus { get; set; }

    /// <summary>
    /// Final approval notes
    /// </summary>
    public string? FinalApprovalNotes { get; set; }

    /// <summary>
    /// Rejection reason if rejected at any stage
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Estimated completion date
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }

    /// <summary>
    /// Actual completion date
    /// </summary>
    public DateTime? ActualCompletionDate { get; set; }

    /// <summary>
    /// Manufacturing efficiency rating (0-100)
    /// </summary>
    public int? EfficiencyRating { get; set; }
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
    public int QuantityProduced { get; set; }
    public int SourceRawGoldPurchaseOrderItemId { get; set; }
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
    public string WorkflowStep { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? QualityCheckStatus { get; set; }
    public string? QualityCheckedByUserId { get; set; }
    public DateTime? QualityCheckDate { get; set; }
    public string? QualityCheckNotes { get; set; }
    public string? FinalApprovalStatus { get; set; }
    public string? FinalApprovedByUserId { get; set; }
    public DateTime? FinalApprovalDate { get; set; }
    public string? FinalApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public int? EfficiencyRating { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
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

/// <summary>
/// DTO for raw material used in manufacturing
/// </summary>
public class ProductManufactureRawMaterialDto
{
    /// <summary>
    /// The purchase order item ID used as source material
    /// </summary>
    public int PurchaseOrderItemId { get; set; }

    /// <summary>
    /// Weight of raw gold consumed from this source (in grams)
    /// </summary>
    public decimal ConsumedWeight { get; set; }

    /// <summary>
    /// Weight lost from this source during manufacturing (wastage) in grams
    /// </summary>
    public decimal WastageWeight { get; set; } = 0;

    /// <summary>
    /// Cost per gram of this raw material at time of consumption
    /// </summary>
    public decimal CostPerGram { get; set; }

    /// <summary>
    /// Total cost of raw material consumed from this source
    /// </summary>
    public decimal TotalRawMaterialCost { get; set; }

    /// <summary>
    /// Percentage of total raw material this source contributed
    /// </summary>
    public decimal ContributionPercentage { get; set; }

    /// <summary>
    /// Sequence order when multiple sources are used
    /// </summary>
    public int SequenceOrder { get; set; } = 1;

    /// <summary>
    /// Notes about this raw material usage
    /// </summary>
    public string? Notes { get; set; }

    // Additional details for display
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int KaratTypeId { get; set; }
    public string KaratTypeName { get; set; } = string.Empty;
}

/// <summary>
/// Enhanced DTO for creating a new product manufacturing record with multiple raw materials
/// </summary>
public class CreateEnhancedProductManufactureDto
{
    /// <summary>
    /// The finished product that was manufactured
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Raw materials used in manufacturing (supports multiple sources)
    /// </summary>
    public List<ProductManufactureRawMaterialDto> RawMaterials { get; set; } = new();
    
    /// <summary>
    /// Date when the product was manufactured
    /// </summary>
    public DateTime ManufactureDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Manufacturing cost per gram (includes making charges, labor, etc.)
    /// </summary>
    public decimal ManufacturingCostPerGram { get; set; }
    
    /// <summary>
    /// Total manufacturing cost for this product (excluding raw material costs)
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
    public string Status { get; set; } = "Draft";

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
/// Enhanced DTO for product manufacturing record response with multiple raw materials
/// </summary>
public class EnhancedProductManufactureDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    
    // Legacy fields for backward compatibility
    public int SourceRawGoldPurchaseOrderItemId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    
    // Enhanced raw materials support
    public List<ProductManufactureRawMaterialDto> RawMaterials { get; set; } = new();
    
    // Calculated totals from raw materials
    public decimal TotalConsumedWeight { get; set; }
    public decimal TotalWastageWeight { get; set; }
    public decimal TotalRawMaterialCost { get; set; }
    
    public DateTime ManufactureDate { get; set; }
    public decimal ManufacturingCostPerGram { get; set; }
    public decimal TotalManufacturingCost { get; set; }
    public decimal GrandTotalCost { get; set; } // Raw materials + manufacturing
    public string? BatchNumber { get; set; }
    public string? ManufacturingNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string WorkflowStep { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
