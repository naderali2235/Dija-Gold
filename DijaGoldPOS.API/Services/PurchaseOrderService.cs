using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.InventoryModels;
using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Models.PurchaseOrderModels;
using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PurchaseOrderService> _logger;
    private readonly IAuditService _auditService;
    private readonly ISupplierService _supplierService;
    private readonly ITreasuryService _treasuryService;
    private readonly IProductOwnershipService _productOwnershipService;

    // Define status workflow
    private static readonly Dictionary<string, List<string>> StatusTransitions = new()
    {
        ["Pending"] = new List<string> { "Sent", "Confirmed", "Cancelled" },
        ["Sent"] = new List<string> { "Confirmed", "Received", "Cancelled" },
        ["Confirmed"] = new List<string> { "Received", "Cancelled" },
        ["Received"] = new List<string> { "Completed" },
        ["Cancelled"] = new List<string>(), // Terminal state
        ["Completed"] = new List<string>() // Terminal state
    };

    public PurchaseOrderService(IUnitOfWork uow, ILogger<PurchaseOrderService> logger, IAuditService auditService, ISupplierService supplierService, ITreasuryService treasuryService, IProductOwnershipService productOwnershipService)
    {
        _uow = uow;
        _logger = logger;
        _auditService = auditService;
        _supplierService = supplierService;
        _treasuryService = treasuryService;
        _productOwnershipService = productOwnershipService;
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequestDto request, string userId)
    {
        // Skip credit validation for system suppliers (DijaGold)
        var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId);
        if (supplier != null && !supplier.IsSystemSupplier)
        {
            // Validate supplier credit limit before creating purchase order
            var creditValidation = await _supplierService.ValidateSupplierCreditAsync(request.SupplierId, request.Items.Sum(i => i.UnitCost * i.QuantityOrdered));
            //if (!creditValidation.CanPurchase)
            //{
            //    _logger.LogWarning("Purchase order creation blocked due to credit limit violation. SupplierId: {SupplierId}, Amount: {Amount}",
            //        request.SupplierId, request.Items.Sum(i => i.UnitCost * i.QuantityOrdered));

            //    throw new ValidationException($"Cannot create purchase order: {creditValidation.Message}");
            //}

            // Log credit validation warnings if any
            if (creditValidation.Warnings.Any())
            {
                foreach (var warning in creditValidation.Warnings)
                {
                    _logger.LogWarning("Credit validation warning for supplier {SupplierId}: {Warning}", request.SupplierId, warning);
                }
            }
        }
        else if (supplier?.IsSystemSupplier == true)
        {
            _logger.LogInformation("Skipping credit validation for system supplier {SupplierId}", request.SupplierId);
        }

        // Generate PO number: PO-YYYYMMDD-XXXX
        var prefix = $"PO-{DateTime.UtcNow:yyyyMMdd}-";
        var counter = await _uow.PurchaseOrders.CountAsync(po => po.PurchaseOrderNumber.StartsWith(prefix)) + 1;
        var poNumber = $"{prefix}{counter:0000}";

        var purchaseOrder = new PurchaseOrder
        {
            PurchaseOrderNumber = poNumber,
            SupplierId = request.SupplierId,
            BranchId = request.BranchId,
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Terms = request.Terms,
            Notes = request.Notes,
            Status = "Pending",
            PaymentStatus = "Unpaid"
        };

        foreach (var item in request.Items)
        {
            var lineTotal = item.UnitCost * item.QuantityOrdered;
            purchaseOrder.PurchaseOrderItems.Add(new PurchaseOrderItem
            {
                ProductId = item.ProductId,
                QuantityOrdered = item.QuantityOrdered,
                WeightOrdered = item.WeightOrdered,
                UnitCost = item.UnitCost,
                LineTotal = item.UnitCost * item.QuantityOrdered,
                Notes = item.Notes
            });
            purchaseOrder.TotalAmount += lineTotal;
        }
        purchaseOrder.OutstandingBalance = purchaseOrder.TotalAmount - purchaseOrder.AmountPaid;

        await _uow.PurchaseOrders.AddAsync(purchaseOrder);
        await _uow.SaveChangesAsync();

        // After creating the PO, update supplier balance with outstanding amount (for non-system suppliers)
        if (supplier != null && !supplier.IsSystemSupplier && purchaseOrder.OutstandingBalance > 0)
        {
            // Increase supplier liability by outstanding amount
            var updatedSupplier = await _uow.Suppliers.UpdateBalanceAsync(purchaseOrder.SupplierId, purchaseOrder.OutstandingBalance);

            // Create a supplier transaction entry for auditability
            var supplierTxn = new SupplierTransaction
            {
                // TransactionNumber will be auto-generated by repository if left empty
                SupplierId = purchaseOrder.SupplierId,
                TransactionDate = DateTime.UtcNow,
                TransactionType = "purchase_order",
                Amount = purchaseOrder.OutstandingBalance,
                BalanceAfterTransaction = updatedSupplier?.CurrentBalance ?? supplier.CurrentBalance + purchaseOrder.OutstandingBalance,
                ReferenceNumber = purchaseOrder.PurchaseOrderNumber,
                Notes = $"Created PO {purchaseOrder.PurchaseOrderNumber}",
                CreatedByUserId = userId,
                BranchId = purchaseOrder.BranchId
            };

            await _uow.Suppliers.CreateTransactionAsync(supplierTxn);
            await _uow.SaveChangesAsync();
        }

        await _auditService.LogAsync(userId, "CREATE_PURCHASE_ORDER", nameof(PurchaseOrder), purchaseOrder.Id.ToString(),
            $"Created PO {purchaseOrder.PurchaseOrderNumber}");

        return await MapToDto(purchaseOrder.Id);
    }

    public async Task<PurchaseOrderDto?> GetAsync(int id)
    {
        var po = await _uow.PurchaseOrders.GetWithItemsAsNoTrackingAsync(id);
        return po == null ? null : MapToDto(po);
    }

    public async Task<(List<PurchaseOrderDto> Items, int TotalCount)> SearchAsync(PurchaseOrderSearchRequestDto request)
    {
        var query = _uow.PurchaseOrders.GetQueryableAsNoTracking("Supplier", "Branch");

        if (!string.IsNullOrWhiteSpace(request.PurchaseOrderNumber))
            query = query.Where(po => po.PurchaseOrderNumber.Contains(request.PurchaseOrderNumber));
        if (request.SupplierId.HasValue)
            query = query.Where(po => po.SupplierId == request.SupplierId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(po => po.BranchId == request.BranchId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(po => po.Status == request.Status);
        if (request.FromDate.HasValue)
            query = query.Where(po => po.OrderDate >= request.FromDate.Value);
        if (request.ToDate.HasValue)
            query = query.Where(po => po.OrderDate <= request.ToDate.Value);

        var total = await query.CountAsync();
        var purchaseOrders = await query
            .Include(po => po.PurchaseOrderItems)
                .ThenInclude(item => item.Product)
            .OrderByDescending(po => po.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var items = purchaseOrders.Select(po => MapToDto(po)).ToList();

        return (items, total);
    }

    public async Task<bool> ReceiveAsync(ReceivePurchaseOrderRequestDto request, string userId)
    {
        try
        {
            _logger.LogInformation("Starting purchase order receipt process. PO ID: {PurchaseOrderId}, User: {UserId}", 
                request.PurchaseOrderId, userId);

            // Input validation
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Receive purchase order request cannot be null");
            
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
            if (request.Items == null || !request.Items.Any())
                throw new ValidationException("No items provided for receiving");

            return await _uow.ExecuteInTransactionAsync(async () =>
            {
                try
                {
                    // Fetch purchase order with items
                    var po = await _uow.PurchaseOrders.GetWithItemsAsync(request.PurchaseOrderId);
                    if (po == null)
                    {
                        _logger.LogWarning("Purchase order not found. PO ID: {PurchaseOrderId}", request.PurchaseOrderId);
                        throw new InvalidOperationException($"Purchase order with ID {request.PurchaseOrderId} not found");
                    }

                    if (po.Status == "Cancelled")
                    {
                        _logger.LogWarning("Cannot receive cancelled purchase order. PO: {PurchaseOrderNumber}", po.PurchaseOrderNumber);
                        throw new InvalidOperationException($"Cannot receive cancelled purchase order {po.PurchaseOrderNumber}");
                    }

                    if (po.Status == "Completed")
                    {
                        _logger.LogWarning("Purchase order already completed. PO: {PurchaseOrderNumber}", po.PurchaseOrderNumber);
                        throw new InvalidOperationException($"Purchase order {po.PurchaseOrderNumber} is already completed");
                    }

                    // Fetch supplier once for efficiency
                    var supplier = await _uow.Suppliers.GetByIdAsync(po.SupplierId);
                    if (supplier == null)
                    {
                        _logger.LogError("Supplier not found for purchase order. Supplier ID: {SupplierId}, PO: {PurchaseOrderNumber}", 
                            po.SupplierId, po.PurchaseOrderNumber);
                        throw new InvalidOperationException($"Supplier not found for purchase order {po.PurchaseOrderNumber}");
                    }

                    var processedItems = 0;
                    var errors = new List<string>();

                    // Process each received item
                    foreach (var item in request.Items)
                    {
                        try
                        {
                            // Validate received item
                            if (item.QuantityReceived < 0 || item.WeightReceived < 0)
                            {
                                errors.Add($"Invalid received quantities for item {item.PurchaseOrderItemId}");
                                continue;
                            }

                            var poItem = po.PurchaseOrderItems.FirstOrDefault(i => i.Id == item.PurchaseOrderItemId);
                            if (poItem == null)
                            {
                                errors.Add($"Purchase order item not found: {item.PurchaseOrderItemId}");
                                continue;
                            }

                            // Validate received quantities don't exceed ordered quantities
                            if (poItem.QuantityReceived + item.QuantityReceived > poItem.QuantityOrdered)
                            {
                                errors.Add($"Received quantity ({poItem.QuantityReceived + item.QuantityReceived}) exceeds ordered quantity ({poItem.QuantityOrdered}) for item {item.PurchaseOrderItemId}");
                                continue;
                            }

                            // Update purchase order item received quantities
                            poItem.QuantityReceived += item.QuantityReceived;
                            poItem.WeightReceived += item.WeightReceived;
                            poItem.Status = poItem.QuantityReceived >= poItem.QuantityOrdered ? "Received" : "Pending";

                            // Prepare inventory updates for this item
                            var (inventory, movement) = await PrepareInventoryUpdatesForReceivedItem(poItem, item, po.BranchId, userId);

                            // Prepare ownership based on supplier type
                            ProductOwnership ownership;
                            if (supplier.IsSystemSupplier)
                            {
                                // For system supplier (DijaGold), purchases go directly to company balance
                                ownership = await PrepareSystemSupplierOwnership(poItem, item, po, userId);
                            }
                            else
                            {
                                // Create ownership restriction for this item (with no payment recorded)
                                ownership = await PrepareOwnershipRestrictionForReceivedItem(poItem, item, po, userId);
                            }

                            processedItems++;
                            _logger.LogDebug("Successfully processed item {ItemId} for PO {PurchaseOrderNumber}", 
                                item.PurchaseOrderItemId, po.PurchaseOrderNumber);
                        }
                        catch (Exception itemEx)
                        {
                            _logger.LogError(itemEx, "Error processing item {ItemId} for PO {PurchaseOrderNumber}", 
                                item.PurchaseOrderItemId, po.PurchaseOrderNumber);
                            errors.Add($"Error processing item {item.PurchaseOrderItemId}: {itemEx.Message}");
                        }
                    }

                    // Check if any items were processed successfully
                    if (processedItems == 0)
                    {
                        var errorMessage = errors.Any() 
                            ? $"Failed to process any items: {string.Join("; ", errors)}"
                            : "No valid items found to process";
                        
                        _logger.LogError("No items were successfully processed for PO {PurchaseOrderNumber}. Errors: {Errors}", 
                            po.PurchaseOrderNumber, string.Join("; ", errors));
                        throw new InvalidOperationException(errorMessage);
                    }

                    // Log warnings for any failed items
                    if (errors.Any())
                    {
                        _logger.LogWarning("Some items failed to process for PO {PurchaseOrderNumber}. Errors: {Errors}", 
                            po.PurchaseOrderNumber, string.Join("; ", errors));
                    }

                    // Update purchase order status if all items are received
                    if (po.PurchaseOrderItems.All(i => i.Status == "Received"))
                    {
                        po.Status = "Received";
                        po.ActualDeliveryDate = DateTime.UtcNow;
                        _logger.LogInformation("Purchase order fully received. PO: {PurchaseOrderNumber}", po.PurchaseOrderNumber);
                    }

                    // Save all changes within the transaction
                    await _uow.SaveChangesAsync();

                    // Log successful completion
                    await _auditService.LogAsync(userId, "RECEIVE_PURCHASE_ORDER", nameof(PurchaseOrder), po.Id.ToString(),
                        $"Received {processedItems} items for PO {po.PurchaseOrderNumber}. Status: {po.Status}");

                    _logger.LogInformation("Successfully completed purchase order receipt. PO: {PurchaseOrderNumber}, Items processed: {ProcessedItems}", 
                        po.PurchaseOrderNumber, processedItems);

                    return true;
                }
                catch (Exception transactionEx)
                {
                    _logger.LogError(transactionEx, "Error occurred during purchase order receipt transaction. PO ID: {PurchaseOrderId}", 
                        request.PurchaseOrderId);
                    throw; // Re-throw to trigger transaction rollback
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive purchase order. PO ID: {PurchaseOrderId}, User: {UserId}", 
                request.PurchaseOrderId, userId);
            throw; // Re-throw with original exception details
        }
    }

    private async Task<(Inventory inventory, InventoryMovement movement)> PrepareInventoryUpdatesForReceivedItem(
        PurchaseOrderItem poItem, ReceivePurchaseOrderItemDto receivedItem, int branchId, string userId)
    {
        _logger.LogInformation("Preparing inventory updates for received item. ProductId: {ProductId}, BranchId: {BranchId}, Quantity: {Quantity}, Weight: {Weight}", 
            poItem.ProductId, branchId, receivedItem.QuantityReceived, receivedItem.WeightReceived);
            
        // Get or create inventory record
        var inventory = await _uow.Inventory.GetByProductAndBranchAsync(poItem.ProductId, branchId);
        
        if (inventory == null)
        {
            // Create new inventory record
            inventory = new Inventory
            {
                ProductId = poItem.ProductId,
                BranchId = branchId,
                QuantityOnHand = receivedItem.QuantityReceived,
                WeightOnHand = receivedItem.WeightReceived,
                ReorderPoint = 10, // Default reorder point
                MinimumStockLevel = 5, // Default minimum stock
                MaximumStockLevel = 1000, // Default maximum stock
                CreatedBy = userId
            };
            
            await _uow.Inventory.AddAsync(inventory);
            _logger.LogInformation("Prepared new inventory record for product {ProductId}", poItem.ProductId);
        }
        else
        {
            // Update existing inventory
            inventory.QuantityOnHand += receivedItem.QuantityReceived;
            inventory.WeightOnHand += receivedItem.WeightReceived;
            _uow.Inventory.Update(inventory);
            
            _logger.LogInformation("Updated existing inventory for product {ProductId}. New quantity: {Quantity}, New weight: {Weight}", 
                poItem.ProductId, inventory.QuantityOnHand, inventory.WeightOnHand);
        }

        // Prepare inventory movement record
        var movement = new InventoryMovement
        {
            Inventory = inventory, // Use navigation property instead of ID
            MovementType = "Purchase Order Receipt",
            QuantityChange = receivedItem.QuantityReceived,
            WeightChange = receivedItem.WeightReceived,
            QuantityBalance = inventory.QuantityOnHand,
            WeightBalance = inventory.WeightOnHand,
            MovementDate = DateTime.UtcNow,
            ReferenceNumber = $"PO-REC-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UnitCost = poItem.UnitCost,
            Notes = $"Received from purchase order item {poItem.Id}",
            CreatedBy = userId
        };

        await _uow.InventoryMovements.AddAsync(movement);
        
        _logger.LogInformation("Prepared inventory movement record. Movement type: {MovementType}, Reference: {ReferenceNumber}", 
            movement.MovementType, movement.ReferenceNumber);
        
        return (inventory, movement);
    }

    private async Task<ProductOwnership> PrepareOwnershipRestrictionForReceivedItem(
        PurchaseOrderItem poItem, ReceivePurchaseOrderItemDto receivedItem, PurchaseOrder po, string userId)
    {
        _logger.LogInformation("Preparing ownership restriction for received item. ProductId: {ProductId}, BranchId: {BranchId}, PO: {POId}", 
            poItem.ProductId, po.BranchId, po.Id);
            
        // Calculate the total cost for the received quantity
        var receivedCost = (receivedItem.QuantityReceived / poItem.QuantityOrdered) * poItem.LineTotal;
        
        // Check if ownership record already exists for this PO item
        var existingOwnerships = await _uow.ProductOwnership.GetByProductAndBranchAsync(poItem.ProductId, po.BranchId);
        var existingOwnership = existingOwnerships.FirstOrDefault(o => 
            o.SupplierId == po.SupplierId && 
            o.PurchaseOrderId == po.Id);

        if (existingOwnership != null)
        {
            // Update existing ownership record
            existingOwnership.TotalQuantity += receivedItem.QuantityReceived;
            existingOwnership.TotalWeight += receivedItem.WeightReceived;
            existingOwnership.TotalCost += receivedCost;
            existingOwnership.OutstandingAmount = existingOwnership.TotalCost - existingOwnership.AmountPaid;
            existingOwnership.OwnershipPercentage = existingOwnership.TotalQuantity > 0 
                ? (existingOwnership.OwnedQuantity / existingOwnership.TotalQuantity) * 100
                : 0;
            existingOwnership.ModifiedAt = DateTime.UtcNow;
            existingOwnership.ModifiedBy = userId;
            
            _uow.ProductOwnership.Update(existingOwnership);
            
            _logger.LogInformation("Updated existing ownership restriction for product {ProductId}. Total cost: {TotalCost}", 
                poItem.ProductId, existingOwnership.TotalCost);
            
            return existingOwnership;
        }
        else
        {
            // Create new ownership record
            var ownership = new ProductOwnership
            {
                ProductId = poItem.ProductId,
                BranchId = po.BranchId,
                SupplierId = po.SupplierId,
                PurchaseOrderId = po.Id,
                TotalQuantity = receivedItem.QuantityReceived,
                TotalWeight = receivedItem.WeightReceived,
                OwnedQuantity = 0, // No ownership until payment is made
                OwnedWeight = 0,   // No ownership until payment is made
                TotalCost = receivedCost,
                AmountPaid = 0,    // No payment recorded
                OutstandingAmount = receivedCost,
                OwnershipPercentage = 0, // 0% ownership since no payment made
                IsActive = true,
                CreatedBy = userId,
                ModifiedBy = userId
            };
            
            await _uow.ProductOwnership.AddAsync(ownership);
            
            _logger.LogInformation("Prepared new ownership restriction for product {ProductId}. Total cost: {TotalCost}", 
                poItem.ProductId, receivedCost);
            
            return ownership;
        }
    }

    private async Task<ProductOwnership> PrepareSystemSupplierOwnership(
        PurchaseOrderItem poItem, ReceivePurchaseOrderItemDto receivedItem, PurchaseOrder po, string userId)
    {
        _logger.LogInformation("Preparing system supplier ownership. ProductId: {ProductId}, BranchId: {BranchId}", 
            poItem.ProductId, po.BranchId);
            
        // For system supplier, create full ownership immediately
        var ownership = new ProductOwnership
        {
            ProductId = poItem.ProductId,
            BranchId = po.BranchId,
            SupplierId = po.SupplierId,
            PurchaseOrderId = po.Id,
            TotalQuantity = receivedItem.QuantityReceived,
            TotalWeight = receivedItem.WeightReceived,
            OwnedQuantity = receivedItem.QuantityReceived, // Full ownership immediately
            OwnedWeight = receivedItem.WeightReceived,     // Full ownership immediately
            TotalCost = poItem.UnitCost * receivedItem.QuantityReceived,
            AmountPaid = poItem.UnitCost * receivedItem.QuantityReceived, // Fully paid immediately
            OutstandingAmount = 0, // No outstanding amount for system supplier
            OwnershipPercentage = 100, // 100% ownership stored as 0-100 percent
            IsActive = true,
            CreatedBy = userId,
            ModifiedBy = userId,
            Notes = "System supplier purchase - full ownership granted immediately"
        };
        
        await _uow.ProductOwnership.AddAsync(ownership);
        
        _logger.LogInformation("Prepared full ownership record for system supplier product {ProductId}", poItem.ProductId);
        
        return ownership;
    }

    public async Task<PurchaseOrderDto> UpdateAsync(int id, UpdatePurchaseOrderRequestDto request, string userId)
    {
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(id);
        if (po == null)
            throw new InvalidOperationException("Purchase order not found");

        // Only allow updates if PO is in Pending status
        if (po.Status != "Pending")
            throw new InvalidOperationException("Can only update purchase orders in Pending status");

        // Update basic properties (but keep status unchanged)
        po.SupplierId = request.SupplierId;
        po.BranchId = request.BranchId;
        po.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        po.Terms = request.Terms;
        po.Notes = request.Notes;

        // Capture original outstanding to compute delta later
        var originalOutstanding = po.OutstandingBalance;

        // Process items: update existing ones and add new ones
        var existingItemIds = po.PurchaseOrderItems.Select(i => i.Id).ToHashSet();
        var updatedItemIds = new HashSet<int>();
        po.TotalAmount = 0;

        foreach (var item in request.Items)
        {
            if (item.Id.HasValue && existingItemIds.Contains(item.Id.Value))
            {
                // Update existing item - but skip if it has received quantities
                var existingItem = po.PurchaseOrderItems.First(i => i.Id == item.Id.Value);
                
                // Skip updating items that have received quantities
                if (existingItem.QuantityReceived > 0 || existingItem.WeightReceived > 0)
                {
                    // Keep the existing item as is, just add to total
                    po.TotalAmount += existingItem.LineTotal;
                    updatedItemIds.Add(item.Id.Value);
                    continue;
                }

                // Update only items that haven't been received
                existingItem.ProductId = item.ProductId;
                existingItem.QuantityOrdered = item.QuantityOrdered;
                existingItem.WeightOrdered = item.WeightOrdered;
                existingItem.UnitCost = item.UnitCost;
                existingItem.Notes = item.Notes;
                existingItem.LineTotal = item.UnitCost * item.QuantityOrdered;
                
                updatedItemIds.Add(item.Id.Value);
            }
            else
            {
                // Add new item
                var lineTotal = item.UnitCost * item.QuantityOrdered;
                po.PurchaseOrderItems.Add(new PurchaseOrderItem
                {
                    ProductId = item.ProductId,
                    QuantityOrdered = item.QuantityOrdered,
                    WeightOrdered = item.WeightOrdered,
                    UnitCost = item.UnitCost,
                    LineTotal = lineTotal,
                    Status = "Pending",
                    Notes = item.Notes
                });
            }
        }

        // Remove items that were not in the update request (but keep items with received quantities)
        var itemsToRemove = po.PurchaseOrderItems
            .Where(i => !updatedItemIds.Contains(i.Id) && i.QuantityReceived == 0 && i.WeightReceived == 0)
            .ToList();
        foreach (var item in itemsToRemove)
        {
            po.PurchaseOrderItems.Remove(item);
        }

        // Recalculate total amount including all items (received and non-received)
        po.TotalAmount = po.PurchaseOrderItems.Sum(i => i.LineTotal);
        po.OutstandingBalance = po.TotalAmount - po.AmountPaid;

        // If outstanding changed, reflect it on supplier balance for non-system suppliers
        var poSupplier = await _uow.Suppliers.GetByIdAsync(po.SupplierId);
        var delta = po.OutstandingBalance - originalOutstanding;
        if (poSupplier != null && !poSupplier.IsSystemSupplier && delta != 0)
        {
            var updatedSupplier = await _uow.Suppliers.UpdateBalanceAsync(po.SupplierId, delta);

            var supplierTxn = new SupplierTransaction
            {
                SupplierId = po.SupplierId,
                TransactionDate = DateTime.UtcNow,
                TransactionType = "purchase_order_update",
                Amount = delta,
                BalanceAfterTransaction = updatedSupplier?.CurrentBalance ?? (poSupplier.CurrentBalance + delta),
                ReferenceNumber = po.PurchaseOrderNumber,
                Notes = $"Updated PO {po.PurchaseOrderNumber}",
                CreatedByUserId = userId,
                BranchId = po.BranchId
            };

            await _uow.Suppliers.CreateTransactionAsync(supplierTxn);
        }

        await _uow.SaveChangesAsync();

        await _auditService.LogAsync(userId, "UPDATE_PURCHASE_ORDER", nameof(PurchaseOrder), po.Id.ToString(),
            $"Updated PO {po.PurchaseOrderNumber}");

        return await MapToDto(po.Id);
    }

    public async Task<PurchaseOrderDto> UpdateStatusAsync(int id, UpdatePurchaseOrderStatusRequestDto request, string userId)
    {
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(id);
        if (po == null)
            throw new InvalidOperationException("Purchase order not found");

        // Validate status transition
        if (!IsValidStatusTransition(po.Status, request.NewStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from {po.Status} to {request.NewStatus}");
        }

        // Additional validation based on status
        if (request.NewStatus == "Received" && !po.PurchaseOrderItems.Any())
        {
            throw new InvalidOperationException("Cannot mark as received: no items in purchase order");
        }

        // Handle cancellation side-effects (status update only, no deletions)
        if (request.NewStatus == "Cancelled")
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                // Reverse payments if any
                if (po.AmountPaid > 0)
                {
                    var reversalNumber = await GenerateReversalTransactionNumberAsync(po.BranchId);

                    var reversal = new FinancialTransaction
                    {
                        TransactionNumber = reversalNumber,
                        TransactionTypeId = LookupTableConstants.FinancialTransactionTypeRefund,
                        BusinessEntityId = po.SupplierId,
                        BusinessEntityTypeId = LookupTableConstants.BusinessEntityTypeSupplier,
                        BranchId = po.BranchId,
                        TransactionDate = DateTime.UtcNow,
                        Subtotal = po.AmountPaid,
                        TotalAmount = po.AmountPaid,
                        AmountPaid = po.AmountPaid,
                        PaymentMethodId = LookupTableConstants.PaymentMethodCash, // default; actual original method may vary
                        StatusId = LookupTableConstants.FinancialTransactionStatusReversed,
                        ProcessedByUserId = userId,
                        Notes = $"Reversal for PO {po.PurchaseOrderNumber} cancellation",
                        ReceiptPrinted = false,
                        GeneralLedgerPosted = false
                    };

                    await _uow.FinancialTransactions.AddAsync(reversal);

                    // Increase supplier balance back by paid amount
                    var supplierForReversal = await _uow.Suppliers.GetByIdAsync(po.SupplierId);
                    if (supplierForReversal != null)
                    {
                        supplierForReversal.CurrentBalance += po.AmountPaid;
                        supplierForReversal.LastTransactionDate = DateTime.UtcNow;
                        supplierForReversal.ModifiedAt = DateTime.UtcNow;
                        supplierForReversal.ModifiedBy = userId;
                        _uow.Suppliers.Update(supplierForReversal);
                    }

                    // Reset PO payment fields
                    po.AmountPaid = 0;
                    po.OutstandingBalance = po.TotalAmount;
                    po.PaymentStatus = "Unpaid";
                }

                // Create negative inventory movements to revert receipts (no deletions)
                foreach (var item in po.PurchaseOrderItems)
                {
                    if ((item.QuantityReceived > 0) || (item.WeightReceived > 0))
                    {
                        // Load inventory
                        var inventory = await _uow.Inventory.GetByProductAndBranchAsync(item.ProductId, po.BranchId);
                        if (inventory == null)
                        {
                            throw new InvalidOperationException($"Cannot cancel PO {po.PurchaseOrderNumber}: inventory not found for product {item.ProductId} in branch {po.BranchId}");
                        }

                        // Ensure sufficient inventory to reverse
                        if (inventory.QuantityOnHand < item.QuantityReceived || inventory.WeightOnHand < item.WeightReceived)
                        {
                            throw new InvalidOperationException($"Cannot cancel PO {po.PurchaseOrderNumber}: insufficient inventory to revert received quantities for product {item.ProductId}");
                        }

                        inventory.QuantityOnHand -= item.QuantityReceived;
                        inventory.WeightOnHand -= item.WeightReceived;
                        _uow.Inventory.Update(inventory);

                        var movement = new InventoryMovement
                        {
                            Inventory = inventory,
                            MovementType = "Purchase Order Cancellation",
                            QuantityChange = -item.QuantityReceived,
                            WeightChange = -item.WeightReceived,
                            QuantityBalance = inventory.QuantityOnHand,
                            WeightBalance = inventory.WeightOnHand,
                            MovementDate = DateTime.UtcNow,
                            ReferenceNumber = $"PO-CAN-{po.PurchaseOrderNumber}",
                            UnitCost = item.UnitCost,
                            Notes = $"Reversal of received quantities for PO item {item.Id}",
                            CreatedBy = userId
                        };
                        await _uow.InventoryMovements.AddAsync(movement);
                    }
                }

                // Adjust supplier balance by removing any remaining outstanding for this PO (non-system suppliers)
                var supplierForOutstanding = await _uow.Suppliers.GetByIdAsync(po.SupplierId);
                if (supplierForOutstanding != null && !supplierForOutstanding.IsSystemSupplier && po.OutstandingBalance > 0)
                {
                    var amountToReverse = po.OutstandingBalance;
                    var updatedSupplier = await _uow.Suppliers.UpdateBalanceAsync(po.SupplierId, -amountToReverse);

                    var supplierTxn = new SupplierTransaction
                    {
                        SupplierId = po.SupplierId,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = "purchase_order_cancel",
                        Amount = -amountToReverse,
                        BalanceAfterTransaction = updatedSupplier?.CurrentBalance ?? (supplierForOutstanding.CurrentBalance - amountToReverse),
                        ReferenceNumber = po.PurchaseOrderNumber,
                        Notes = $"Cancelled PO {po.PurchaseOrderNumber}",
                        CreatedByUserId = userId,
                        BranchId = po.BranchId
                    };

                    await _uow.Suppliers.CreateTransactionAsync(supplierTxn);

                    // Zero out the PO outstanding since it's cancelled
                    po.OutstandingBalance = 0;
                    po.PaymentStatus = "Unpaid";
                }

                // Persist before updating status
                await _uow.SaveChangesAsync();
            });
        }

        // Update status
        var oldStatus = po.Status;
        po.Status = request.NewStatus;

        // Set actual delivery date when received
        if (request.NewStatus == "Received")
        {
            po.ActualDeliveryDate = DateTime.UtcNow;
        }

        // Add status notes to general notes if provided
        if (!string.IsNullOrWhiteSpace(request.StatusNotes))
        {
            po.Notes = string.IsNullOrWhiteSpace(po.Notes) 
                ? $"Status changed to {request.NewStatus}: {request.StatusNotes}"
                : $"{po.Notes}\nStatus changed to {request.NewStatus}: {request.StatusNotes}";
        }

        await _uow.SaveChangesAsync();

        await _auditService.LogAsync(userId, "UPDATE_PURCHASE_ORDER_STATUS", nameof(PurchaseOrder), po.Id.ToString(),
            $"Status changed from {oldStatus} to {request.NewStatus}");

        return await MapToDto(po.Id);
    }

    public async Task<PurchaseOrderStatusTransitionDto> GetAvailableStatusTransitionsAsync(int id)
    {
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(id);
        if (po == null)
            throw new InvalidOperationException("Purchase order not found");

        var availableTransitions = StatusTransitions.ContainsKey(po.Status) 
            ? StatusTransitions[po.Status] 
            : new List<string>();

        // Additional validation for specific transitions
        var validationMessage = "";
        if (po.Status == "Pending" && !po.PurchaseOrderItems.Any())
        {
            validationMessage = "Cannot send purchase order: no items added";
            availableTransitions = availableTransitions.Where(s => s != "Sent").ToList();
        }

        if (po.Status == "Sent" && po.PurchaseOrderItems.All(i => i.QuantityReceived == 0 && i.WeightReceived == 0))
        {
            validationMessage = "Cannot mark as received: no items have been received";
            availableTransitions = availableTransitions.Where(s => s != "Received").ToList();
        }

        return new PurchaseOrderStatusTransitionDto
        {
            CurrentStatus = po.Status,
            AvailableTransitions = availableTransitions,
            ValidationMessage = validationMessage
        };
    }

    private static bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        return StatusTransitions.ContainsKey(currentStatus) && 
               StatusTransitions[currentStatus].Contains(newStatus);
    }

    private async Task<PurchaseOrderDto> MapToDto(int id)
    {
        var po = await _uow.PurchaseOrders.GetWithItemsAsNoTrackingAsync(id) ?? throw new InvalidOperationException("PO not found");
        return MapToDto(po);
    }

    public async Task<PurchaseOrderPaymentResult> ProcessPaymentAsync(ProcessPurchaseOrderPaymentRequestDto request, string userId)
    {
        // Top-level validation and logging for clearer diagnostics
        if (request == null)
        {
            _logger.LogWarning("ProcessPaymentAsync invoked with null request by user {UserId}", userId);
            return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Payment request cannot be null" };
        }
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("ProcessPaymentAsync invoked with empty user id. PO: {PurchaseOrderId}", request.PurchaseOrderId);
            return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Invalid user context" };
        }

        try
        {
            return await _uow.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("Starting payment processing. PO ID: {PurchaseOrderId}, Amount: {Amount}, Method: {MethodId}, User: {UserId}",
                    request.PurchaseOrderId, request.PaymentAmount, request.PaymentMethodId, userId);

                // Load PO and validate existence
                var po = await _uow.PurchaseOrders.GetWithItemsAsync(request.PurchaseOrderId);
                if (po == null)
                {
                    _logger.LogWarning("Purchase order not found for payment. PO ID: {PurchaseOrderId}", request.PurchaseOrderId);
                    return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Purchase order not found" };
                }

                // Disallow payments for system supplier purchases
                var supplier = await _uow.Suppliers.GetByIdAsync(po.SupplierId);
                if (supplier?.IsSystemSupplier == true)
                {
                    _logger.LogWarning("Payment attempt on system supplier PO {PONumber} blocked", po.PurchaseOrderNumber);
                    return new PurchaseOrderPaymentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Cannot process payments for system supplier. System supplier purchases are automatically fully owned."
                    };
                }

                // Validate payment method and amount
                if (request.PaymentMethodId <= 0)
                {
                    return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Invalid payment method" };
                }
                if (request.PaymentAmount <= 0)
                {
                    return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Payment amount must be greater than zero" };
                }
                if (request.PaymentAmount > po.OutstandingBalance)
                {
                    return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Payment amount cannot exceed outstanding balance" };
                }
                if (po.Status == "Cancelled")
                {
                    return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = $"Cannot pay a cancelled purchase order ({po.PurchaseOrderNumber})" };
                }

                // Ensure treasury has sufficient funds before proceeding
                var treBalance = await _treasuryService.GetBalanceAsync(po.BranchId);
                if (treBalance < request.PaymentAmount)
                {
                    _logger.LogWarning("Insufficient treasury balance for PO payment. Branch: {BranchId}, Required: {Amount}, Balance: {Balance}", po.BranchId, request.PaymentAmount, treBalance);
                    return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Insufficient treasury balance" };
                }

                // Pay supplier via treasury; this debits treasury and records supplier transaction
                var noteSuffix = string.IsNullOrWhiteSpace(request.Notes) ? string.Empty : $" - {request.Notes}";
                var referenceInfo = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? string.Empty : $" [Ref: {request.ReferenceNumber}]";
                var payNotes = $"PO {po.PurchaseOrderNumber}{referenceInfo}{noteSuffix}";

                var (_, supplierTxn) = await _treasuryService.PaySupplierAsync(po.BranchId, po.SupplierId, request.PaymentAmount, userId, payNotes);

                // Update purchase order payment information
                po.AmountPaid += request.PaymentAmount;
                po.OutstandingBalance = po.TotalAmount - po.AmountPaid;
                po.PaymentStatus = po.OutstandingBalance == 0 ? "Paid" : "Partial";

                // Release ownership restrictions proportionally to payment amount
                await ReleaseOwnershipRestrictions(po, request.PaymentAmount, userId);

                await _uow.SaveChangesAsync();

                await _auditService.LogAsync(userId, "PROCESS_PAYMENT", nameof(PurchaseOrder), po.Id.ToString(),
                    $"Processed payment of {request.PaymentAmount:C} for PO {po.PurchaseOrderNumber}");

                _logger.LogInformation("Payment processed via treasury successfully. PO: {PONumber}, SupplierTxn: {Txn}", po.PurchaseOrderNumber, supplierTxn.TransactionNumber);

                return new PurchaseOrderPaymentResult
                {
                    IsSuccess = true,
                    PurchaseOrder = MapToDto(po),
                    AmountPaid = request.PaymentAmount,
                    OutstandingAmount = po.OutstandingBalance,
                    TransactionNumber = supplierTxn.TransactionNumber
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ProcessPaymentAsync for PO {PurchaseOrderId}", request.PurchaseOrderId);
            return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<List<PurchaseOrderDto>> GetOverduePurchaseOrdersAsync(int? branchId = null)
    {
        var query = _uow.PurchaseOrders.GetQueryableAsNoTracking("Supplier", "Branch");

        if (branchId.HasValue)
            query = query.Where(po => po.BranchId == branchId.Value);

        // Get POs that are overdue (ExpectedDeliveryDate passed and not completed)
        var overduePos = await query
            .Where(po => po.ExpectedDeliveryDate < DateTime.UtcNow &&
                        po.Status != "Completed" &&
                        po.Status != "Cancelled")
            .Include(po => po.PurchaseOrderItems)
                .ThenInclude(item => item.Product)
            .OrderByDescending(po => po.ExpectedDeliveryDate)
            .ToListAsync();

        return overduePos.Select(po => MapToDto(po)).ToList();
    }

    public async Task<decimal> GetTotalOutstandingAmountAsync(int? supplierId = null, int? branchId = null)
    {
        var query = _uow.PurchaseOrders.GetQueryableAsNoTracking();

        if (supplierId.HasValue)
            query = query.Where(po => po.SupplierId == supplierId.Value);

        if (branchId.HasValue)
            query = query.Where(po => po.BranchId == branchId.Value);

        return await query.SumAsync(po => po.OutstandingBalance);
    }

    private async Task ReleaseOwnershipRestrictions(PurchaseOrder po, decimal paymentAmount, string userId)
    {
        _logger.LogInformation("Releasing ownership restrictions for PO {PurchaseOrderId} with payment {PaymentAmount}",
            po.Id, paymentAmount);

        // Get all ownership records for this purchase order
        var ownershipRecords = await _uow.ProductOwnership.GetByPurchaseOrderAsync(po.Id);

        if (!ownershipRecords.Any())
            return;

        // Calculate total outstanding amount for this purchase order
        var totalOutstanding = ownershipRecords.Sum(o => o.OutstandingAmount);
        
        if (totalOutstanding <= 0)
            return;

        foreach (var ownership in ownershipRecords)
        {
            // Calculate proportional payment amount based on this ownership's share of total outstanding
            var proportionalPayment = paymentAmount * (ownership.OutstandingAmount / totalOutstanding);
            
            // Use the existing service method to update ownership
            await _productOwnershipService.UpdateOwnershipAfterPaymentAsync(
                ownership.Id,
                proportionalPayment,
                po.PurchaseOrderNumber,
                userId);

            _logger.LogInformation("Updated ownership for product {ProductId} with proportional payment {ProportionalPayment}",
                ownership.ProductId, proportionalPayment);
        }
    }

    private async Task UpdateSupplierBalance(int supplierId, decimal paymentAmount, string userId)
    {
        _logger.LogInformation("Updating supplier balance for supplier {SupplierId}: reducing by {PaymentAmount}",
            supplierId, paymentAmount);

        var supplier = await _uow.Suppliers.GetByIdAsync(supplierId);
        if (supplier == null)
        {
            _logger.LogWarning("Supplier {SupplierId} not found when updating balance", supplierId);
            return;
        }

        supplier.CurrentBalance -= paymentAmount;
        supplier.LastTransactionDate = DateTime.UtcNow;
        supplier.ModifiedAt = DateTime.UtcNow;
        supplier.ModifiedBy = userId;

        _uow.Suppliers.Update(supplier);

        _logger.LogInformation("Updated supplier {SupplierId} balance to {CurrentBalance}",
            supplierId, supplier.CurrentBalance);
    }

    private async Task<string> GenerateTransactionNumberAsync(int branchId)
    {
        // Generate transaction number: PO-PMT-YYYYMMDD-BRANCH-XXXX
        var prefix = $"PO-PMT-{DateTime.UtcNow:yyyyMMdd}-{branchId:00}-";
        var counter = await _uow.FinancialTransactions.CountAsync(ft => ft.TransactionNumber.StartsWith(prefix)) + 1;
        return $"{prefix}{counter:0000}";
    }

    private async Task<string> GenerateReversalTransactionNumberAsync(int branchId)
    {
        // Generate transaction number: PO-RVR-YYYYMMDD-BRANCH-XXXX
        var prefix = $"PO-RVR-{DateTime.UtcNow:yyyyMMdd}-{branchId:00}-";
        var counter = await _uow.FinancialTransactions.CountAsync(ft => ft.TransactionNumber.StartsWith(prefix)) + 1;
        return $"{prefix}{counter:0000}";
    }


    public static PurchaseOrderDto MapToDto(PurchaseOrder po)
    {
        var availableStatuses = StatusTransitions.ContainsKey(po.Status)
            ? StatusTransitions[po.Status]
            : new List<string>();

        return new PurchaseOrderDto
        {
            Id = po.Id,
            PurchaseOrderNumber = po.PurchaseOrderNumber,
            SupplierId = po.SupplierId,
            SupplierName = po.Supplier?.CompanyName,
            BranchId = po.BranchId,
            BranchName = po.Branch?.Name,
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            ActualDeliveryDate = po.ActualDeliveryDate,
            TotalAmount = po.TotalAmount,
            AmountPaid = po.AmountPaid,
            OutstandingBalance = po.OutstandingBalance,
            Status = po.Status,
            PaymentStatus = po.PaymentStatus,
            Terms = po.Terms,
            Notes = po.Notes,
            AvailableStatuses = availableStatuses,
            Items = po.PurchaseOrderItems.Select(i => new PurchaseOrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductCode = i.Product?.ProductCode,
                ProductName = i.Product?.Name,
                QuantityOrdered = i.QuantityOrdered,
                QuantityReceived = i.QuantityReceived,
                WeightOrdered = i.WeightOrdered,
                WeightReceived = i.WeightReceived,
                UnitCost = i.UnitCost,
                LineTotal = i.LineTotal,
                Status = i.Status,
                Notes = i.Notes,
                IsReceived = i.QuantityReceived > 0 || i.WeightReceived > 0,
                CanEdit = po.Status == "Pending" && (i.QuantityReceived == 0 && i.WeightReceived == 0)
            }).ToList()
        };
    }
}
