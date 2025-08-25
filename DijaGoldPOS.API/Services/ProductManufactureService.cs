using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

public class ProductManufactureService : IProductManufactureService
{
    private readonly IProductManufactureRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IManufacturingWorkflowService _workflowService;
    private readonly ILogger<ProductManufactureService> _logger;

    public ProductManufactureService(
        IProductManufactureRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IManufacturingWorkflowService workflowService,
        ILogger<ProductManufactureService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _workflowService = workflowService;
        _logger = logger;
    }

    public async Task<ProductManufactureDto> CreateManufacturingRecordAsync(CreateProductManufactureDto createDto)
    {
        try
        {
            // Validate sufficient raw gold is available
            var isAvailable = await _repository.IsSufficientRawGoldAvailableAsync(createDto.SourcePurchaseOrderItemId, createDto.ConsumedWeight + createDto.WastageWeight);
            if (!isAvailable)
            {
                throw new InvalidOperationException("Insufficient raw gold weight available for manufacturing this product");
            }

            // Validate karat compatibility
            await ValidateKaratCompatibilityAsync(createDto.ProductId, createDto.SourcePurchaseOrderItemId);

            // Calculate comprehensive manufacturing cost including raw gold cost basis
            var totalCost = await CalculateTotalManufacturingCostAsync(createDto);

            var entity = new ProductManufacture
            {
                ProductId = createDto.ProductId,
                SourcePurchaseOrderItemId = createDto.SourcePurchaseOrderItemId,
                ConsumedWeight = createDto.ConsumedWeight,
                WastageWeight = createDto.WastageWeight,
                ManufactureDate = DateTime.UtcNow,
                ManufacturingCostPerGram = createDto.ManufacturingCostPerGram,
                TotalManufacturingCost = totalCost, // Now includes both raw gold cost + manufacturing cost
                BatchNumber = createDto.BatchNumber,
                ManufacturingNotes = createDto.ManufacturingNotes,
                Status = createDto.Status, // Use the status from DTO
                WorkflowStep = "Draft",
                Priority = createDto.Priority ?? "Normal",
                EstimatedCompletionDate = createDto.EstimatedCompletionDate,
                BranchId = createDto.BranchId,
                TechnicianId = createDto.TechnicianId ?? 0
            };

            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Update inventory for the finished product
            await UpdateProductInventoryAsync(entity.ProductId, createDto.BranchId, 1, entity.TotalManufacturingCost);

            return await GetManufacturingRecordByIdAsync(entity.Id) ??
                throw new InvalidOperationException("Failed to retrieve created record");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product manufacture record");
            throw;
        }
    }

    public async Task<ProductManufactureDto> UpdateManufacturingRecordAsync(int id, UpdateProductManufactureDto updateDto)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Record with ID {id} not found");

            if (updateDto.ConsumedWeight.HasValue)
                entity.ConsumedWeight = updateDto.ConsumedWeight.Value;

            if (updateDto.WastageWeight.HasValue)
                entity.WastageWeight = updateDto.WastageWeight.Value;

            if (updateDto.ManufacturingCostPerGram.HasValue)
            {
                entity.ManufacturingCostPerGram = updateDto.ManufacturingCostPerGram.Value;
                entity.TotalManufacturingCost = entity.ConsumedWeight * updateDto.ManufacturingCostPerGram.Value;
            }

            if (updateDto.TotalManufacturingCost.HasValue)
                entity.TotalManufacturingCost = updateDto.TotalManufacturingCost.Value;

            if (updateDto.BatchNumber != null)
                entity.BatchNumber = updateDto.BatchNumber;

            if (updateDto.ManufacturingNotes != null)
                entity.ManufacturingNotes = updateDto.ManufacturingNotes;

            if (updateDto.Status != null)
                entity.Status = updateDto.Status;

