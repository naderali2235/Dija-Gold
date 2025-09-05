using AutoMapper;
using AutoMapper.QueryableExtensions;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.InventoryModels;
using DijaGoldPOS.API.Models.ProductModels;

namespace DijaGoldPOS.API.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryTypeId, o => o.MapFrom(s => s.CategoryTypeId))
            .ForMember(d => d.CategoryType, o => o.MapFrom(s => s.CategoryType))
            .ForMember(d => d.KaratTypeId, o => o.MapFrom(s => s.KaratTypeId))
            .ForMember(d => d.KaratType, o => o.MapFrom(s => s.KaratType))
            .ForMember(d => d.SubCategory, o => o.MapFrom(s => s.SubCategoryLookup));

        CreateMap<Inventory, ProductInventoryDto>()
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.IsLowStock, o => o.MapFrom(s => s.QuantityOnHand <= s.ReorderPoint));

        CreateMap<Product, ProductWithInventoryDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.ProductCode))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.CategoryTypeId, o => o.MapFrom(s => s.CategoryTypeId))
            .ForMember(d => d.CategoryType, o => o.MapFrom(s => s.CategoryType))
            .ForMember(d => d.KaratTypeId, o => o.MapFrom(s => s.KaratTypeId))
            .ForMember(d => d.KaratType, o => o.MapFrom(s => s.KaratType))
            .ForMember(d => d.Weight, o => o.MapFrom(s => s.Weight))
            .ForMember(d => d.Brand, o => o.MapFrom(s => s.Brand))
            .ForMember(d => d.DesignStyle, o => o.MapFrom(s => s.DesignStyle))
            .ForMember(d => d.SubCategoryId, o => o.MapFrom(s => s.SubCategoryId))
            .ForMember(d => d.SubCategory, o => o.MapFrom(s => s.SubCategoryLookup))
            .ForMember(d => d.Shape, o => o.MapFrom(s => s.Shape))
            .ForMember(d => d.PurityCertificateNumber, o => o.MapFrom(s => s.PurityCertificateNumber))
            .ForMember(d => d.CountryOfOrigin, o => o.MapFrom(s => s.CountryOfOrigin))
            .ForMember(d => d.YearOfMinting, o => o.MapFrom(s => s.YearOfMinting))
            .ForMember(d => d.FaceValue, o => o.MapFrom(s => s.FaceValue))
            .ForMember(d => d.HasNumismaticValue, o => o.MapFrom(s => s.HasNumismaticValue))
            .ForMember(d => d.MakingChargesApplicable, o => o.MapFrom(s => s.MakingChargesApplicable))
            .ForMember(d => d.ProductMakingChargesTypeId, o => o.MapFrom(s => s.ProductMakingChargesTypeId))
            .ForMember(d => d.ProductMakingChargesValue, o => o.MapFrom(s => s.ProductMakingChargesValue))
            .ForMember(d => d.UseProductMakingCharges, o => o.MapFrom(s => s.UseProductMakingCharges))
            .ForMember(d => d.SupplierId, o => o.MapFrom(s => s.SupplierId))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
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
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.ProductCode))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.CategoryTypeId, o => o.MapFrom(s => s.CategoryTypeId))
            .ForMember(d => d.KaratTypeId, o => o.MapFrom(s => s.KaratTypeId))
            .ForMember(d => d.Weight, o => o.MapFrom(s => s.Weight))
            .ForMember(d => d.Brand, o => o.MapFrom(s => s.Brand))
            .ForMember(d => d.DesignStyle, o => o.MapFrom(s => s.DesignStyle))
            .ForMember(d => d.SubCategoryId, o => o.MapFrom(s => s.SubCategoryId))
            .ForMember(d => d.Shape, o => o.MapFrom(s => s.Shape))
            .ForMember(d => d.PurityCertificateNumber, o => o.MapFrom(s => s.PurityCertificateNumber))
            .ForMember(d => d.CountryOfOrigin, o => o.MapFrom(s => s.CountryOfOrigin))
            .ForMember(d => d.YearOfMinting, o => o.MapFrom(s => s.YearOfMinting))
            .ForMember(d => d.FaceValue, o => o.MapFrom(s => s.FaceValue))
            .ForMember(d => d.HasNumismaticValue, o => o.MapFrom(s => s.HasNumismaticValue))
            .ForMember(d => d.MakingChargesApplicable, o => o.MapFrom(s => s.MakingChargesApplicable))
            .ForMember(d => d.ProductMakingChargesTypeId, o => o.MapFrom(s => s.ProductMakingChargesTypeId))
            .ForMember(d => d.ProductMakingChargesValue, o => o.MapFrom(s => s.ProductMakingChargesValue))
            .ForMember(d => d.UseProductMakingCharges, o => o.MapFrom(s => s.UseProductMakingCharges))
            .ForMember(d => d.SupplierId, o => o.MapFrom(s => s.SupplierId));
    }
}


