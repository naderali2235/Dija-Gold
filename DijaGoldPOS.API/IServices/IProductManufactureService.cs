using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service interface for ProductManufacture business operations
/// </summary>
public interface IProductManufactureService
{
    /// <summary>
    /// Create a new manufacturing record
    /// </summary>
    Task<ProductManufactureDto> CreateManufacturingRecordAsync(CreateProductManufactureDto createDto);
    
    /// <summary>
    /// Update an existing manufacturing record
    /// </summary>
    Task<ProductManufactureDto> UpdateManufacturingRecordAsync(int id, UpdateProductManufactureDto updateDto);
    
    /// <summary>
    /// Get a manufacturing record by ID
    /// </summary>
    Task<ProductManufactureDto?> GetManufacturingRecordByIdAsync(int id);
    
    /// <summary>
    /// Get all manufacturing records
    /// </summary>
    Task<IEnumerable<ProductManufactureDto>> GetAllManufacturingRecordsAsync();
    
    /// <summary>
    /// Get manufacturing records for a specific product
    /// </summary>
    Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByProductAsync(int productId);
    
    /// <summary>
    /// Get manufacturing records for a specific purchase order
    /// </summary>
    Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByPurchaseOrderAsync(int purchaseOrderId);
    
    /// <summary>
    /// Get manufacturing summary by purchase order
    /// </summary>
    Task<ManufacturingSummaryByPurchaseOrderDto?> GetManufacturingSummaryByPurchaseOrderAsync(int purchaseOrderId);
    
    /// <summary>
    /// Get manufacturing summary by product
    /// </summary>
    Task<ManufacturingSummaryByProductDto?> GetManufacturingSummaryByProductAsync(int productId);
    
    /// <summary>
    /// Get manufacturing records by batch number
    /// </summary>
    Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByBatchAsync(string batchNumber);
    
    /// <summary>
    /// Get manufacturing records within a date range
    /// </summary>
    Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Delete a manufacturing record
    /// </summary>
    Task<bool> DeleteManufacturingRecordAsync(int id);
    
    /// <summary>
    /// Get available raw gold purchase order items for manufacturing
    /// </summary>
    Task<IEnumerable<RawGoldPurchaseOrderItemDto>> GetAvailableRawGoldItemsAsync(int? branchId = null);

    /// <summary>
    /// Get remaining weight for a specific raw gold purchase order item
    /// </summary>
    Task<decimal> GetRemainingWeightAsync(int rawGoldPurchaseOrderItemId);

    /// <summary>
    /// Check if a raw gold purchase order item has sufficient weight for manufacturing
    /// </summary>
    Task<bool> CheckSufficientWeightAsync(int rawGoldPurchaseOrderItemId, decimal requiredWeight);
    
    /// <summary>
    /// Validate manufacturing request
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateManufacturingRequestAsync(CreateProductManufactureDto createDto);

    #region Workflow Methods

    /// <summary>
    /// Transitions a manufacturing record to the next workflow step
    /// </summary>
    Task<bool> TransitionWorkflowAsync(int id, string targetStatus, string? notes = null);

    /// <summary>
    /// Performs quality check on a manufacturing record
    /// </summary>
    Task<bool> PerformQualityCheckAsync(int id, bool passed, string? notes = null);

    /// <summary>
    /// Performs final approval on a manufacturing record
    /// </summary>
    Task<bool> PerformFinalApprovalAsync(int id, bool approved, string? notes = null);

    /// <summary>
    /// Gets workflow history for a manufacturing record
    /// </summary>
    Task<IEnumerable<ManufacturingWorkflowHistory>> GetWorkflowHistoryAsync(int id);

    /// <summary>
    /// Gets available workflow transitions for a manufacturing record
    /// </summary>
    Task<IEnumerable<string>> GetAvailableTransitionsAsync(int id);

    #endregion
}
