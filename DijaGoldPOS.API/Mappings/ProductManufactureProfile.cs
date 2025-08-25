using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for ProductManufacture entity and DTOs
/// </summary>
public class ProductManufactureProfile : Profile
{
    public ProductManufactureProfile()
    {
        // Entity to DTO mappings
        CreateMap<ProductManufacture, ProductManufactureDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product.ProductCode))
            .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.SourcePurchaseOrderItem.PurchaseOrder.PurchaseOrderNumber))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.SourcePurchaseOrderItem.PurchaseOrder.Supplier.CompanyName));

        // DTO to Entity mappings
        CreateMap<CreateProductManufactureDto, ProductManufacture>();
        
        CreateMap<UpdateProductManufactureDto, ProductManufacture>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
