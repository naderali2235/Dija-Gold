using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PurchaseOrderService> _logger;
    private readonly IAuditService _auditService;

    public PurchaseOrderService(IUnitOfWork uow, ILogger<PurchaseOrderService> logger, IAuditService auditService)
    {
        _uow = uow;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequestDto request, string userId)
    {
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
        var items = await query
            .OrderByDescending(po => po.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(po => new PurchaseOrderDto
            {
                Id = po.Id,
                PurchaseOrderNumber = po.PurchaseOrderNumber,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier.CompanyName,
                BranchId = po.BranchId,
                BranchName = po.Branch.Name,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                ActualDeliveryDate = po.ActualDeliveryDate,
                TotalAmount = po.TotalAmount,
                AmountPaid = po.AmountPaid,
                OutstandingBalance = po.OutstandingBalance,
                Status = po.Status,
                PaymentStatus = po.PaymentStatus
            })
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> ReceiveAsync(ReceivePurchaseOrderRequestDto request, string userId)
    {
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(request.PurchaseOrderId);
        if (po == null || po.Status == "Cancelled") return false;

        foreach (var item in request.Items)
        {
            var poItem = po.PurchaseOrderItems.FirstOrDefault(i => i.Id == item.PurchaseOrderItemId);
            if (poItem == null) continue;

            poItem.QuantityReceived += item.QuantityReceived;
            poItem.WeightReceived += item.WeightReceived;
            poItem.Status = poItem.QuantityReceived >= poItem.QuantityOrdered ? "Received" : "Pending";
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
    }

    private async Task<PurchaseOrderDto> MapToDto(int id)
    {
        var po = await _uow.PurchaseOrders.GetWithItemsAsNoTrackingAsync(id) ?? throw new InvalidOperationException("PO not found");
        return MapToDto(po);
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder po)
    {
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
                Notes = i.Notes
            }).ToList()
        };
    }
}


