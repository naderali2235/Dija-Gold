using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;
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

    public PurchaseOrderService(IUnitOfWork uow, ILogger<PurchaseOrderService> logger, IAuditService auditService, ISupplierService supplierService)
    {
        _uow = uow;
        _logger = logger;
        _auditService = auditService;
        _supplierService = supplierService;
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequestDto request, string userId)
    {
        // Validate supplier credit limit before creating purchase order
        var creditValidation = await _supplierService.ValidateSupplierCreditAsync(request.SupplierId, request.Items.Sum(i => i.UnitCost * i.QuantityOrdered));
        if (!creditValidation.CanPurchase)
        {
            _logger.LogWarning("Purchase order creation blocked due to credit limit violation. SupplierId: {SupplierId}, Amount: {Amount}",
                request.SupplierId, request.Items.Sum(i => i.UnitCost * i.QuantityOrdered));

            throw new ValidationException($"Cannot create purchase order: {creditValidation.Message}");
        }

        // Log credit validation warnings if any
        if (creditValidation.Warnings.Any())
        {
            foreach (var warning in creditValidation.Warnings)
            {
                _logger.LogWarning("Credit validation warning for supplier {SupplierId}: {Warning}", request.SupplierId, warning);
            }
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
                LineTotal = lineTotal,
                Status = "Pending",
                Notes = item.Notes
            });
            purchaseOrder.TotalAmount += lineTotal;
        }
        purchaseOrder.OutstandingBalance = purchaseOrder.TotalAmount - purchaseOrder.AmountPaid;

        await _uow.PurchaseOrders.AddAsync(purchaseOrder);
        await _uow.SaveChangesAsync();

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
        return await _uow.ExecuteInTransactionAsync(async () =>
        {
            var po = await _uow.PurchaseOrders.GetWithItemsAsync(request.PurchaseOrderId);
            if (po == null || po.Status == "Cancelled") return false;

            foreach (var item in request.Items)
            {
                var poItem = po.PurchaseOrderItems.FirstOrDefault(i => i.Id == item.PurchaseOrderItemId);
                if (poItem == null) continue;

                // Update purchase order item received quantities
                poItem.QuantityReceived += item.QuantityReceived;
                poItem.WeightReceived += item.WeightReceived;
                poItem.Status = poItem.QuantityReceived >= poItem.QuantityOrdered ? "Received" : "Pending";

                // Update inventory for this item
                await UpdateInventoryForReceivedItem(poItem, item, po.BranchId, userId);

                // Create ownership restriction for this item (with no payment recorded)
                await CreateOwnershipRestrictionForReceivedItem(poItem, item, po, userId);
            }

            if (po.PurchaseOrderItems.All(i => i.Status == "Received"))
            {
                po.Status = "Received";
                po.ActualDeliveryDate = DateTime.UtcNow;
            }

            await _uow.SaveChangesAsync();

            await _auditService.LogAsync(userId, "RECEIVE_PURCHASE_ORDER", nameof(PurchaseOrder), po.Id.ToString(),
                $"Received items for PO {po.PurchaseOrderNumber}");

            return true;
        });
    }

    private async Task UpdateInventoryForReceivedItem(PurchaseOrderItem poItem, ReceivePurchaseOrderItemDto receivedItem, int branchId, string userId)
    {
        _logger.LogInformation("Updating inventory for received item. ProductId: {ProductId}, BranchId: {BranchId}, Quantity: {Quantity}, Weight: {Weight}", 
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
        }
        else
        {
            // Update existing inventory
            inventory.QuantityOnHand += receivedItem.QuantityReceived;
            inventory.WeightOnHand += receivedItem.WeightReceived;
            _uow.Inventory.Update(inventory);
        }

        // Create inventory movement record
        var movement = new InventoryMovement
        {
            InventoryId = inventory.Id,
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
        
        _logger.LogInformation("Successfully updated inventory for product {ProductId}. New quantity: {Quantity}, New weight: {Weight}", 
            poItem.ProductId, inventory.QuantityOnHand, inventory.WeightOnHand);
    }

    private async Task CreateOwnershipRestrictionForReceivedItem(PurchaseOrderItem poItem, ReceivePurchaseOrderItemDto receivedItem, PurchaseOrder po, string userId)
    {
        _logger.LogInformation("Creating ownership restriction for received item. ProductId: {ProductId}, BranchId: {BranchId}, PO: {POId}", 
            poItem.ProductId, po.BranchId, po.Id);
            
        // Calculate the total cost for the received quantity
        var receivedCost = (receivedItem.QuantityReceived / poItem.QuantityOrdered) * poItem.LineTotal;
        
        // Create ownership restriction with no payment recorded
        var ownershipRequest = new ProductOwnershipRequest
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
            AmountPaid = 0     // No payment recorded
        };

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
        }
        
        _logger.LogInformation("Successfully created ownership restriction for product {ProductId}. Total cost: {TotalCost}, Outstanding: {Outstanding}", 
            poItem.ProductId, receivedCost, receivedCost);
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

        if (request.NewStatus == "Cancelled" && po.Status == "Received")
        {
            throw new InvalidOperationException("Cannot cancel a received purchase order");
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
        return await _uow.ExecuteInTransactionAsync(async () =>
        {
            var po = await _uow.PurchaseOrders.GetWithItemsAsync(request.PurchaseOrderId);
            if (po == null)
                return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Purchase order not found" };

            if (request.PaymentAmount <= 0)
                return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Payment amount must be greater than zero" };

            if (request.PaymentAmount > po.OutstandingBalance)
                return new PurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Payment amount cannot exceed outstanding balance" };

            // Create financial transaction for the payment
            var transactionNumber = await GenerateTransactionNumberAsync(po.BranchId);

            var financialTransaction = new FinancialTransaction
            {
                TransactionNumber = transactionNumber,
                TransactionTypeId = LookupTableConstants.FinancialTransactionTypeRefund,
                BusinessEntityId = po.SupplierId,
                BusinessEntityTypeId = LookupTableConstants.BusinessEntityTypeSupplier,
                BranchId = po.BranchId,
                TransactionDate = DateTime.UtcNow,
                Subtotal = request.PaymentAmount,
                TotalAmount = request.PaymentAmount,
                AmountPaid = request.PaymentAmount,
                PaymentMethodId = request.PaymentMethodId,
                StatusId = LookupTableConstants.FinancialTransactionStatusCompleted,
                ProcessedByUserId = userId,
                Notes = $"Payment for PO {po.PurchaseOrderNumber}: {request.Notes}",
                ReceiptPrinted = false,
                GeneralLedgerPosted = false
            };

            await _uow.FinancialTransactions.AddAsync(financialTransaction);

            // Update purchase order payment information
            po.AmountPaid += request.PaymentAmount;
            po.OutstandingBalance = po.TotalAmount - po.AmountPaid;

            // Update payment status
            po.PaymentStatus = po.OutstandingBalance == 0 ? "Paid" : "Partial";

            // Release ownership restrictions proportionally to payment amount
            await ReleaseOwnershipRestrictions(po, request.PaymentAmount, userId);

            // Update supplier balance
            await UpdateSupplierBalance(po.SupplierId, request.PaymentAmount, userId);

            await _uow.SaveChangesAsync();

            await _auditService.LogAsync(userId, "PROCESS_PAYMENT", nameof(PurchaseOrder), po.Id.ToString(),
                $"Processed payment of {request.PaymentAmount:C} for PO {po.PurchaseOrderNumber}");

            return new PurchaseOrderPaymentResult
            {
                IsSuccess = true,
                PurchaseOrder = MapToDto(po),
                AmountPaid = request.PaymentAmount,
                OutstandingAmount = po.OutstandingBalance,
                TransactionNumber = transactionNumber
            };
        });
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

        // Calculate the payment percentage
        var paymentPercentage = po.TotalAmount > 0 ? (paymentAmount / po.TotalAmount) * 100 : 0;

        // Get all ownership records for this purchase order
        var ownershipRecords = await _uow.ProductOwnership.GetByPurchaseOrderAsync(po.Id);

        foreach (var ownership in ownershipRecords)
        {
            // Calculate how much ownership to release based on payment percentage
            var ownershipToRelease = (paymentPercentage / 100) * ownership.TotalQuantity;
            var weightToRelease = (paymentPercentage / 100) * ownership.TotalWeight;

            // Update ownership record
            ownership.OwnedQuantity = Math.Min(ownership.OwnedQuantity + ownershipToRelease, ownership.TotalQuantity);
            ownership.OwnedWeight = Math.Min(ownership.OwnedWeight + weightToRelease, ownership.TotalWeight);
            ownership.AmountPaid += (paymentAmount * (ownership.TotalCost / po.TotalAmount));
            ownership.OutstandingAmount = ownership.TotalCost - ownership.AmountPaid;
            ownership.OwnershipPercentage = ownership.TotalQuantity > 0 ? (ownership.OwnedQuantity / ownership.TotalQuantity) * 100 : 0;
            ownership.ModifiedAt = DateTime.UtcNow;
            ownership.ModifiedBy = userId;

            _uow.ProductOwnership.Update(ownership);

            _logger.LogInformation("Updated ownership for product {ProductId}: Owned {OwnedQuantity}/{TotalQuantity} ({OwnershipPercentage}%)",
                ownership.ProductId, ownership.OwnedQuantity, ownership.TotalQuantity, ownership.OwnershipPercentage);
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


