using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Models.SupplierModels;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for ProductOwnership entity and related DTOs
/// </summary>
public class ProductOwnershipProfile : Profile
{
    public ProductOwnershipProfile()
    {
        // ProductOwnership entity to ProductOwnershipDto
        CreateMap<ProductOwnership, ProductOwnershipDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product.ProductCode))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.CompanyName : null))
            .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.PurchaseOrderNumber : null))
            .ForMember(dest => dest.CustomerPurchaseNumber, opt => opt.MapFrom(src => src.CustomerPurchase != null ? src.CustomerPurchase.PurchaseNumber : null))
            .ForMember(dest => dest.OwnershipPercentage, opt => opt.MapFrom(src => src.OwnershipPercentage))
            .ForMember(dest => dest.OutstandingAmount, opt => opt.MapFrom(src => src.TotalCost - src.AmountPaid));

        // ProductOwnershipRequest to ProductOwnership entity
        CreateMap<ProductOwnershipRequest, ProductOwnership>()
            .ForMember(dest => dest.OwnershipPercentage, opt => opt.MapFrom(src => 
                src.TotalQuantity > 0 ? (src.OwnedQuantity / src.TotalQuantity) * 100 : 0))
            .ForMember(dest => dest.OutstandingAmount, opt => opt.MapFrom(src => src.TotalCost - src.AmountPaid))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // OwnershipMovement entity to OwnershipMovementDto
        CreateMap<OwnershipMovement, OwnershipMovementDto>();

        // CreateOwnershipMovementRequest to OwnershipMovement entity
        CreateMap<CreateOwnershipMovementRequest, OwnershipMovement>()
            .ForMember(dest => dest.MovementDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
