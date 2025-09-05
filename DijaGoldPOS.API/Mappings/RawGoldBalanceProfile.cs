using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.InventoryModels;
using DijaGoldPOS.API.Models.OwneShipModels;
using DijaGoldPOS.API.Models.SupplierModels;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for Raw Gold Balance entities and related DTOs
/// </summary>
public class RawGoldBalanceProfile : Profile
{
    public RawGoldBalanceProfile()
    {
        // RawGoldTransfer mappings
        CreateMap<RawGoldTransfer, RawGoldTransferDto>()
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : "Unknown"))
            .ForMember(d => d.FromSupplierName, o => o.MapFrom(s => s.FromSupplier != null ? s.FromSupplier.CompanyName : null))
            .ForMember(d => d.ToSupplierName, o => o.MapFrom(s => s.ToSupplier != null ? s.ToSupplier.CompanyName : null))
            .ForMember(d => d.FromKaratTypeName, o => o.MapFrom(s => s.FromKaratType != null ? s.FromKaratType.Name : "Unknown"))
            .ForMember(d => d.ToKaratTypeName, o => o.MapFrom(s => s.ToKaratType != null ? s.ToKaratType.Name : "Unknown"))
            .ForMember(d => d.CustomerPurchaseNumber, o => o.MapFrom(s => s.CustomerPurchase != null ? s.CustomerPurchase.PurchaseNumber : null));

        CreateMap<WaiveGoldToSupplierRequest, RawGoldTransfer>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransferNumber, o => o.Ignore())
            .ForMember(d => d.FromSupplierId, o => o.Ignore()) // Always null for waiving (from merchant)
            .ForMember(d => d.ToSupplierId, o => o.MapFrom(s => s.ToSupplierId))
            .ForMember(d => d.ToWeight, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.FromGoldRate, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.ToGoldRate, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.ConversionFactor, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.TransferValue, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.TransferDate, o => o.Ignore()) // Set in service
            .ForMember(d => d.TransferType, o => o.UseValue("Waive"))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.FromSupplier, o => o.Ignore())
            .ForMember(d => d.ToSupplier, o => o.Ignore())
            .ForMember(d => d.FromKaratType, o => o.Ignore())
            .ForMember(d => d.ToKaratType, o => o.Ignore())
            .ForMember(d => d.CustomerPurchase, o => o.Ignore());

        CreateMap<ConvertGoldKaratRequest, RawGoldTransfer>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransferNumber, o => o.Ignore())
            .ForMember(d => d.FromSupplierId, o => o.MapFrom(s => s.SupplierId))
            .ForMember(d => d.ToSupplierId, o => o.MapFrom(s => s.SupplierId))
            .ForMember(d => d.ToWeight, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.FromGoldRate, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.ToGoldRate, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.ConversionFactor, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.TransferValue, o => o.Ignore()) // Calculated in service
            .ForMember(d => d.TransferDate, o => o.Ignore()) // Set in service
            .ForMember(d => d.TransferType, o => o.UseValue("Convert"))
            .ForMember(d => d.CustomerPurchaseId, o => o.Ignore())
            .ForMember(d => d.CreatedByUserId, o => o.Ignore()) // Set in service
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.FromSupplier, o => o.Ignore())
            .ForMember(d => d.ToSupplier, o => o.Ignore())
            .ForMember(d => d.FromKaratType, o => o.Ignore())
            .ForMember(d => d.ToKaratType, o => o.Ignore())
            .ForMember(d => d.CustomerPurchase, o => o.Ignore());

        // SupplierGoldBalance mappings
        CreateMap<SupplierGoldBalance, SupplierGoldBalanceDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.CompanyName : "Unknown"))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : "Unknown"))
            .ForMember(d => d.KaratTypeName, o => o.MapFrom(s => s.KaratType != null ? s.KaratType.Name : "Unknown"))
            .ForMember(d => d.KaratPurity, o => o.UseValue(0)); // TODO: Add purity calculation logic

        // RawGoldInventory to MerchantRawGoldBalanceDto mapping
        CreateMap<RawGoldInventory, MerchantRawGoldBalanceDto>()
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : "Unknown"))
            .ForMember(d => d.KaratTypeName, o => o.MapFrom(s => s.KaratType != null ? s.KaratType.Name : "Unknown"))
            .ForMember(d => d.KaratPurity, o => o.UseValue(0)) // TODO: Add purity calculation logic
            .ForMember(d => d.AvailableWeight, o => o.MapFrom(s => s.AvailableWeight))
            .ForMember(d => d.TotalValue, o => o.MapFrom(s => s.TotalValue))
            .ForMember(d => d.LastMovementDate, o => o.MapFrom(s => s.LastMovementDate));

        // KaratConversionDto mapping (no source entity, created in service)
        // GoldBalanceSummaryDto mapping (no source entity, created in service)
        
        // GoldTransferSearchRequest validation mapping (used for filtering, no direct entity mapping needed)
    }
}
