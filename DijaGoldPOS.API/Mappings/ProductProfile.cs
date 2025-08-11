using AutoMapper;
using AutoMapper.QueryableExtensions;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.CompanyName : null));

        CreateMap<Inventory, ProductInventoryDto>()
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch.Name))
            .ForMember(d => d.IsLowStock, o => o.MapFrom(s => s.QuantityOnHand <= s.ReorderPoint));

        CreateMap<Product, ProductWithInventoryDto>()
            .IncludeBase<Product, ProductDto>()
            .ForMember(d => d.TotalQuantityOnHand, o => o.MapFrom(s => s.InventoryRecords.Sum(i => i.QuantityOnHand)))
            .ForMember(d => d.TotalWeightOnHand, o => o.MapFrom(s => s.InventoryRecords.Sum(i => i.WeightOnHand)))
            .ForMember(d => d.Inventory, o => o.MapFrom(s => s.InventoryRecords));

        CreateMap<CreateProductRequestDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore());

        CreateMap<UpdateProductRequestDto, Product>()
            .IncludeBase<CreateProductRequestDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore());
    }
}


