using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;

namespace DijaGoldPOS.API.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // Map OrderItem to OrderItemDto
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.ProductCode : string.Empty))
            .ForMember(d => d.KaratType, o => o.MapFrom(s => s.Product != null ? s.Product.KaratType != null ? s.Product.KaratType.Name : string.Empty : string.Empty))
            .ForMember(d => d.KaratTypeLookup, o => o.MapFrom(s => s.Product != null ? s.Product.KaratType : null))
            .ForMember(d => d.Weight, o => o.MapFrom(s => s.Product != null ? s.Product.Weight : 0));

        // Map Order to OrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : string.Empty))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.FullName : null))
            .ForMember(d => d.CashierName, o => o.MapFrom(s => s.Cashier != null ? s.Cashier.FullName : string.Empty))
            .ForMember(d => d.ApprovedByName, o => o.MapFrom(s => s.ApprovedByUser != null ? s.ApprovedByUser.FullName : null))
            .ForMember(d => d.GoldRatePerGram, o => o.MapFrom(s => s.GoldRate != null ? s.GoldRate.RatePerGram : 0))
            .ForMember(d => d.OriginalOrderNumber, o => o.MapFrom(s => s.OriginalOrder != null ? s.OriginalOrder.OrderNumber : null))
            .ForMember(d => d.FinancialTransactionNumber, o => o.MapFrom(s => s.FinancialTransaction != null ? s.FinancialTransaction.TransactionNumber : null))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.OrderItems))
            .ForMember(d => d.OrderTypeId, o => o.MapFrom(s => s.OrderTypeId))
            .ForMember(d => d.OrderTypeDescription, o => o.MapFrom(s => s.OrderType != null ? s.OrderType.Name : string.Empty))
            .ForMember(d => d.OrderType, o => o.MapFrom(s => s.OrderType))
            .ForMember(d => d.StatusId, o => o.MapFrom(s => s.StatusId))
            .ForMember(d => d.StatusDescription, o => o.MapFrom(s => s.Status != null ? s.Status.Name : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.ModifiedAt));

        // Map CreateOrderRequest to Order
        CreateMap<CreateOrderRequest, Order>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderNumber, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.CashierId, o => o.Ignore())
            .ForMember(d => d.OriginalOrderId, o => o.Ignore())
            .ForMember(d => d.FinancialTransactionId, o => o.Ignore())
            .ForMember(d => d.ReturnReason, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Customer, o => o.Ignore())
            .ForMember(d => d.Cashier, o => o.Ignore())
            .ForMember(d => d.ApprovedByUser, o => o.Ignore())
            .ForMember(d => d.GoldRate, o => o.Ignore())
            .ForMember(d => d.OriginalOrder, o => o.Ignore())
            .ForMember(d => d.FinancialTransaction, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore())
            .ForMember(d => d.OrderType, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Map CreateOrderItemRequest to OrderItem
        CreateMap<CreateOrderItemRequest, OrderItem>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderId, o => o.Ignore())
            .ForMember(d => d.UnitPrice, o => o.Ignore())
            .ForMember(d => d.TotalPrice, o => o.Ignore())
            .ForMember(d => d.DiscountAmount, o => o.Ignore())
            .ForMember(d => d.FinalPrice, o => o.Ignore())
            .ForMember(d => d.MakingCharges, o => o.Ignore())
            .ForMember(d => d.TaxAmount, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Order, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore());

        // Map UpdateOrderRequest to Order
        CreateMap<UpdateOrderRequest, Order>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderNumber, o => o.Ignore())
            .ForMember(d => d.OrderTypeId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.CustomerId, o => o.Ignore())
            .ForMember(d => d.CashierId, o => o.Ignore())
            .ForMember(d => d.GoldRateId, o => o.Ignore())
            .ForMember(d => d.OriginalOrderId, o => o.Ignore())
            .ForMember(d => d.FinancialTransactionId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Customer, o => o.Ignore())
            .ForMember(d => d.Cashier, o => o.Ignore())
            .ForMember(d => d.ApprovedByUser, o => o.Ignore())
            .ForMember(d => d.GoldRate, o => o.Ignore())
            .ForMember(d => d.OriginalOrder, o => o.Ignore())
            .ForMember(d => d.FinancialTransaction, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore())
            .ForMember(d => d.OrderType, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Map OrderSearchRequest to Order (for query purposes)
        CreateMap<OrderSearchRequest, Order>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderNumber, o => o.Ignore())
            .ForMember(d => d.OrderTypeId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.CustomerId, o => o.Ignore())
            .ForMember(d => d.CashierId, o => o.Ignore())
            .ForMember(d => d.ApprovedByUserId, o => o.Ignore())
            .ForMember(d => d.GoldRateId, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.OriginalOrderId, o => o.Ignore())
            .ForMember(d => d.FinancialTransactionId, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.EstimatedCompletionDate, o => o.Ignore())
            .ForMember(d => d.ReturnReason, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Customer, o => o.Ignore())
            .ForMember(d => d.Cashier, o => o.Ignore())
            .ForMember(d => d.ApprovedByUser, o => o.Ignore())
            .ForMember(d => d.GoldRate, o => o.Ignore())
            .ForMember(d => d.OriginalOrder, o => o.Ignore())
            .ForMember(d => d.FinancialTransaction, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore())
            .ForMember(d => d.OrderType, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Map ProcessOrderPaymentRequest
        CreateMap<ProcessOrderPaymentRequest, Order>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderNumber, o => o.Ignore())
            .ForMember(d => d.OrderTypeId, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.CustomerId, o => o.Ignore())
            .ForMember(d => d.CashierId, o => o.Ignore())
            .ForMember(d => d.ApprovedByUserId, o => o.Ignore())
            .ForMember(d => d.GoldRateId, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.OriginalOrderId, o => o.Ignore())
            .ForMember(d => d.FinancialTransactionId, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.EstimatedCompletionDate, o => o.Ignore())
            .ForMember(d => d.ReturnReason, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.Customer, o => o.Ignore())
            .ForMember(d => d.Cashier, o => o.Ignore())
            .ForMember(d => d.ApprovedByUser, o => o.Ignore())
            .ForMember(d => d.GoldRate, o => o.Ignore())
            .ForMember(d => d.OriginalOrder, o => o.Ignore())
            .ForMember(d => d.FinancialTransaction, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore())
            .ForMember(d => d.OrderType, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Map OrderSummary
        CreateMap<OrderSummary, OrderSummaryDto>();
    }
}
