using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for PurchaseOrder entity and related DTOs
/// </summary>
public class PurchaseOrderProfile : Profile
{
    public PurchaseOrderProfile()
    {
        // Entity to DTO mappings
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.CompanyName : null))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.PurchaseOrderItems.Sum(i => i.LineTotal)))
            .ForMember(d => d.OutstandingBalance, o => o.MapFrom(s => s.PurchaseOrderItems.Sum(i => i.LineTotal) - s.AmountPaid))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status ?? string.Empty))
            .ForMember(d => d.PaymentStatus, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.Items, o => o.MapFrom(s => s.PurchaseOrderItems))
            .ForMember(d => d.AvailableStatuses, o => o.Ignore()); // Would need custom resolver

        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.ProductCode : null))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : null))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status ?? string.Empty))
            .ForMember(d => d.IsReceived, o => o.MapFrom(s => s.QuantityReceived > 0))
            .ForMember(d => d.CanEdit, o => o.Ignore()); // Would need custom resolver

        // DTO to Entity mappings
        CreateMap<CreatePurchaseOrderRequestDto, PurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderItems, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore());

        CreateMap<CreatePurchaseOrderItemRequestDto, PurchaseOrderItem>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderId, o => o.Ignore())
            .ForMember(d => d.QuantityReceived, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.WeightReceived, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.LineTotal, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.PurchaseOrder, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        CreateMap<UpdatePurchaseOrderRequestDto, PurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderItems, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore())
            .ForMember(d => d.SupplierId, o => o.MapFrom(s => s.SupplierId))
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.ExpectedDeliveryDate, o => o.MapFrom(s => s.ExpectedDeliveryDate))
            .ForMember(d => d.Terms, o => o.MapFrom(s => s.Terms))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes));

        CreateMap<UpdatePurchaseOrderItemRequestDto, PurchaseOrderItem>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderId, o => o.Ignore())
            .ForMember(d => d.QuantityReceived, o => o.Ignore())
            .ForMember(d => d.WeightReceived, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.LineTotal, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.PurchaseOrder, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId))
            .ForMember(d => d.QuantityOrdered, o => o.MapFrom(s => s.QuantityOrdered))
            .ForMember(d => d.WeightOrdered, o => o.MapFrom(s => s.WeightOrdered))
            .ForMember(d => d.UnitCost, o => o.MapFrom(s => s.UnitCost))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes));

        // Search mappings
        CreateMap<PurchaseOrderSearchRequestDto, PurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.SupplierId, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.ExpectedDeliveryDate, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.Terms, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderItems, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore());

        // Receipt and delivery mappings
        CreateMap<ProcessPurchaseOrderDeliveryRequestDto, PurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.SupplierId, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.ExpectedDeliveryDate, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.MapFrom(s => s.ActualDeliveryDate))
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.Terms, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderItems, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore());

        CreateMap<ProcessPurchaseOrderPaymentRequestDto, PurchaseOrder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderNumber, o => o.Ignore())
            .ForMember(d => d.SupplierId, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.ExpectedDeliveryDate, o => o.Ignore())
            .ForMember(d => d.ActualDeliveryDate, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.Terms, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderItems, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore());
    }
}
