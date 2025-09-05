using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.LookupModels;


namespace DijaGoldPOS.API.Mappings;

public class LookupProfile : Profile
{
    public LookupProfile()
    {
        // Map lookup entities to their corresponding DTOs
        CreateMap<FinancialTransactionTypeLookup, FinancialTransactionTypeLookupDto>();
        CreateMap<PaymentMethodLookup, PaymentMethodLookupDto>();
        CreateMap<FinancialTransactionStatusLookup, FinancialTransactionStatusLookupDto>();
        CreateMap<BusinessEntityTypeLookup, BusinessEntityTypeLookupDto>();
        CreateMap<KaratTypeLookup, KaratTypeLookupDto>();
        CreateMap<ChargeTypeLookup, ChargeTypeLookupDto>();
        CreateMap<ProductCategoryTypeLookup, ProductCategoryTypeLookupDto>();
        CreateMap<RepairStatusLookup, RepairStatusLookupDto>();
        CreateMap<RepairPriorityLookup, RepairPriorityLookupDto>();
        CreateMap<OrderTypeLookup, OrderTypeLookupDto>();
        CreateMap<OrderStatusLookup, OrderStatusLookupDto>();
        CreateMap<SubCategoryLookup, SubCategoryLookupDto>();
        CreateMap<TransactionStatusLookup, TransactionStatusLookupDto>();
        CreateMap<TransactionTypeLookup, TransactionTypeLookupDto>();
    }
}
