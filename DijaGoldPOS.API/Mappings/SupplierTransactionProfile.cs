using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

public class SupplierTransactionProfile : Profile
{
    public SupplierTransactionProfile()
    {
        CreateMap<SupplierTransaction, SupplierTransactionDto>()
            .ForMember(d => d.TransactionId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.TransactionNumber, o => o.MapFrom(s => s.TransactionNumber))
            .ForMember(d => d.TransactionDate, o => o.MapFrom(s => s.TransactionDate))
            .ForMember(d => d.TransactionType, o => o.MapFrom(s => s.TransactionType))
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Amount))
            .ForMember(d => d.BalanceAfterTransaction, o => o.MapFrom(s => s.BalanceAfterTransaction))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes));
    }
}
