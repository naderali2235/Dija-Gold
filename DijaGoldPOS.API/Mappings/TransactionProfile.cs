using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionItem, TransactionItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.ProductCode : string.Empty))
            .ForMember(d => d.KaratType, o => o.MapFrom(s => s.Product != null ? s.Product.KaratType : s.Product.KaratType));

        CreateMap<TransactionTax, TransactionTaxDto>()
            .ForMember(d => d.TaxName, o => o.MapFrom(s => s.TaxConfiguration != null ? s.TaxConfiguration.TaxName : string.Empty))
            .ForMember(d => d.TaxCode, o => o.MapFrom(s => s.TaxConfiguration != null ? s.TaxConfiguration.TaxCode : string.Empty));

        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : string.Empty))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.FullName : null))
            .ForMember(d => d.CashierName, o => o.MapFrom(s => s.Cashier != null ? s.Cashier.FullName : string.Empty))
            .ForMember(d => d.ApprovedByName, o => o.MapFrom(s => s.ApprovedByUser != null ? s.ApprovedByUser.FullName : null))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.TransactionItems))
            .ForMember(d => d.Taxes, o => o.MapFrom(s => s.TransactionTaxes));
    }
}


