using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DijaGoldPOS.API.Shared;
using System.Linq;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using Microsoft.Extensions.Logging;
using DijaGoldPOS.API.Models.PurchaseOrderModels;
using DijaGoldPOS.API.Models.ManfacturingModels;

namespace DijaGoldPOS.API.Services;


public class RawGoldPurchaseOrderService : IRawGoldPurchaseOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _uow;
    private readonly ITreasuryService _treasuryService;
    private readonly IProductOwnershipService _productOwnershipService;
    private readonly ILogger<RawGoldPurchaseOrderService> _logger;

    public RawGoldPurchaseOrderService(
        ApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IUnitOfWork uow,
        ITreasuryService treasuryService,
        IProductOwnershipService productOwnershipService,
        ILogger<RawGoldPurchaseOrderService> logger)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _uow = uow;
        _treasuryService = treasuryService;
        _productOwnershipService = productOwnershipService;
        _logger = logger;
    }

    public async Task<IEnumerable<RawGoldPurchaseOrderDto>> GetAllAsync()
    {
        var purchaseOrders = await _context.RawGoldPurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .Include(po => po.RawGoldPurchaseOrderItems)
                .ThenInclude(item => item.KaratType)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<RawGoldPurchaseOrderDto>>(purchaseOrders);
    }

    public async Task<RawGoldPurchaseOrderDto?> GetByIdAsync(int id)
    {
        var purchaseOrder = await _context.RawGoldPurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .Include(po => po.RawGoldPurchaseOrderItems)
                .ThenInclude(item => item.KaratType)
            .FirstOrDefaultAsync(po => po.Id == id);

        return purchaseOrder == null ? null : _mapper.Map<RawGoldPurchaseOrderDto>(purchaseOrder);
    }

    public async Task<RawGoldPurchaseOrderDto> CreateAsync(CreateRawGoldPurchaseOrderDto createDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Generate purchase order number
            var orderNumber = await GeneratePurchaseOrderNumberAsync();

            // Check if this is the system supplier
            var supplier = await _context.Suppliers.FindAsync(createDto.SupplierId);
            bool isSystemSupplier = supplier?.CompanyName == "DijaGold POS System";

            var purchaseOrder = new RawGoldPurchaseOrder
            {
                PurchaseOrderNumber = orderNumber,
                SupplierId = createDto.SupplierId,
                BranchId = createDto.BranchId,
                OrderDate = createDto.OrderDate,
                ExpectedDeliveryDate = createDto.ExpectedDeliveryDate,
                Status = isSystemSupplier ? "Received" : "Pending",
                PaymentStatus = isSystemSupplier ? "Paid" : "Unpaid",
                ActualDeliveryDate = isSystemSupplier ? createDto.OrderDate : null,
                Notes = createDto.Notes,
                CreatedBy = _currentUserService.UserId ?? "system"
            };

            _context.RawGoldPurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            // Add purchase order items
            decimal totalAmount = 0;
            decimal totalWeightOrdered = 0;
            foreach (var itemDto in createDto.Items)
            {
                var lineTotal = itemDto.WeightOrdered * itemDto.UnitCostPerGram;
                totalAmount += lineTotal;
                totalWeightOrdered += itemDto.WeightOrdered;

                var item = new RawGoldPurchaseOrderItem
                {
                    RawGoldPurchaseOrderId = purchaseOrder.Id,
                    KaratTypeId = itemDto.KaratTypeId,
                    WeightOrdered = itemDto.WeightOrdered,
                    WeightReceived = isSystemSupplier ? itemDto.WeightOrdered : 0,
                    AvailableWeightForManufacturing = isSystemSupplier ? itemDto.WeightOrdered : 0,
                    UnitCostPerGram = itemDto.UnitCostPerGram,
                    LineTotal = lineTotal,
                    Description = itemDto.Description,
                    Notes = itemDto.Notes,
                    Status = isSystemSupplier ? "Received" : "Pending",
                    CreatedBy = _currentUserService.UserId ?? "system"
                };

                _context.RawGoldPurchaseOrderItems.Add(item);
            }

            purchaseOrder.TotalAmount = totalAmount;
            purchaseOrder.TotalWeightOrdered = totalWeightOrdered;
            // Set initial total weight received (auto-received for system supplier; otherwise 0)
            purchaseOrder.TotalWeightReceived = isSystemSupplier ? totalWeightOrdered : 0;
            
            // For system supplier, automatically mark as paid
            if (isSystemSupplier)
            {
                purchaseOrder.AmountPaid = totalAmount;
                purchaseOrder.OutstandingBalance = 0;
                purchaseOrder.PaymentStatus = "Paid";
            }
            else
            {
                purchaseOrder.AmountPaid = 0;
                purchaseOrder.OutstandingBalance = totalAmount;
                purchaseOrder.PaymentStatus = "Unpaid";
            }

            await _context.SaveChangesAsync();
            
            // Update supplier A/P balance for non-system suppliers to reflect liability
            if (!isSystemSupplier && supplier != null)
            {
                // Increase the amount we owe to the supplier by the PO total
                supplier.CurrentBalance += totalAmount;
                supplier.LastTransactionDate = DateTime.UtcNow;
                supplier.ModifiedAt = DateTime.UtcNow;
                supplier.ModifiedBy = _currentUserService.UserId ?? "system";
                _context.Suppliers.Update(supplier);
                await _context.SaveChangesAsync();
            }
            
            // For system supplier, automatically update inventory after items are saved
            if (isSystemSupplier)
            {
                var savedItems = await _context.RawGoldPurchaseOrderItems
                    .Where(item => item.RawGoldPurchaseOrderId == purchaseOrder.Id)
                    .ToListAsync();
                
                foreach (var item in savedItems)
                {
                    // Set the navigation property for inventory methods
                    item.RawGoldPurchaseOrder = purchaseOrder;
                    await UpdateRawGoldInventoryAsync(item, item.WeightReceived);
                    await CreateInventoryMovementAsync(item, item.WeightReceived);
                }
                
                await _context.SaveChangesAsync();

                // For system supplier, auto-complete PO and release reservations so stock becomes available immediately
                purchaseOrder.Status = "Completed";
                foreach (var item in savedItems)
                {
                    var inventory = await _context.RawGoldInventories
                        .FirstOrDefaultAsync(rgi => rgi.KaratTypeId == item.KaratTypeId && rgi.BranchId == purchaseOrder.BranchId);
                    if (inventory != null)
                    {
                        inventory.WeightReserved = Math.Max(0, inventory.WeightReserved - item.WeightReceived);
                        inventory.ModifiedBy = _currentUserService.UserId ?? "system";
                        inventory.ModifiedAt = DateTime.UtcNow;
                        _context.RawGoldInventories.Update(inventory);
                    }
                }
                _context.RawGoldPurchaseOrders.Update(purchaseOrder);
                await _context.SaveChangesAsync();
            }
            
            await transaction.CommitAsync();

                return await GetByIdAsync(purchaseOrder.Id) ?? throw new InvalidOperationException("Failed to retrieve created purchase order");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<RawGoldPurchaseOrderDto> UpdateAsync(int id, UpdateRawGoldPurchaseOrderDto updateDto)
    {
        var purchaseOrder = await _context.RawGoldPurchaseOrders
            .Include(po => po.RawGoldPurchaseOrderItems)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (purchaseOrder == null)
            throw new KeyNotFoundException($"Raw gold purchase order with ID {id} not found");

        if (purchaseOrder.Status == "Received")
            throw new InvalidOperationException("Cannot update a received purchase order");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Check if this is the system supplier
            var supplier = await _context.Suppliers.FindAsync(purchaseOrder.SupplierId);
            bool isSystemSupplier = supplier?.CompanyName == "DijaGold POS System";

            // Update purchase order properties
            purchaseOrder.ExpectedDeliveryDate = updateDto.ExpectedDeliveryDate;
            purchaseOrder.Notes = updateDto.Notes;
            purchaseOrder.ModifiedBy = _currentUserService.UserId ?? "system";
            purchaseOrder.ModifiedAt = DateTime.UtcNow;

            // Remove existing items and add new ones
            _context.RawGoldPurchaseOrderItems.RemoveRange(purchaseOrder.RawGoldPurchaseOrderItems);

            decimal totalAmount = 0;
            decimal totalWeightOrdered = 0;
            foreach (var itemDto in updateDto.Items)
            {
                var lineTotal = itemDto.WeightOrdered * itemDto.UnitCostPerGram;
                totalAmount += lineTotal;
                totalWeightOrdered += itemDto.WeightOrdered;

                var item = new RawGoldPurchaseOrderItem
                {
                    RawGoldPurchaseOrderId = purchaseOrder.Id,
                    KaratTypeId = itemDto.KaratTypeId,
                    WeightOrdered = itemDto.WeightOrdered,
                    UnitCostPerGram = itemDto.UnitCostPerGram,
                    LineTotal = lineTotal,
                    Description = itemDto.Description,
                    Notes = itemDto.Notes,
                    Status = "Pending",
                    CreatedBy = _currentUserService.UserId ?? "system"
                };

                _context.RawGoldPurchaseOrderItems.Add(item);
            }

            purchaseOrder.TotalAmount = totalAmount;
            purchaseOrder.TotalWeightOrdered = totalWeightOrdered;
            
            // For system supplier, automatically mark as paid
            if (isSystemSupplier)
            {
                purchaseOrder.AmountPaid = totalAmount;
                purchaseOrder.OutstandingBalance = 0;
                purchaseOrder.PaymentStatus = "Paid";
            }
            else
            {
                purchaseOrder.OutstandingBalance = totalAmount - purchaseOrder.AmountPaid;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated purchase order");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var purchaseOrder = await _context.RawGoldPurchaseOrders
            .Include(po => po.RawGoldPurchaseOrderItems)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (purchaseOrder == null)
            return false;

        if (purchaseOrder.Status == "Received")
            throw new InvalidOperationException("Cannot delete a received purchase order");

        // Disallow delete when any item has been consumed in manufacturing
        var itemIds = purchaseOrder.RawGoldPurchaseOrderItems.Select(i => i.Id).ToList();
        if (itemIds.Count > 0)
        {
            var hasManufacturingConsumption = await _context.Set<ProductManufactureRawMaterial>()
                .AnyAsync(rm => itemIds.Contains(rm.RawGoldPurchaseOrderItemId) && rm.IsActive);

            if (hasManufacturingConsumption)
            {
                throw new InvalidOperationException(
                    $"Cannot delete raw gold purchase order {purchaseOrder.PurchaseOrderNumber}: one or more items have been consumed in manufacturing.");
            }
        }

        purchaseOrder.IsActive = false;
        purchaseOrder.ModifiedBy = _currentUserService.UserId ?? "system";
        purchaseOrder.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RawGoldPurchaseOrderDto> ReceiveRawGoldAsync(int id, ReceiveRawGoldDto receiveDto)
    {
        var purchaseOrder = await _context.RawGoldPurchaseOrders
            .Include(po => po.RawGoldPurchaseOrderItems)
                .ThenInclude(item => item.KaratType)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (purchaseOrder == null)
            throw new KeyNotFoundException($"Raw gold purchase order with ID {id} not found");

        if (purchaseOrder.Status == "Received")
            throw new InvalidOperationException("Purchase order has already been received");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var receiveItem in receiveDto.Items)
            {
                var orderItem = purchaseOrder.RawGoldPurchaseOrderItems
                    .FirstOrDefault(item => item.Id == receiveItem.RawGoldPurchaseOrderItemId);

                if (orderItem == null)
                    throw new KeyNotFoundException($"Purchase order item with ID {receiveItem.RawGoldPurchaseOrderItemId} not found");

                // Update received weight
                orderItem.WeightReceived = receiveItem.WeightReceived;
                orderItem.AvailableWeightForManufacturing = receiveItem.WeightReceived;
                orderItem.Status = "Received";
                orderItem.ModifiedBy = _currentUserService.UserId ?? "system";
                orderItem.ModifiedAt = DateTime.UtcNow;

                // Update or create raw gold inventory
                await UpdateRawGoldInventoryAsync(orderItem, receiveItem.WeightReceived);

                // Create inventory movement record
                await CreateInventoryMovementAsync(orderItem, receiveItem.WeightReceived);

                // Create or update raw gold ownership record
                await CreateOrUpdateRawGoldOwnershipAsync(orderItem, _currentUserService.UserId ?? "system");
            }

            // Update purchase order status
            purchaseOrder.Status = "Received";
            purchaseOrder.ActualDeliveryDate = receiveDto.ReceivedDate;
            // Aggregate total weight received across all items
            purchaseOrder.TotalWeightReceived = purchaseOrder.RawGoldPurchaseOrderItems.Sum(i => i.WeightReceived);
            purchaseOrder.ModifiedBy = _currentUserService.UserId ?? "system";
            purchaseOrder.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated purchase order");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<RawGoldInventoryDto>> GetRawGoldInventoryAsync(int? branchId = null)
    {
        var query = _context.RawGoldInventories
            .Include(rgi => rgi.KaratType)
            .Include(rgi => rgi.Branch)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(rgi => rgi.BranchId == branchId.Value);

        var inventory = await query
            .OrderBy(rgi => rgi.KaratType.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<RawGoldInventoryDto>>(inventory);
    }

    public async Task<RawGoldInventoryDto?> GetRawGoldInventoryByKaratAsync(int karatTypeId, int branchId)
    {
        var inventory = await _context.RawGoldInventories
            .Include(rgi => rgi.KaratType)
            .Include(rgi => rgi.Branch)
            .FirstOrDefaultAsync(rgi => rgi.KaratTypeId == karatTypeId && rgi.BranchId == branchId);

        return inventory == null ? null : _mapper.Map<RawGoldInventoryDto>(inventory);
    }

    private async Task<string> GeneratePurchaseOrderNumberAsync()
    {
        var today = DateTime.Today;
        var prefix = $"RGP{today:yyyyMMdd}";
        
        var lastOrder = await _context.RawGoldPurchaseOrders
            .Where(po => po.PurchaseOrderNumber.StartsWith(prefix))
            .OrderByDescending(po => po.PurchaseOrderNumber)
            .FirstOrDefaultAsync();

        if (lastOrder == null)
            return $"{prefix}001";

        var lastNumber = lastOrder.PurchaseOrderNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var number))
            return $"{prefix}{(number + 1):D3}";

        return $"{prefix}001";
    }

    private async Task UpdateRawGoldInventoryAsync(RawGoldPurchaseOrderItem orderItem, decimal receivedWeight)
    {
        var inventory = await _context.RawGoldInventories
            .FirstOrDefaultAsync(rgi => rgi.KaratTypeId == orderItem.KaratTypeId && 
                                       rgi.BranchId == orderItem.RawGoldPurchaseOrder.BranchId);

        if (inventory == null)
        {
            // Create new inventory record
            inventory = new RawGoldInventory
            {
                KaratTypeId = orderItem.KaratTypeId,
                BranchId = orderItem.RawGoldPurchaseOrder.BranchId,
                WeightOnHand = receivedWeight,
                AverageCostPerGram = orderItem.UnitCostPerGram,
                TotalValue = receivedWeight * orderItem.UnitCostPerGram,
                LastCountDate = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId ?? "system"
            };

            _context.RawGoldInventories.Add(inventory);
        }
        else
        {
            // Update existing inventory using weighted average cost
            var newTotalValue = inventory.TotalValue + (receivedWeight * orderItem.UnitCostPerGram);
            var newTotalWeight = inventory.WeightOnHand + receivedWeight;
            
            inventory.WeightOnHand = newTotalWeight;
            inventory.AverageCostPerGram = newTotalWeight > 0 ? newTotalValue / newTotalWeight : 0;
            inventory.TotalValue = newTotalValue;
            inventory.ModifiedBy = _currentUserService.UserId ?? "system";
            inventory.ModifiedAt = DateTime.UtcNow;
        }

        // Reserve received weight until PO is completed or cancelled
        inventory.WeightReserved += receivedWeight;
        inventory.LastMovementDate = DateTime.UtcNow;
    }

    private async Task CreateInventoryMovementAsync(RawGoldPurchaseOrderItem orderItem, decimal receivedWeight)
    {
        var inventory = await _context.RawGoldInventories
            .FirstOrDefaultAsync(rgi => rgi.KaratTypeId == orderItem.KaratTypeId && 
                                       rgi.BranchId == orderItem.RawGoldPurchaseOrder.BranchId);

        if (inventory != null)
        {
            var movement = new RawGoldInventoryMovement
            {
                RawGoldInventoryId = inventory.Id,
                RawGoldPurchaseOrderId = orderItem.RawGoldPurchaseOrderId,
                RawGoldPurchaseOrderItemId = orderItem.Id,
                MovementDate = DateTime.UtcNow,
                MovementType = "Purchase Receipt",
                WeightChange = receivedWeight,
                WeightBalance = inventory.WeightOnHand,
                UnitCostPerGram = orderItem.UnitCostPerGram,
                TotalCost = receivedWeight * orderItem.UnitCostPerGram,
                ReferenceNumber = orderItem.RawGoldPurchaseOrder.PurchaseOrderNumber,
                Notes = $"Raw gold received from purchase order {orderItem.RawGoldPurchaseOrder.PurchaseOrderNumber}",
                CreatedBy = _currentUserService.UserId ?? "system"
            };

            _context.RawGoldInventoryMovements.Add(movement);
        }
    }

    public async Task<RawGoldPurchaseOrderPaymentResult> ProcessPaymentAsync(ProcessRawGoldPurchaseOrderPaymentRequestDto request)
    {
        var po = await _context.RawGoldPurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.RawGoldPurchaseOrderId);

        if (po == null)
            return new RawGoldPurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Raw gold purchase order not found" };

        // System supplier purchases are auto-paid; disallow manual payments
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == po.SupplierId);
        if (supplier?.IsSystemSupplier == true)
        {
            return new RawGoldPurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Cannot process payments for system supplier. System supplier purchases are automatically fully owned." };
        }

        if (request.PaymentAmount <= 0)
            return new RawGoldPurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Payment amount must be greater than zero" };

        if (request.PaymentAmount > po.OutstandingBalance)
            return new RawGoldPurchaseOrderPaymentResult { IsSuccess = false, ErrorMessage = "Payment amount cannot exceed outstanding balance" };

        var userId = _currentUserService.UserId ?? "system";
        var noteSuffix = string.IsNullOrWhiteSpace(request.Notes) ? string.Empty : $" - {request.Notes}";
        var referenceInfo = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? string.Empty : $" [Ref: {request.ReferenceNumber}]";
        var payNotes = $"RGP {po.PurchaseOrderNumber}{referenceInfo}{noteSuffix}";

        // Execute payment via treasury (no nested transactions here)
        var (_, supplierTxn) = await _treasuryService.PaySupplierAsync(po.BranchId, po.SupplierId, request.PaymentAmount, userId, payNotes);

        // Update PO payment fields
        po.AmountPaid += request.PaymentAmount;
        po.OutstandingBalance = po.TotalAmount - po.AmountPaid;
        po.PaymentStatus = po.OutstandingBalance == 0 ? "Paid" : "Partial";
        po.ModifiedBy = userId;
        po.ModifiedAt = DateTime.UtcNow;

        // Update ownership records proportionally to payment amount
        await UpdateRawGoldOwnershipAfterPaymentAsync(po, request.PaymentAmount, userId);

        await _uow.SaveChangesAsync();

        // Return updated PO via DTO
        var updatedPo = await GetByIdAsync(po.Id);
        return new RawGoldPurchaseOrderPaymentResult
        {
            IsSuccess = true,
            PurchaseOrder = updatedPo,
            AmountPaid = request.PaymentAmount,
            OutstandingAmount = po.OutstandingBalance,
            TransactionNumber = supplierTxn.TransactionNumber
        };
    }

    private async Task<string> GeneratePaymentTransactionNumberAsync(int branchId)
    {
        // RGP-PMT-YYYYMMDD-BR-XXXX
        var prefix = $"RGP-PMT-{DateTime.UtcNow:yyyyMMdd}-{branchId:00}-";
        var count = await _context.FinancialTransactions
            .CountAsync(ft => ft.TransactionNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):0000}";
    }

    public async Task<RawGoldPurchaseOrderDto> UpdateStatusAsync(int id, string newStatus, string? statusNotes = null)
    {
        var po = await _context.RawGoldPurchaseOrders
            .Include(p => p.RawGoldPurchaseOrderItems)
            .FirstOrDefaultAsync(p => p.Id == id) ?? throw new InvalidOperationException("Raw gold purchase order not found");

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            if (newStatus == "Cancelled")
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
                        PaymentMethodId = LookupTableConstants.PaymentMethodCash,
                        StatusId = LookupTableConstants.FinancialTransactionStatusReversed,
                        ProcessedByUserId = _currentUserService.UserId ?? "system",
                        Notes = $"Reversal for Raw Gold PO {po.PurchaseOrderNumber} cancellation",
                        ReceiptPrinted = false,
                        GeneralLedgerPosted = false
                    };

                    _context.FinancialTransactions.Add(reversal);

                    // Increase supplier balance back by paid amount
                    var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == po.SupplierId);
                    if (supplier != null)
                    {
                        supplier.CurrentBalance += po.AmountPaid;
                        supplier.LastTransactionDate = DateTime.UtcNow;
                        supplier.ModifiedAt = DateTime.UtcNow;
                        supplier.ModifiedBy = _currentUserService.UserId ?? "system";
                        _context.Suppliers.Update(supplier);
                    }

                    // Reset PO payment fields
                    po.AmountPaid = 0;
                    po.OutstandingBalance = po.TotalAmount;
                    po.PaymentStatus = "Unpaid";
                }

                // Reverse inventory: create negative movements and update inventory values (no deletions)
                foreach (var item in po.RawGoldPurchaseOrderItems)
                {
                    if (item.WeightReceived > 0)
                    {
                        var inventory = await _context.RawGoldInventories.FirstOrDefaultAsync(rgi =>
                            rgi.KaratTypeId == item.KaratTypeId && rgi.BranchId == po.BranchId);

                        if (inventory == null)
                            throw new InvalidOperationException($"Cannot cancel RGP {po.PurchaseOrderNumber}: inventory not found for karat {item.KaratTypeId} in branch {po.BranchId}");

                        if (inventory.WeightOnHand < item.WeightReceived)
                            throw new InvalidOperationException($"Cannot cancel RGP {po.PurchaseOrderNumber}: insufficient raw gold inventory to revert received weight for karat {item.KaratTypeId}");

                        // Update inventory using weighted values
                        inventory.WeightOnHand -= item.WeightReceived;
                        // Reduce reserved as the PO is being cancelled
                        inventory.WeightReserved = Math.Max(0, inventory.WeightReserved - item.WeightReceived);
                        inventory.TotalValue -= item.WeightReceived * item.UnitCostPerGram;
                        inventory.AverageCostPerGram = inventory.WeightOnHand > 0 ? (inventory.TotalValue / inventory.WeightOnHand) : 0;
                        inventory.ModifiedBy = _currentUserService.UserId ?? "system";
                        inventory.ModifiedAt = DateTime.UtcNow;
                        _context.RawGoldInventories.Update(inventory);

                        var movement = new RawGoldInventoryMovement
                        {
                            RawGoldInventoryId = inventory.Id,
                            RawGoldPurchaseOrderId = po.Id,
                            RawGoldPurchaseOrderItemId = item.Id,
                            MovementDate = DateTime.UtcNow,
                            MovementType = "Purchase Order Cancellation",
                            WeightChange = -item.WeightReceived,
                            WeightBalance = inventory.WeightOnHand,
                            UnitCostPerGram = item.UnitCostPerGram,
                            TotalCost = -(item.WeightReceived * item.UnitCostPerGram),
                            ReferenceNumber = $"RGP-CAN-{po.PurchaseOrderNumber}",
                            Notes = $"Reversal of received weight for RGP item {item.Id}",
                            CreatedBy = _currentUserService.UserId ?? "system"
                        };
                        _context.RawGoldInventoryMovements.Add(movement);
                    }
                }

                // Mark all items as cancelled and clear availability
                foreach (var item in po.RawGoldPurchaseOrderItems)
                {
                    item.Status = "Cancelled";
                    item.AvailableWeightForManufacturing = 0;
                    item.ModifiedBy = _currentUserService.UserId ?? "system";
                    item.ModifiedAt = DateTime.UtcNow;
                }
            }

            // Apply status and notes
            po.Status = newStatus;
            if (!string.IsNullOrWhiteSpace(statusNotes))
            {
                po.Notes = string.IsNullOrWhiteSpace(po.Notes)
                    ? $"Status changed to {newStatus}: {statusNotes}"
                    : $"{po.Notes}\nStatus changed to {newStatus}: {statusNotes}";
            }
            po.ModifiedBy = _currentUserService.UserId ?? "system";
            po.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated purchase order");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private async Task<string> GenerateReversalTransactionNumberAsync(int branchId)
    {
        // RGP-RVR-YYYYMMDD-BR-XXXX
        var prefix = $"RGP-RVR-{DateTime.UtcNow:yyyyMMdd}-{branchId:00}-";
        var count = await _context.FinancialTransactions
            .CountAsync(ft => ft.TransactionNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):0000}";
    }

    /// <summary>
    /// Update raw gold ownership records proportionally after payment
    /// </summary>
    private async Task UpdateRawGoldOwnershipAfterPaymentAsync(RawGoldPurchaseOrder po, decimal paymentAmount, string userId)
    {
        // Find raw gold ownership records for this supplier and branch with outstanding amounts
        var ownershipRecords = await _context.RawGoldOwnerships
            .Where(rgo => rgo.SupplierId == po.SupplierId && 
                         rgo.BranchId == po.BranchId && 
                         rgo.IsActive &&
                         rgo.OutstandingAmount > 0)
            .ToListAsync();

        if (!ownershipRecords.Any())
            return;

        // Calculate total outstanding amount for this supplier/branch
        var totalOutstanding = ownershipRecords.Sum(rgo => rgo.OutstandingAmount);
        
        if (totalOutstanding <= 0)
            return;

        foreach (var ownership in ownershipRecords)
        {
            // Calculate proportional payment amount based on this ownership's share of total outstanding
            var proportionalPayment = paymentAmount * (ownership.OutstandingAmount / totalOutstanding);
            
            // Update raw gold ownership directly
            ownership.AmountPaid += proportionalPayment;
            ownership.OutstandingAmount -= proportionalPayment;
            
            // Recalculate ownership percentage based on payment
            ownership.OwnershipPercentage = ownership.TotalCost > 0 
                ? ownership.AmountPaid / ownership.TotalCost 
                : 0.0m;
            
            // Update owned weight based on payment percentage
            ownership.OwnedWeight = ownership.TotalWeight * ownership.OwnershipPercentage;
            
            ownership.ModifiedAt = DateTime.UtcNow;
            ownership.ModifiedBy = userId;
        }
        
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Create or update raw gold ownership records based on payment percentage
    /// </summary>
    private async Task CreateOrUpdateRawGoldOwnershipAsync(RawGoldPurchaseOrderItem item, string userId)
    {
        try
        {
            // Find existing ownership record for this karat type, supplier, and branch
            var existingOwnership = await _context.RawGoldOwnerships
                .FirstOrDefaultAsync(rgo => 
                    rgo.KaratTypeId == item.KaratTypeId &&
                    rgo.SupplierId == item.RawGoldPurchaseOrder.SupplierId &&
                    rgo.BranchId == item.RawGoldPurchaseOrder.BranchId &&
                    rgo.IsActive);

            if (existingOwnership != null)
            {
                // Update existing ownership
                existingOwnership.TotalWeight += item.WeightReceived;
                existingOwnership.TotalCost += item.LineTotal;
                existingOwnership.OutstandingAmount += item.LineTotal;
                
                // Recalculate ownership percentage based on payment
                existingOwnership.OwnershipPercentage = existingOwnership.TotalCost > 0 
                    ? existingOwnership.AmountPaid / existingOwnership.TotalCost 
                    : 0.0m;
                
                // Owned weight is based on payment percentage
                existingOwnership.OwnedWeight = existingOwnership.TotalWeight * existingOwnership.OwnershipPercentage;
                
                existingOwnership.ModifiedAt = DateTime.UtcNow;
                existingOwnership.ModifiedBy = userId;
            }
            else
            {
                // Create new ownership record
                var newOwnership = new RawGoldOwnership
                {
                    KaratTypeId = item.KaratTypeId,
                    BranchId = item.RawGoldPurchaseOrder.BranchId,
                    SupplierId = item.RawGoldPurchaseOrder.SupplierId,
                    RawGoldPurchaseOrderId = item.RawGoldPurchaseOrderId,
                    TotalWeight = item.WeightReceived,
                    OwnedWeight = 0.0m, // 0% ownership until payment is made
                    OwnershipPercentage = 0.0m, // 0% until payment is made
                    TotalCost = item.LineTotal,
                    AmountPaid = 0.0m,
                    OutstandingAmount = item.LineTotal,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _context.RawGoldOwnerships.AddAsync(newOwnership);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating raw gold ownership for item {ItemId}", item.Id);
            throw;
        }
    }
}
