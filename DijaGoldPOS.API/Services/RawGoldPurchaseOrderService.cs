using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for managing raw gold purchase orders with specialized business logic
/// </summary>
public interface IRawGoldPurchaseOrderService
{
    Task<IEnumerable<RawGoldPurchaseOrderDto>> GetAllAsync();
    Task<RawGoldPurchaseOrderDto?> GetByIdAsync(int id);
    Task<RawGoldPurchaseOrderDto> CreateAsync(CreateRawGoldPurchaseOrderDto createDto);
    Task<RawGoldPurchaseOrderDto> UpdateAsync(int id, UpdateRawGoldPurchaseOrderDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<RawGoldPurchaseOrderDto> ReceiveRawGoldAsync(int id, ReceiveRawGoldDto receiveDto);
    Task<IEnumerable<RawGoldInventoryDto>> GetRawGoldInventoryAsync(int? branchId = null);
    Task<RawGoldInventoryDto?> GetRawGoldInventoryByKaratAsync(int karatTypeId, int branchId);
}

public class RawGoldPurchaseOrderService : IRawGoldPurchaseOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public RawGoldPurchaseOrderService(
        ApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
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
            .FirstOrDefaultAsync(po => po.Id == id);

        if (purchaseOrder == null)
            return false;

        if (purchaseOrder.Status == "Received")
            throw new InvalidOperationException("Cannot delete a received purchase order");

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
            }

            // Update purchase order status
            purchaseOrder.Status = "Received";
            purchaseOrder.ActualDeliveryDate = receiveDto.ReceivedDate;
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
}
