using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.SupplierModels;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for Supplier entity and related DTOs
/// </summary>
public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        // Entity to DTO mappings
        CreateMap<Supplier, SupplierDto>()
            .ForMember(d => d.LastTransactionDate, o => o.MapFrom(s => s.PurchaseOrders != null && s.PurchaseOrders.Any() ?
                s.PurchaseOrders.Max(po => po.CreatedAt) : (DateTime?)null));

        CreateMap<Supplier, SupplierBalanceDto>()
            .ForMember(d => d.SupplierId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.CompanyName))
            .ForMember(d => d.CreditLimit, o => o.MapFrom(s => s.CreditLimit))
            .ForMember(d => d.CurrentBalance, o => o.MapFrom(s => s.CurrentBalance))
            .ForMember(d => d.AvailableCredit, o => o.MapFrom(s => s.CreditLimit - s.CurrentBalance))
            .ForMember(d => d.PaymentTermsDays, o => o.MapFrom(s => s.PaymentTermsDays))
            .ForMember(d => d.CreditLimitEnforced, o => o.MapFrom(s => s.CreditLimitEnforced))
            .ForMember(d => d.LastTransactionDate, o => o.MapFrom(s => s.PurchaseOrders != null && s.PurchaseOrders.Any() ?
                s.PurchaseOrders.Max(po => po.CreatedAt) : (DateTime?)null))
            .ForMember(d => d.RecentTransactions, o => o.Ignore()); // Would need custom resolver

        CreateMap<Supplier, SupplierProductsDto>()
            .ForMember(d => d.SupplierId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.CompanyName))
            .ForMember(d => d.TotalProductCount, o => o.MapFrom(s => s.PurchaseOrders != null ?
                s.PurchaseOrders.SelectMany(po => po.PurchaseOrderItems).Select(poi => poi.ProductId).Distinct().Count() : 0))
            .ForMember(d => d.Products, o => o.Ignore()); // Would need custom resolver

        CreateMap<Supplier, SupplierCreditAlertDto>()
            .ForMember(d => d.SupplierId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.CompanyName))
            .ForMember(d => d.CreditLimit, o => o.MapFrom(s => s.CreditLimit))
            .ForMember(d => d.CurrentBalance, o => o.MapFrom(s => s.CurrentBalance))
            .ForMember(d => d.AvailableCredit, o => o.MapFrom(s => s.CreditLimit - s.CurrentBalance))
            .ForMember(d => d.CreditUtilizationPercentage, o => o.MapFrom(s => s.CreditLimit > 0 ?
                (s.CurrentBalance / s.CreditLimit) * 100 : 0))
            .ForMember(d => d.AlertType, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.Severity, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.Message, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.LastTransactionDate, o => o.MapFrom(s => s.PurchaseOrders != null && s.PurchaseOrders.Any() ?
                s.PurchaseOrders.Max(po => po.CreatedAt) : (DateTime?)null));

        // DTO to Entity mappings
        CreateMap<CreateSupplierRequestDto, Supplier>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.CurrentBalance, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.LastTransactionDate, o => o.Ignore())
            .ForMember(d => d.PurchaseOrders, o => o.Ignore())
            .ForMember(d => d.SupplierTransactions, o => o.Ignore());

        CreateMap<UpdateSupplierRequestDto, Supplier>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CurrentBalance, o => o.Ignore())
            .ForMember(d => d.LastTransactionDate, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.PurchaseOrders, o => o.Ignore())
            .ForMember(d => d.SupplierTransactions, o => o.Ignore())
            .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.CompanyName))
            .ForMember(d => d.ContactPersonName, o => o.MapFrom(s => s.ContactPersonName))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
            .ForMember(d => d.TaxRegistrationNumber, o => o.MapFrom(s => s.TaxRegistrationNumber))
            .ForMember(d => d.CommercialRegistrationNumber, o => o.MapFrom(s => s.CommercialRegistrationNumber))
            .ForMember(d => d.CreditLimit, o => o.MapFrom(s => s.CreditLimit))
            .ForMember(d => d.PaymentTermsDays, o => o.MapFrom(s => s.PaymentTermsDays))
            .ForMember(d => d.CreditLimitEnforced, o => o.MapFrom(s => s.CreditLimitEnforced))
            .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes));

        // Search mappings
        CreateMap<SupplierSearchRequestDto, Supplier>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CompanyName, o => o.Ignore())
            .ForMember(d => d.ContactPersonName, o => o.Ignore())
            .ForMember(d => d.Phone, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.Address, o => o.Ignore())
            .ForMember(d => d.TaxRegistrationNumber, o => o.Ignore())
            .ForMember(d => d.CommercialRegistrationNumber, o => o.Ignore())
            .ForMember(d => d.CreditLimit, o => o.Ignore())
            .ForMember(d => d.CurrentBalance, o => o.Ignore())
            .ForMember(d => d.PaymentTermsDays, o => o.Ignore())
            .ForMember(d => d.CreditLimitEnforced, o => o.Ignore())
            .ForMember(d => d.PaymentTerms, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.LastTransactionDate, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.PurchaseOrders, o => o.Ignore())
            .ForMember(d => d.SupplierTransactions, o => o.Ignore());

        // Balance update mappings
        CreateMap<UpdateSupplierBalanceRequestDto, SupplierTransaction>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransactionNumber, o => o.Ignore())
            .ForMember(d => d.TransactionDate, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Amount))
            .ForMember(d => d.BalanceAfterTransaction, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore());

        // Transaction mappings
        CreateMap<SupplierTransaction, SupplierTransactionDto>()
            .ForMember(d => d.TransactionId, o => o.MapFrom(s => s.Id));

        // Credit validation mapping
        CreateMap<Supplier, SupplierCreditValidationResult>()
            .ForMember(d => d.CanPurchase, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.Message, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.RequestedAmount, o => o.Ignore()) // Set in service
            .ForMember(d => d.CurrentBalance, o => o.MapFrom(s => s.CurrentBalance))
            .ForMember(d => d.CreditLimit, o => o.MapFrom(s => s.CreditLimit))
            .ForMember(d => d.CreditLimitEnforced, o => o.MapFrom(s => s.CreditLimitEnforced))
            .ForMember(d => d.Warnings, o => o.Ignore()); // Would need custom resolver
    }
}
