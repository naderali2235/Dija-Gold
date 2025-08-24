using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>();

        CreateMap<CreateCustomerRequestDto, Customer>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.RegistrationDate, o => o.Ignore())
            .ForMember(d => d.LoyaltyPoints, o => o.MapFrom(_ => 0))
            .ForMember(d => d.TotalPurchaseAmount, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.LastPurchaseDate, o => o.Ignore())
            .ForMember(d => d.TotalOrders, o => o.MapFrom(_ => 0));

        CreateMap<UpdateCustomerRequestDto, Customer>()
            .IncludeBase<CreateCustomerRequestDto, Customer>()
            .ForMember(d => d.Id, o => o.Ignore());
    }
}


