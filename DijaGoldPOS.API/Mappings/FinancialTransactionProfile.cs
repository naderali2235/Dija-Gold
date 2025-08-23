using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

public class FinancialTransactionProfile : Profile
{
    public FinancialTransactionProfile()
    {
        // Map FinancialTransaction to FinancialTransactionDto
        CreateMap<FinancialTransaction, FinancialTransactionDto>()
            .ForMember(d => d.TransactionTypeId, o => o.MapFrom(s => s.TransactionTypeId))
            .ForMember(d => d.TransactionTypeDescription, o => o.MapFrom(s => s.TransactionType != null ? s.TransactionType.Name : string.Empty))
            .ForMember(d => d.BusinessEntityType, o => o.MapFrom(s => s.BusinessEntityType != null ? s.BusinessEntityType.Name : string.Empty))
            .ForMember(d => d.PaymentMethodDescription, o => o.MapFrom(s => s.PaymentMethod != null ? s.PaymentMethod.Name : string.Empty))
            .ForMember(d => d.StatusDescription, o => o.MapFrom(s => s.Status != null ? s.Status.Name : string.Empty));

        // Map CreateFinancialTransactionRequestDto to FinancialTransaction
        CreateMap<CreateFinancialTransactionRequestDto, FinancialTransaction>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransactionNumber, o => o.Ignore())
            .ForMember(d => d.TransactionDate, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.ProcessedByUserId, o => o.Ignore())
            .ForMember(d => d.ReceiptPrinted, o => o.Ignore())
            .ForMember(d => d.GeneralLedgerPosted, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.ProcessedByUser, o => o.Ignore())
            .ForMember(d => d.ApprovedByUser, o => o.Ignore())
            .ForMember(d => d.OriginalTransaction, o => o.Ignore())
            .ForMember(d => d.ReversalTransactions, o => o.Ignore())
            .ForMember(d => d.TransactionType, o => o.Ignore())
            .ForMember(d => d.BusinessEntityType, o => o.Ignore())
            .ForMember(d => d.PaymentMethod, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Map UpdateFinancialTransactionRequestDto to FinancialTransaction
        CreateMap<UpdateFinancialTransactionRequestDto, FinancialTransaction>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransactionNumber, o => o.Ignore())
            .ForMember(d => d.TransactionTypeId, o => o.Ignore())
            .ForMember(d => d.BusinessEntityId, o => o.Ignore())
            .ForMember(d => d.BusinessEntityTypeId, o => o.Ignore())
            .ForMember(d => d.TransactionDate, o => o.Ignore())
            .ForMember(d => d.ProcessedByUserId, o => o.Ignore())
            .ForMember(d => d.ApprovedByUserId, o => o.Ignore())
            .ForMember(d => d.OriginalTransactionId, o => o.Ignore())
            .ForMember(d => d.ReceiptPrinted, o => o.Ignore())
            .ForMember(d => d.GeneralLedgerPosted, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.ProcessedByUser, o => o.Ignore())
            .ForMember(d => d.ApprovedByUser, o => o.Ignore())
            .ForMember(d => d.OriginalTransaction, o => o.Ignore())
            .ForMember(d => d.ReversalTransactions, o => o.Ignore())
            .ForMember(d => d.TransactionType, o => o.Ignore())
            .ForMember(d => d.BusinessEntityType, o => o.Ignore())
            .ForMember(d => d.PaymentMethod, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Map FinancialTransactionSearchRequestDto to FinancialTransaction (for query purposes)
        CreateMap<FinancialTransactionSearchRequestDto, FinancialTransaction>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransactionNumber, o => o.Ignore())
            .ForMember(d => d.TransactionTypeId, o => o.Ignore())
            .ForMember(d => d.BusinessEntityId, o => o.Ignore())
            .ForMember(d => d.BusinessEntityTypeId, o => o.Ignore())
            .ForMember(d => d.TransactionDate, o => o.Ignore())
            .ForMember(d => d.Subtotal, o => o.Ignore())
            .ForMember(d => d.TotalTaxAmount, o => o.Ignore())
            .ForMember(d => d.TotalDiscountAmount, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.Ignore())
            .ForMember(d => d.ChangeGiven, o => o.Ignore())
            .ForMember(d => d.PaymentMethodId, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.ProcessedByUserId, o => o.Ignore())
            .ForMember(d => d.ApprovedByUserId, o => o.Ignore())
            .ForMember(d => d.OriginalTransactionId, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.ReceiptPrinted, o => o.Ignore())
            .ForMember(d => d.GeneralLedgerPosted, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.ProcessedByUser, o => o.Ignore())
            .ForMember(d => d.ApprovedByUser, o => o.Ignore())
            .ForMember(d => d.OriginalTransaction, o => o.Ignore())
            .ForMember(d => d.ReversalTransactions, o => o.Ignore())
            .ForMember(d => d.TransactionType, o => o.Ignore())
            .ForMember(d => d.BusinessEntityType, o => o.Ignore())
            .ForMember(d => d.PaymentMethod, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Map FinancialTransactionSummaryDto
        CreateMap<FinancialTransactionSummaryDto, FinancialTransactionSummaryDto>();
    }
}
