using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.InventoryModels;
using DijaGoldPOS.API.Models.PurchaseOrderModels;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for Raw Gold entities and related DTOs
/// </summary>
public class RawGoldProfile : Profile
{
    public RawGoldProfile()
    {
        // RawGoldPurchaseOrder mappings
        CreateMap<RawGoldPurchaseOrder, RawGoldPurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.CompanyName : null))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.RawGoldPurchaseOrderItems))
            .ForMember(d => d.AvailableStatuses, o => o.Ignore()); // Would need custom resolver

        CreateMap<CreateRawGoldPurchaseOrderDto, RawGoldPurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.Ignore())
            .ForMember(d => d.OutstandingBalance, o => o.Ignore())
            .ForMember(d => d.PaymentStatus, o => o.Ignore())
            .ForMember(d => d.TotalWeightOrdered, o => o.Ignore())
            .ForMember(d => d.TotalWeightReceived, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.RawGoldPurchaseOrderItems, o => o.Ignore());

        CreateMap<UpdateRawGoldPurchaseOrderDto, RawGoldPurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.SupplierId, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.Ignore())
            .ForMember(d => d.OutstandingBalance, o => o.Ignore())
            .ForMember(d => d.PaymentStatus, o => o.Ignore())
            .ForMember(d => d.TotalWeightOrdered, o => o.Ignore())
            .ForMember(d => d.TotalWeightReceived, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.RawGoldPurchaseOrderItems, o => o.Ignore());

        // RawGoldPurchaseOrderItem mappings
        CreateMap<RawGoldPurchaseOrderItem, RawGoldPurchaseOrderItemDto>()
            .ForMember(d => d.KaratTypeName, o => o.MapFrom(s => s.KaratType != null ? s.KaratType.Name : null))
            .ForMember(d => d.IsReceived, o => o.MapFrom(s => s.WeightReceived > 0))
            .ForMember(d => d.CanEdit, o => o.Ignore()); // Would need custom resolver

        CreateMap<CreateRawGoldPurchaseOrderItemDto, RawGoldPurchaseOrderItem>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.RawGoldPurchaseOrderId, o => o.Ignore())
            .ForMember(d => d.WeightReceived, o => o.Ignore())
            .ForMember(d => d.WeightConsumedInManufacturing, o => o.Ignore())
            .ForMember(d => d.AvailableWeightForManufacturing, o => o.Ignore())
            .ForMember(d => d.LineTotal, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.RawGoldPurchaseOrder, o => o.Ignore())
            .ForMember(d => d.KaratType, o => o.Ignore())
            .ForMember(d => d.SourceManufacturingRecords, o => o.Ignore());

        // RawGoldInventory mappings
        CreateMap<RawGoldInventory, RawGoldInventoryDto>()
            .ForMember(d => d.KaratTypeName, o => o.MapFrom(s => s.KaratType != null ? s.KaratType.Name : null))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.AvailableWeight, o => o.MapFrom(s => s.WeightOnHand - s.WeightReserved))
            .ForMember(d => d.IsLowStock, o => o.MapFrom(s => s.WeightOnHand <= s.ReorderPoint));

        // RawGoldInventoryMovement mappings
        CreateMap<RawGoldInventoryMovement, RawGoldInventoryMovementDto>()
            .ForMember(d => d.UnitCost, o => o.MapFrom(s => s.UnitCost))
            .ForMember(d => d.TotalCost, o => o.MapFrom(s => s.TotalCost));

        // Receive Raw Gold mappings
        CreateMap<ReceiveRawGoldDto, RawGoldPurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.SupplierId, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.ExpectedDeliveryDate, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.MapFrom(s => s.ReceivedDate))
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.Ignore())
            .ForMember(d => d.OutstandingBalance, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.RawGoldPurchaseOrderItems, o => o.Ignore());

        CreateMap<ReceiveRawGoldItemDto, RawGoldPurchaseOrderItem>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.RawGoldPurchaseOrderItemId))
            .ForMember(d => d.RawGoldPurchaseOrderId, o => o.Ignore())
            .ForMember(d => d.KaratTypeId, o => o.Ignore())
            .ForMember(d => d.WeightOrdered, o => o.Ignore())
            .ForMember(d => d.UnitCostPerGram, o => o.Ignore())
            .ForMember(d => d.WeightReceived, o => o.MapFrom(s => s.WeightReceived))
            .ForMember(d => d.WeightConsumedInManufacturing, o => o.Ignore())
            .ForMember(d => d.AvailableWeightForManufacturing, o => o.Ignore())
            .ForMember(d => d.LineTotal, o => o.Ignore())
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.PurityPercentage, o => o.Ignore())
            .ForMember(d => d.CertificateNumber, o => o.Ignore())
            .ForMember(d => d.Source, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.RawGoldPurchaseOrder, o => o.Ignore())
            .ForMember(d => d.KaratType, o => o.Ignore())
            .ForMember(d => d.SourceManufacturingRecords, o => o.Ignore());
    }
}
