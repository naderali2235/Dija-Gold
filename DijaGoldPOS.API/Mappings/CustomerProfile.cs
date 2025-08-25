using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        // Entity to DTO mapping
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.TotalOrders, o => o.MapFrom(s => s.Orders != null ? s.Orders.Count : 0));

        // DTO to Entity mappings
        CreateMap<CreateCustomerRequestDto, Customer>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.RegistrationDate, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.LoyaltyPoints, o => o.MapFrom(_ => 0))
            .ForMember(d => d.TotalPurchaseAmount, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.LastPurchaseDate, o => o.Ignore())
            .ForMember(d => d.TotalOrders, o => o.MapFrom(_ => 0))
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.CustomerPurchases, o => o.Ignore());

        CreateMap<UpdateCustomerRequestDto, Customer>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.RegistrationDate, o => o.Ignore())
            .ForMember(d => d.LoyaltyPoints, o => o.Ignore())
            .ForMember(d => d.TotalPurchaseAmount, o => o.Ignore())
            .ForMember(d => d.LastPurchaseDate, o => o.Ignore())
            .ForMember(d => d.TotalOrders, o => o.MapFrom(_ => 0))
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.CustomerPurchases, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.NationalId, o => o.MapFrom(s => s.NationalId))
            .ForMember(d => d.MobileNumber, o => o.MapFrom(s => s.MobileNumber))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
            .ForMember(d => d.LoyaltyTier, o => o.MapFrom(s => s.LoyaltyTier))
            .ForMember(d => d.DefaultDiscountPercentage, o => o.MapFrom(s => s.DefaultDiscountPercentage))
            .ForMember(d => d.MakingChargesWaived, o => o.MapFrom(s => s.MakingChargesWaived))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes));

        // CustomerPurchase related mappings
        CreateMap<CustomerPurchase, CustomerPurchaseDto>();
        CreateMap<CustomerPurchaseItem, CustomerPurchaseItemDto>();
    }
}