            entity.ModifiedAt = DateTime.UtcNow;

            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetManufacturingRecordByIdAsync(id) ??
                throw new InvalidOperationException("Failed to retrieve updated record");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product manufacture record");
            throw;
        }
    }

    public async Task<ProductManufactureDto?> GetManufacturingRecordByIdAsync(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<ProductManufactureDto>(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product manufacture record");
            throw;
        }
    }

    public async Task<IEnumerable<ProductManufactureDto>> GetAllManufacturingRecordsAsync()
    {
        try
        {
            var records = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductManufactureDto>>(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all manufacturing records");
            throw;
        }
    }

    public async Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByProductAsync(int productId)
    {
        try
        {
            var records = await _repository.GetByProductIdAsync(productId);
            return _mapper.Map<IEnumerable<ProductManufactureDto>>(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing records by product");
            throw;
        }
    }

    public async Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByPurchaseOrderAsync(int purchaseOrderId)
    {
        try
        {
            var records = await _repository.GetByPurchaseOrderIdAsync(purchaseOrderId);
            return _mapper.Map<IEnumerable<ProductManufactureDto>>(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing records by purchase order");
            throw;
        }
    }

    public async Task<ManufacturingSummaryByPurchaseOrderDto?> GetManufacturingSummaryByPurchaseOrderAsync(int purchaseOrderId)
    {
        try
        {
            return await _repository.GetManufacturingSummaryByPurchaseOrderAsync(purchaseOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing summary by purchase order");
            throw;
        }
    }

    public async Task<ManufacturingSummaryByProductDto?> GetManufacturingSummaryByProductAsync(int productId)
    {
        try
        {
            return await _repository.GetManufacturingSummaryByProductAsync(productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing summary by product");
            throw;
        }
    }

    public async Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByBatchAsync(string batchNumber)
    {
        try
        {
            var records = await _repository.GetByBatchNumberAsync(batchNumber);
            return _mapper.Map<IEnumerable<ProductManufactureDto>>(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing records by batch");
            throw;
        }
    }

    public async Task<IEnumerable<ProductManufactureDto>> GetManufacturingRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _repository.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<ProductManufactureDto>>(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manufacturing records by date range");
            throw;
        }
    }

    public async Task<bool> DeleteManufacturingRecordAsync(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product manufacture record");
            throw;
        }
    }

    public async Task<IEnumerable<object>> GetAvailableRawGoldItemsAsync()
    {
        try
        {
            // This is a placeholder - in a real implementation, you'd query for available raw gold items
            // For now, return empty list since we need to implement the logic
            return await Task.FromResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available raw gold items");
            throw;
        }
    }

    public async Task<decimal> GetRemainingWeightAsync(int purchaseOrderItemId)
    {
        try
        {
            var purchaseOrderItem = await _unitOfWork.Repository<PurchaseOrderItem>().GetByIdAsync(purchaseOrderItemId);
            if (purchaseOrderItem == null)
            {
                throw new InvalidOperationException($"Purchase order item with ID {purchaseOrderItemId} not found");
            }

            // Calculate total weight already consumed in manufacturing
            var totalConsumedWeight = await _repository.GetTotalConsumedWeightByPurchaseOrderItemAsync(purchaseOrderItemId);

            // Calculate remaining weight
            var remainingWeight = purchaseOrderItem.WeightReceived - totalConsumedWeight;

            return Math.Max(0, remainingWeight); // Ensure non-negative result
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving remaining weight for purchase order item {PurchaseOrderItemId}", purchaseOrderItemId);
            throw;
        }
    }

    public async Task<bool> CheckSufficientWeightAsync(int purchaseOrderItemId, decimal requiredWeight)
    {
        try
        {
            var remainingWeight = await GetRemainingWeightAsync(purchaseOrderItemId);
            return remainingWeight >= requiredWeight;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking sufficient weight for purchase order item {PurchaseOrderItemId}", purchaseOrderItemId);
            throw;
        }
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateManufacturingRequestAsync(CreateProductManufactureDto createDto)
    {
        try
        {
            // Validate that the product exists
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(createDto.ProductId);
            if (product == null)
            {
                return (false, "Product not found");
            }

            // Validate that the purchase order item exists
            var purchaseOrderItem = await _unitOfWork.Repository<PurchaseOrderItem>().GetByIdAsync(createDto.SourcePurchaseOrderItemId);
            if (purchaseOrderItem == null)
            {
                return (false, "Purchase order item not found");
            }

            // Validate sufficient raw gold is available
            var isAvailable = await _repository.IsSufficientRawGoldAvailableAsync(createDto.SourcePurchaseOrderItemId, createDto.ConsumedWeight + createDto.WastageWeight);
            if (!isAvailable)
            {
                return (false, "Insufficient raw gold weight available for manufacturing this product");
            }

            // Validate karat compatibility
            try
            {
                await ValidateKaratCompatibilityAsync(createDto.ProductId, createDto.SourcePurchaseOrderItemId);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating manufacturing request");
            return (false, "An error occurred during validation");
        }
    }

    #region Workflow Methods

    public async Task<bool> TransitionWorkflowAsync(int id, string targetStatus, string? notes = null)
    {
        try
        {
            return await _workflowService.TransitionWorkflowAsync(id, targetStatus, notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning workflow for manufacturing {Id}", id);
            throw;
        }
    }

    public async Task<bool> PerformQualityCheckAsync(int id, bool passed, string? notes = null)
    {
        try
        {
            return await _workflowService.PerformQualityCheckAsync(id, passed, notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing quality check for manufacturing {Id}", id);
            throw;
        }
    }

    public async Task<bool> PerformFinalApprovalAsync(int id, bool approved, string? notes = null)
    {
        try
        {
            return await _workflowService.PerformFinalApprovalAsync(id, approved, notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing final approval for manufacturing {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ManufacturingWorkflowHistory>> GetWorkflowHistoryAsync(int id)
    {
        try
        {
            return await _workflowService.GetWorkflowHistoryAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow history for manufacturing {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetAvailableTransitionsAsync(int id)
    {
        try
        {
            var manufacture = await GetManufacturingRecordByIdAsync(id);
            if (manufacture == null)
            {
                return Array.Empty<string>();
            }

            return _workflowService.GetAvailableTransitions(manufacture.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available transitions for manufacturing {Id}", id);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    private async Task ValidateKaratCompatibilityAsync(int productId, int purchaseOrderItemId)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        var purchaseOrderItem = await _unitOfWork.Repository<PurchaseOrderItem>().GetByIdAsync(purchaseOrderItemId);

        if (product == null || purchaseOrderItem == null)
        {
            throw new InvalidOperationException("Invalid product or purchase order item");
        }

        // For raw gold items, validate karat compatibility
        if (purchaseOrderItem.IsRawGold && purchaseOrderItem.RawGoldKaratTypeId.HasValue)
        {
            if (product.KaratTypeId != purchaseOrderItem.RawGoldKaratTypeId.Value)
            {
                throw new InvalidOperationException(
                    $"Product karat ({product.KaratType?.Name ?? "Unknown"}) does not match raw gold karat ({purchaseOrderItem.RawGoldKaratType?.Name ?? "Unknown"})"
                );
            }
        }
    }

    private async Task<decimal> CalculateTotalManufacturingCostAsync(CreateProductManufactureDto createDto)
    {
        // Get the purchase order item to calculate raw gold cost
        var purchaseOrderItem = await _unitOfWork.Repository<PurchaseOrderItem>().GetByIdAsync(createDto.SourcePurchaseOrderItemId);
        if (purchaseOrderItem == null)
        {
            throw new InvalidOperationException("Purchase order item not found");
        }

        // Raw gold cost = (consumed weight / total received weight) * total line cost
        var rawGoldCost = 0m;
        if (purchaseOrderItem.WeightReceived > 0)
        {
            rawGoldCost = (createDto.ConsumedWeight / purchaseOrderItem.WeightReceived) * purchaseOrderItem.LineTotal;
        }

        // Manufacturing cost = consumed weight * manufacturing cost per gram
        var manufacturingCost = createDto.ConsumedWeight * createDto.ManufacturingCostPerGram;

        // Total cost = raw gold cost + manufacturing cost
        return rawGoldCost + manufacturingCost;
    }

    private async Task UpdateProductInventoryAsync(int productId, int branchId, decimal quantity, decimal unitCost)
    {
        try
        {
            // Get or create inventory record
            var inventory = await _unitOfWork.Repository<Inventory>()
                .FindFirstAsync(i => i.ProductId == productId && i.BranchId == branchId);

            if (inventory == null)
            {
                // Create new inventory record
                inventory = new Inventory
                {
                    ProductId = productId,
                    BranchId = branchId,
                    QuantityOnHand = quantity,
                    WeightOnHand = quantity, // Assuming quantity = weight for gold products
                    MinimumStockLevel = 0,
                    MaximumStockLevel = 1000,
                    ReorderPoint = 10
                };
                await _unitOfWork.Repository<Inventory>().AddAsync(inventory);
            }
            else
            {
                // Update existing inventory
                inventory.QuantityOnHand += quantity;
                inventory.WeightOnHand += quantity; // Assuming quantity = weight for gold products
                _unitOfWork.Repository<Inventory>().Update(inventory);
            }

            // Record inventory movement
            var movement = new InventoryMovement
            {
                InventoryId = inventory.Id,
                QuantityChange = quantity,
                WeightChange = quantity,
                UnitCost = unitCost,
                MovementType = "Manufactured",
                ReferenceNumber = $"MANUF-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
                Notes = $"Product manufactured from raw gold"
            };

            await _unitOfWork.Repository<InventoryMovement>().AddAsync(movement);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product inventory for product {ProductId}", productId);
            throw;
        }
    }

    #endregion
}
