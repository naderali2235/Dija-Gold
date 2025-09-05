using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using Microsoft.EntityFrameworkCore;
using DijaGoldPOS.API.Shared;
using Microsoft.Extensions.Logging;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Models.PurchaseOrderModels;
using DijaGoldPOS.API.Models.ManfacturingModels;
using DijaGoldPOS.API.Models.OwneShipModels;
using DijaGoldPOS.API.Models.InventoryModels;

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
            // Calculate total weight needed for the requested quantity
            var totalWeightPerPiece = createDto.ConsumedWeight + createDto.WastageWeight;
            var totalWeightNeeded = totalWeightPerPiece * createDto.QuantityToProduce;

            // Reserve raw gold inventory before creating manufacturing record
            await ReserveRawGoldForManufacturingAsync(createDto.RawMaterials, createDto.CreatedBy);

            // Validate sufficient raw gold is available for the requested quantity
            var isAvailable = await IsRawGoldSufficientAsync(createDto.SourceRawGoldPurchaseOrderItemId, totalWeightNeeded);
            if (!isAvailable)
            {
                // Reverse reservations if validation fails
                await ReverseRawGoldReservationsAsync(createDto.RawMaterials, createDto.CreatedBy);
                throw new InvalidOperationException($"Insufficient raw gold weight available for manufacturing {createDto.QuantityToProduce} pieces. Required: {totalWeightNeeded}g");
            }

            // Validate karat compatibility using raw gold purchase order item
            await ValidateRawGoldKaratCompatibilityAsync(createDto.ProductId, createDto.SourceRawGoldPurchaseOrderItemId);

            // Calculate comprehensive manufacturing cost including raw gold cost basis
            var totalCost = await CalculateRawGoldManufacturingCostAsync(createDto);

            var entity = new ProductManufacture
            {
                ProductId = createDto.ProductId,
                QuantityProduced = createDto.QuantityToProduce,
                SourceRawGoldPurchaseOrderItemId = createDto.SourceRawGoldPurchaseOrderItemId,
                ConsumedWeight = totalWeightNeeded,
                WastageWeight = createDto.WastageWeight * createDto.QuantityToProduce,
                ManufactureDate = DateTime.UtcNow,
                ManufacturingCostPerGram = createDto.ManufacturingCostPerGram,
                TotalManufacturingCost = totalCost, // Now includes both raw gold cost + manufacturing cost
                BatchNumber = createDto.BatchNumber,
                ManufacturingNotes = createDto.ManufacturingNotes,
                Status = "Draft", // Force initial status to Draft; status changes must go through workflow
                WorkflowStep = "Draft",
                Priority = createDto.Priority ?? "Normal",
                EstimatedCompletionDate = createDto.EstimatedCompletionDate,
                BranchId = createDto.BranchId,
                TechnicianId = createDto.TechnicianId
            };

            await _repository.AddAsync(entity);
            
            // Save the ProductManufacture entity first to get the ID
            await _unitOfWork.SaveChangesAsync();

            // Create raw material record for tracking (now that we have the ProductManufacture ID)
            var rawMaterial = new ProductManufactureRawMaterial
            {
                ProductManufactureId = entity.Id,
                RawGoldPurchaseOrderItemId = createDto.SourceRawGoldPurchaseOrderItemId,
                ConsumedWeight = totalWeightNeeded,
                WastageWeight = createDto.WastageWeight * createDto.QuantityToProduce,
                CostPerGram = createDto.ManufacturingCostPerGram,
                TotalRawMaterialCost = totalCost,
                ContributionPercentage = 1.0m, // 100% since it's the only source
                SequenceOrder = 1,
                Notes = $"Raw gold from purchase order item {createDto.SourceRawGoldPurchaseOrderItemId}"
            };

            // Add raw material record to the context
            await _unitOfWork.Repository<ProductManufactureRawMaterial>().AddAsync(rawMaterial);

            // Update raw gold consumption in the raw gold purchase order item
            await UpdateRawGoldConsumptionAsync(createDto.SourceRawGoldPurchaseOrderItemId, totalWeightNeeded);

            // Update raw gold inventory
            await UpdateRawGoldInventoryAsync(createDto.SourceRawGoldPurchaseOrderItemId, createDto.BranchId, totalWeightNeeded);

            // Update inventory for the finished product (add the quantity produced)
            await UpdateProductInventoryAsync(entity.ProductId, createDto.BranchId, createDto.QuantityToProduce, entity.TotalManufacturingCost);

            // Save all remaining changes
            await _unitOfWork.SaveChangesAsync();

            return await GetManufacturingRecordByIdAsync(entity.Id) ??
                throw new InvalidOperationException("Failed to retrieve created record");
        }
        catch (Exception ex)
        {
            // Reverse any reservations made if creation fails
            try
            {
                await ReverseRawGoldReservationsAsync(createDto.RawMaterials, createDto.CreatedBy);
            }
            catch (Exception reverseEx)
            {
                _logger.LogError(reverseEx, "Error reversing raw gold reservations after manufacturing creation failure");
            }
            
            _logger.LogError(ex, "Error creating manufacturing record");
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

            // Disallow direct status changes here; enforce workflow endpoints for transitions
            if (updateDto.Status != null)
            {
                throw new InvalidOperationException("Direct status updates are not allowed. Use the workflow endpoints to transition status.");
            }

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

    public async Task<IEnumerable<RawGoldPurchaseOrderItemDto>> GetAvailableRawGoldItemsAsync(int? branchId = null)
    {
        try
        {
            var query = _unitOfWork.Repository<RawGoldPurchaseOrderItem>()
                .GetQueryable()
                .Where(item => item.Status == "Received" && item.AvailableWeightForManufacturing > 0);

            if (branchId.HasValue)
            {
                query = query.Where(item => item.RawGoldPurchaseOrder.BranchId == branchId.Value);
            }

            var items = await query
                .Include(item => item.KaratType)
                .Include(item => item.RawGoldPurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RawGoldPurchaseOrderItemDto>>(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available raw gold items");
            throw;
        }
    }

    public async Task<decimal> GetRemainingWeightAsync(int rawGoldPurchaseOrderItemId)
    {
        try
        {
            var rawGoldItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>().GetByIdAsync(rawGoldPurchaseOrderItemId);
            if (rawGoldItem == null)
            {
                throw new InvalidOperationException($"Raw gold purchase order item with ID {rawGoldPurchaseOrderItemId} not found");
            }

            // Calculate total weight already consumed in manufacturing
            var totalConsumedWeight = await _repository.GetTotalConsumedWeightByRawGoldItemAsync(rawGoldPurchaseOrderItemId);

            // Calculate remaining weight
            var remainingWeight = rawGoldItem.WeightReceived - totalConsumedWeight;

            return Math.Max(0, remainingWeight); // Ensure non-negative result
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving remaining weight for raw gold purchase order item {RawGoldPurchaseOrderItemId}", rawGoldPurchaseOrderItemId);
            throw;
        }
    }

    public async Task<bool> CheckSufficientWeightAsync(int rawGoldPurchaseOrderItemId, decimal requiredWeight)
    {
        try
        {
            var remainingWeight = await GetRemainingWeightAsync(rawGoldPurchaseOrderItemId);
            return remainingWeight >= requiredWeight;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking sufficient weight for raw gold purchase order item {RawGoldPurchaseOrderItemId}", rawGoldPurchaseOrderItemId);
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

            // Validate that the raw gold purchase order item exists
            var rawGoldItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>().GetByIdAsync(createDto.SourceRawGoldPurchaseOrderItemId);
            if (rawGoldItem == null)
            {
                return (false, "Raw gold purchase order item not found");
            }

            // Validate sufficient raw gold is available
            var isAvailable = await IsRawGoldSufficientAsync(createDto.SourceRawGoldPurchaseOrderItemId, createDto.ConsumedWeight + createDto.WastageWeight);
            if (!isAvailable)
            {
                return (false, "Insufficient raw gold weight available for manufacturing this product");
            }

            // Validate karat compatibility
            try
            {
                await ValidateRawGoldKaratCompatibilityAsync(createDto.ProductId, createDto.SourceRawGoldPurchaseOrderItemId);
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

    private async Task<bool> IsRawGoldSufficientAsync(int rawGoldPurchaseOrderItemId, decimal requiredWeight)
    {
        try
        {
            var rawGoldItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>().GetByIdAsync(rawGoldPurchaseOrderItemId);
            if (rawGoldItem == null)
            {
                return false;
            }

            return rawGoldItem.AvailableWeightForManufacturing >= requiredWeight;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking raw gold sufficiency for item {RawGoldPurchaseOrderItemId}", rawGoldPurchaseOrderItemId);
            return false;
        }
    }

    private async Task ValidateRawGoldKaratCompatibilityAsync(int productId, int rawGoldPurchaseOrderItemId)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetQueryable()
            .Include(p => p.KaratType)
            .FirstOrDefaultAsync(p => p.Id == productId);
            
        var rawGoldItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>()
            .GetQueryable()
            .Include(item => item.KaratType)
            .FirstOrDefaultAsync(item => item.Id == rawGoldPurchaseOrderItemId);

        if (product == null || rawGoldItem == null)
        {
            throw new InvalidOperationException("Invalid product or raw gold purchase order item");
        }

        // Validate karat compatibility between product and raw gold
        if (product.KaratTypeId != rawGoldItem.KaratTypeId)
        {
            throw new InvalidOperationException($"Karat type mismatch: Product requires {product.KaratType?.Name} but raw gold is {rawGoldItem.KaratType?.Name}");
        }
    }

    private async Task<decimal> CalculateRawGoldManufacturingCostAsync(CreateProductManufactureDto createDto)
    {
        // Get the raw gold purchase order item to calculate raw gold cost
        var rawGoldItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>().GetByIdAsync(createDto.SourceRawGoldPurchaseOrderItemId);
        if (rawGoldItem == null)
        {
            throw new InvalidOperationException("Raw gold purchase order item not found");
        }

        // Raw gold cost = consumed weight * unit cost per gram
        var rawGoldCost = createDto.ConsumedWeight * rawGoldItem.UnitCostPerGram;

        // Manufacturing cost = consumed weight * manufacturing cost per gram
        var manufacturingCost = createDto.ConsumedWeight * createDto.ManufacturingCostPerGram;

        // Total cost = raw gold cost + manufacturing cost
        return rawGoldCost + manufacturingCost;
    }

    private async Task UpdateRawGoldConsumptionAsync(int rawGoldPurchaseOrderItemId, decimal consumedWeight)
    {
        try
        {
            var rawGoldItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>().GetByIdAsync(rawGoldPurchaseOrderItemId);
            if (rawGoldItem == null)
            {
                throw new InvalidOperationException($"Raw gold purchase order item with ID {rawGoldPurchaseOrderItemId} not found");
            }

            // Update consumed weight
            rawGoldItem.WeightConsumedInManufacturing += consumedWeight;
            // Keep AvailableWeightForManufacturing in sync with consumption. This field has a backing store
            // and was previously set during receiving, so we must update it explicitly here.
            rawGoldItem.AvailableWeightForManufacturing = Math.Max(0m, rawGoldItem.WeightReceived - rawGoldItem.WeightConsumedInManufacturing);
            _unitOfWork.Repository<RawGoldPurchaseOrderItem>().Update(rawGoldItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw gold consumption for item {RawGoldPurchaseOrderItemId}", rawGoldPurchaseOrderItemId);
            throw;
        }
    }

    private async Task UpdateRawGoldInventoryAsync(int rawGoldPurchaseOrderItemId, int branchId, decimal consumedWeight)
    {
        try
        {
            // Get the raw gold item to determine karat type
            var rawGoldItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>()
                .GetQueryable()
                .Include(item => item.KaratType)
                .FirstOrDefaultAsync(item => item.Id == rawGoldPurchaseOrderItemId);

            if (rawGoldItem == null)
            {
                throw new InvalidOperationException($"Raw gold purchase order item with ID {rawGoldPurchaseOrderItemId} not found");
            }

            // Find or create raw gold inventory record
            var rawGoldInventory = await _unitOfWork.Repository<RawGoldInventory>()
                .FindFirstAsync(inv => inv.BranchId == branchId && inv.KaratTypeId == rawGoldItem.KaratTypeId);

            if (rawGoldInventory != null)
            {
                // Update existing inventory - reduce weight on hand
                rawGoldInventory.WeightOnHand -= consumedWeight;
                // Reduce reserved as we are consuming from reserved stock while PO is open
                rawGoldInventory.WeightReserved = Math.Max(0m, rawGoldInventory.WeightReserved - consumedWeight);
                rawGoldInventory.LastMovementDate = DateTime.UtcNow;
                _unitOfWork.Repository<RawGoldInventory>().Update(rawGoldInventory);

                // Record inventory movement
                var movement = new RawGoldInventoryMovement
                {
                    RawGoldInventoryId = rawGoldInventory.Id,
                    RawGoldPurchaseOrderId = rawGoldItem.RawGoldPurchaseOrderId,
                    RawGoldPurchaseOrderItemId = rawGoldPurchaseOrderItemId,
                    MovementType = "Manufacturing Consumption",
                    WeightChange = -consumedWeight,
                    WeightBalance = rawGoldInventory.WeightOnHand,
                    MovementDate = DateTime.UtcNow,
                    ReferenceNumber = $"MANUF-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
                    UnitCost = rawGoldItem.UnitCostPerGram,
                    TotalCost = consumedWeight * rawGoldItem.UnitCostPerGram,
                    Notes = $"Raw gold consumed in manufacturing"
                };

                await _unitOfWork.Repository<RawGoldInventoryMovement>().AddAsync(movement);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw gold inventory for item {RawGoldPurchaseOrderItemId}", rawGoldPurchaseOrderItemId);
            throw;
        }
    }

    private async Task UpdateProductInventoryAsync(int productId, int branchId, decimal quantity, decimal unitCost)
    {
        try
        {

            // Get or create inventory record
            var inventory = await _unitOfWork.Repository<Inventory>()
                .FindFirstAsync(i => i.ProductId == productId && i.BranchId == branchId);

            bool isNewInventory = inventory == null;
            
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
                
                // Save changes to get the inventory ID
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                // Update existing inventory
                inventory.QuantityOnHand += quantity;
                inventory.WeightOnHand += quantity; // Assuming quantity = weight for gold products
                _unitOfWork.Repository<Inventory>().Update(inventory);
            }

            // Record inventory movement (now inventory.Id is available)
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product inventory for product {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Transfer ownership from raw gold to manufactured products based on payment percentage
    /// </summary>
    private async Task TransferOwnershipFromRawGoldAsync(int manufacturingRecordId, List<ProductManufactureRawMaterialDto> rawMaterials, string userId)
    {
        try
        {
            foreach (var material in rawMaterials)
            {
                // Get raw gold ownership records for this karat type and supplier
                var rawGoldOwnerships = await _unitOfWork.Repository<RawGoldOwnership>().GetQueryable()
                    .Where(rgo => rgo.KaratTypeId == material.KaratTypeId && 
                                  rgo.SupplierId == material.SupplierId &&
                                  rgo.OwnedWeight > 0 &&
                                  rgo.IsActive)
                    .OrderBy(rgo => rgo.CreatedAt) // FIFO
                    .ToListAsync();

                decimal remainingWeightToTransfer = material.WeightUsed;

                foreach (var ownership in rawGoldOwnerships)
                {
                    if (remainingWeightToTransfer <= 0) break;

                    decimal weightToDeduct = Math.Min(ownership.OwnedWeight, remainingWeightToTransfer);
                    decimal costPerGram = ownership.TotalCost / ownership.TotalWeight;
                    decimal transferCost = weightToDeduct * costPerGram;

                    // Calculate ownership percentage based on payment - ONLY paid portion gets 100% ownership
                    decimal paymentPercentage = ownership.AmountPaid / ownership.TotalCost;
                    decimal paidWeight = weightToDeduct * paymentPercentage;
                    decimal unpaidWeight = weightToDeduct * (1 - paymentPercentage);

                    // Create product ownership for manufactured product - split by payment status
                    if (paidWeight > 0)
                    {
                        // Fully paid portion gets 100% ownership
                        var paidOwnership = new ProductOwnership
                        {
                            ProductId = material.ProductId,
                            BranchId = ownership.BranchId,
                            SupplierId = ownership.SupplierId,
                            TotalQuantity = paidWeight,
                            TotalWeight = paidWeight,
                            OwnedQuantity = paidWeight, // 100% ownership for paid portion
                            OwnedWeight = paidWeight,
                            OwnershipPercentage = 1.0m, // 100%
                            TotalCost = transferCost * paymentPercentage,
                            AmountPaid = transferCost * paymentPercentage,
                            OutstandingAmount = 0,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId
                        };
                        await _unitOfWork.Repository<ProductOwnership>().AddAsync(paidOwnership);
                    }

                    if (unpaidWeight > 0)
                    {
                        // Unpaid portion gets 0% ownership
                        var unpaidOwnership = new ProductOwnership
                        {
                            ProductId = material.ProductId,
                            BranchId = ownership.BranchId,
                            SupplierId = ownership.SupplierId,
                            TotalQuantity = unpaidWeight,
                            TotalWeight = unpaidWeight,
                            OwnedQuantity = 0, // 0% ownership for unpaid portion
                            OwnedWeight = 0,
                            OwnershipPercentage = 0.0m, // 0%
                            TotalCost = transferCost * (1 - paymentPercentage),
                            AmountPaid = 0,
                            OutstandingAmount = transferCost * (1 - paymentPercentage),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId
                        };
                        await _unitOfWork.Repository<ProductOwnership>().AddAsync(unpaidOwnership);
                    }

                    // Reduce raw gold ownership
                    ownership.OwnedWeight -= weightToDeduct;
                    ownership.TotalWeight -= weightToDeduct;
                    ownership.TotalCost -= transferCost;
                    ownership.AmountPaid -= transferCost * paymentPercentage;
                    ownership.OutstandingAmount -= transferCost * (1 - paymentPercentage);
                    ownership.ModifiedAt = DateTime.UtcNow;
                    ownership.ModifiedBy = userId;

                    if (ownership.OwnedWeight <= 0)
                    {
                        ownership.IsActive = false;
                    }

                    remainingWeightToTransfer -= weightToDeduct;
                }

                if (remainingWeightToTransfer > 0)
                {
                    _logger.LogWarning("Insufficient raw gold ownership for transfer. Remaining: {Weight}g", remainingWeightToTransfer);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring ownership from raw gold to manufactured products");
            throw;
        }
    }

    #endregion

    #region Raw Gold Inventory Reservation Methods

    /// <summary>
    /// Reserve raw gold inventory for manufacturing
    /// </summary>
    private async Task ReserveRawGoldForManufacturingAsync(List<ProductManufactureRawMaterialDto> rawMaterials, string userId)
    {
        foreach (var material in rawMaterials)
        {
            var inventory = await _unitOfWork.Repository<RawGoldInventory>()
                .GetQueryable()
                .FirstOrDefaultAsync(rgi => rgi.KaratTypeId == material.KaratTypeId && 
                                          rgi.IsActive);

            if (inventory == null)
            {
                throw new InvalidOperationException($"Raw gold inventory not found for karat type {material.KaratTypeId}");
            }

            if (inventory.AvailableWeight < material.WeightUsed)
            {
                throw new InvalidOperationException($"Insufficient raw gold available. Required: {material.WeightUsed}g, Available: {inventory.AvailableWeight}g");
            }

            // Reserve the weight
            inventory.WeightReserved += material.WeightUsed;
            inventory.ModifiedAt = DateTime.UtcNow;
            inventory.ModifiedBy = userId;

            // Create inventory movement record
            var movement = new RawGoldInventoryMovement
            {
                RawGoldInventoryId = inventory.Id,
                MovementType = "Reserved",
                WeightChange = -material.WeightUsed, // Negative because it reduces available weight
                WeightAfter = inventory.AvailableWeight,
                CostPerGram = inventory.AverageCostPerGram,
                TotalCost = material.WeightUsed * inventory.AverageCostPerGram,
                Reference = $"Manufacturing Reservation",
                Notes = $"Reserved {material.WeightUsed}g for manufacturing",
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            await _unitOfWork.Repository<RawGoldInventoryMovement>().AddAsync(movement);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Reverse raw gold reservations (used when manufacturing fails or is cancelled)
    /// </summary>
    private async Task ReverseRawGoldReservationsAsync(List<ProductManufactureRawMaterialDto> rawMaterials, string userId)
    {
        foreach (var material in rawMaterials)
        {
            var inventory = await _unitOfWork.Repository<RawGoldInventory>()
                .GetQueryable()
                .FirstOrDefaultAsync(rgi => rgi.KaratTypeId == material.KaratTypeId && 
                                          rgi.IsActive);

            if (inventory != null)
            {
                // Release the reservation
                inventory.WeightReserved = Math.Max(0, inventory.WeightReserved - material.WeightUsed);
                inventory.ModifiedAt = DateTime.UtcNow;
                inventory.ModifiedBy = userId;

                // Create inventory movement record
                var movement = new RawGoldInventoryMovement
                {
                    RawGoldInventoryId = inventory.Id,
                    MovementType = "Reservation Reversed",
                    WeightChange = material.WeightUsed, // Positive because it increases available weight
                    WeightAfter = inventory.AvailableWeight,
                    CostPerGram = inventory.AverageCostPerGram,
                    TotalCost = material.WeightUsed * inventory.AverageCostPerGram,
                    Reference = $"Manufacturing Reservation Reversal",
                    Notes = $"Reversed reservation of {material.WeightUsed}g",
                    MovementDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsActive = true
                };

                await _unitOfWork.Repository<RawGoldInventoryMovement>().AddAsync(movement);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Complete manufacturing workflow and finalize raw gold consumption
    /// </summary>
    public async Task<ProductManufactureDto> CompleteManufacturingAsync(int manufacturingId, string userId)
    {
        try
        {
            var manufacturing = await _repository.GetByIdAsync(manufacturingId);
            if (manufacturing == null)
            {
                throw new InvalidOperationException($"Manufacturing record {manufacturingId} not found");
            }

            if (manufacturing.Status == "Completed")
            {
                throw new InvalidOperationException("Manufacturing is already completed");
            }

            if (manufacturing.Status == "Cancelled")
            {
                throw new InvalidOperationException("Cannot complete cancelled manufacturing");
            }

            // Get raw materials used in this manufacturing
            var rawMaterials = await GetRawMaterialsForManufacturingAsync(manufacturingId);

            // Complete raw gold consumption (convert reservations to actual consumption)
            await CompleteRawGoldConsumptionAsync(rawMaterials, userId);

            // Transfer ownership from raw gold to manufactured products
            await TransferOwnershipFromRawGoldAsync(manufacturingId, rawMaterials, userId);

            // Update manufacturing status
            manufacturing.Status = "Completed";
            manufacturing.WorkflowStep = "Complete";
            manufacturing.ModifiedAt = DateTime.UtcNow;
            manufacturing.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Manufacturing {ManufacturingId} completed successfully", manufacturingId);

            return _mapper.Map<ProductManufactureDto>(manufacturing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing manufacturing {ManufacturingId}", manufacturingId);
            throw;
        }
    }

    /// <summary>
    /// Cancel manufacturing and reverse all reservations and records
    /// </summary>
    public async Task<ProductManufactureDto> CancelManufacturingAsync(int manufacturingId, string userId, string? cancellationReason = null)
    {
        try
        {
            var manufacturing = await _repository.GetByIdAsync(manufacturingId);
            if (manufacturing == null)
            {
                throw new InvalidOperationException($"Manufacturing record {manufacturingId} not found");
            }

            if (manufacturing.Status == "Completed")
            {
                throw new InvalidOperationException("Cannot cancel completed manufacturing");
            }

            if (manufacturing.Status == "Cancelled")
            {
                throw new InvalidOperationException("Manufacturing is already cancelled");
            }

            // Get raw materials used in this manufacturing
            var rawMaterials = await GetRawMaterialsForManufacturingAsync(manufacturingId);

            // Reverse raw gold reservations
            await ReverseRawGoldReservationsAsync(rawMaterials, userId);

            // Reverse any product ownership records created
            await ReverseProductOwnershipAsync(manufacturingId, userId);

            // Reverse product inventory if any was added
            await ReverseProductInventoryAsync(manufacturing.ProductId, manufacturing.BranchId, manufacturing.QuantityProduced, userId);

            // Update manufacturing status
            manufacturing.Status = "Cancelled";
            manufacturing.WorkflowStep = "Cancelled";
            manufacturing.ManufacturingNotes = string.IsNullOrEmpty(manufacturing.ManufacturingNotes) 
                ? $"Cancelled: {cancellationReason}" 
                : $"{manufacturing.ManufacturingNotes}\nCancelled: {cancellationReason}";
            manufacturing.ModifiedAt = DateTime.UtcNow;
            manufacturing.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Manufacturing {ManufacturingId} cancelled successfully. Reason: {Reason}", manufacturingId, cancellationReason);

            return _mapper.Map<ProductManufactureDto>(manufacturing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling manufacturing {ManufacturingId}", manufacturingId);
            throw;
        }
    }

    /// <summary>
    /// Complete manufacturing and convert reservations to actual consumption
    /// </summary>
    private async Task CompleteRawGoldConsumptionAsync(List<ProductManufactureRawMaterialDto> rawMaterials, string userId)
    {
        foreach (var material in rawMaterials)
        {
            var inventory = await _unitOfWork.Repository<RawGoldInventory>()
                .GetQueryable()
                .FirstOrDefaultAsync(rgi => rgi.KaratTypeId == material.KaratTypeId && 
                                          rgi.IsActive);

            if (inventory == null)
            {
                throw new InvalidOperationException($"Raw gold inventory not found for karat type {material.KaratTypeId}");
            }

            // Convert reservation to actual consumption
            inventory.WeightOnHand -= material.WeightUsed;
            inventory.WeightReserved = Math.Max(0, inventory.WeightReserved - material.WeightUsed);
            inventory.LastMovementDate = DateTime.UtcNow;
            inventory.ModifiedAt = DateTime.UtcNow;
            inventory.ModifiedBy = userId;

            // Create inventory movement record for consumption
            var movement = new RawGoldInventoryMovement
            {
                RawGoldInventoryId = inventory.Id,
                MovementType = "Manufacturing Consumption",
                WeightChange = -material.WeightUsed,
                WeightAfter = inventory.WeightOnHand,
                CostPerGram = inventory.AverageCostPerGram,
                TotalCost = material.WeightUsed * inventory.AverageCostPerGram,
                Reference = $"Manufacturing Completed",
                Notes = $"Consumed {material.WeightUsed}g in manufacturing",
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            await _unitOfWork.Repository<RawGoldInventoryMovement>().AddAsync(movement);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Get raw materials used in a specific manufacturing record
    /// </summary>
    private async Task<List<ProductManufactureRawMaterialDto>> GetRawMaterialsForManufacturingAsync(int manufacturingId)
    {
        // This would typically come from a manufacturing raw materials table
        // For now, we'll reconstruct from the manufacturing record
        var manufacturing = await _repository.GetByIdAsync(manufacturingId);
        if (manufacturing == null)
        {
            return new List<ProductManufactureRawMaterialDto>();
        }

        // Get the raw gold purchase order item details
        var purchaseOrderItem = await _unitOfWork.Repository<RawGoldPurchaseOrderItem>()
            .GetQueryable()
            .Include(poi => poi.RawGoldPurchaseOrder)
            .FirstOrDefaultAsync(poi => poi.Id == manufacturing.SourceRawGoldPurchaseOrderItemId);

        if (purchaseOrderItem == null)
        {
            return new List<ProductManufactureRawMaterialDto>();
        }

        return new List<ProductManufactureRawMaterialDto>
        {
            new ProductManufactureRawMaterialDto
            {
                PurchaseOrderItemId = purchaseOrderItem.Id,
                ProductId = manufacturing.ProductId,
                SupplierId = purchaseOrderItem.RawGoldPurchaseOrder.SupplierId,
                KaratTypeId = purchaseOrderItem.KaratTypeId,
                ConsumedWeight = manufacturing.ConsumedWeight,
                WastageWeight = manufacturing.WastageWeight,
                CostPerGram = purchaseOrderItem.UnitCostPerGram,
                TotalRawMaterialCost = manufacturing.ConsumedWeight * purchaseOrderItem.UnitCostPerGram,
                ContributionPercentage = 1.0m,
                SequenceOrder = 1
            }
        };
    }

    /// <summary>
    /// Reverse product ownership records created during manufacturing
    /// </summary>
    private async Task ReverseProductOwnershipAsync(int manufacturingId, string userId)
    {
        var ownershipRecords = await _unitOfWork.Repository<ProductOwnership>()
            .GetQueryable()
            .Where(po => po.ManufacturingRecordId == manufacturingId && po.IsActive)
            .ToListAsync();

        foreach (var ownership in ownershipRecords)
        {
            ownership.IsActive = false;
            ownership.ModifiedAt = DateTime.UtcNow;
            ownership.ModifiedBy = userId;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Reverse product inventory adjustments made during manufacturing
    /// </summary>
    private async Task ReverseProductInventoryAsync(int productId, int branchId, int quantity, string userId)
    {
        var inventory = await _unitOfWork.Repository<Inventory>()
            .GetQueryable()
            .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.BranchId == branchId && pi.IsActive);

        if (inventory != null)
        {
            inventory.QuantityOnHand = Math.Max(0, inventory.QuantityOnHand - quantity);
            inventory.ModifiedAt = DateTime.UtcNow;
            inventory.ModifiedBy = userId;

            // Create inventory movement record
            var movement = new InventoryMovement
            {
                InventoryId = inventory.Id,
                MovementType = "Manufacturing Reversal",
                QuantityChange = -quantity,
                QuantityBalance = inventory.QuantityOnHand,
                WeightChange = 0, // No weight change for quantity reversal
                WeightBalance = inventory.WeightOnHand,
                UnitCost = 0, // Will be calculated based on average cost
                ReferenceNumber = "Manufacturing Cancellation",
                Notes = $"Reversed manufacturing of {quantity} units",
                MovementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            await _unitOfWork.Repository<InventoryMovement>().AddAsync(movement);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    #endregion
}
