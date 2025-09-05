using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.CustomerModels;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for customer purchase entities and DTOs
/// </summary>
public class CustomerPurchaseProfile : Profile
{
    public CustomerPurchaseProfile()
    {
        // CustomerPurchase -> CustomerPurchaseDto
        CreateMap<CustomerPurchase, CustomerPurchaseDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.Name))
            .ForMember(dest => dest.Items, opt => opt.Ignore()); // Handled separately in service

        // CustomerPurchaseItem -> CustomerPurchaseItemDto
        CreateMap<CustomerPurchaseItem, CustomerPurchaseItemDto>()
            .ForMember(dest => dest.KaratTypeName, opt => opt.MapFrom(src => src.KaratType.Name));

        // CreateCustomerPurchaseRequest -> CustomerPurchase (reverse mapping)
        CreateMap<CreateCustomerPurchaseRequest, CustomerPurchase>()
            .ForMember(dest => dest.CustomerPurchaseItems, opt => opt.MapFrom(src => src.Items));

        // CreateCustomerPurchaseItemRequest -> CustomerPurchaseItem
        CreateMap<CreateCustomerPurchaseItemRequest, CustomerPurchaseItem>();
    }
}
