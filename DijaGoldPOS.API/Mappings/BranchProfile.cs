using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for Branch entity and related DTOs
/// </summary>
public class BranchProfile : Profile
{
    public BranchProfile()
    {
        // Entity to DTO mappings
        CreateMap<Branch, BranchDto>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive));

        CreateMap<Branch, BranchInfoDto>();

        // DTO to Entity mappings
        CreateMap<CreateBranchRequestDto, Branch>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Users, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.InventoryItems, o => o.Ignore())
            .ForMember(d => d.CashDrawerBalances, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore());

        CreateMap<UpdateBranchRequestDto, Branch>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Users, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.InventoryItems, o => o.Ignore())
            .ForMember(d => d.CashDrawerBalances, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore())
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Code))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone))
            .ForMember(d => d.ManagerName, o => o.MapFrom(s => s.ManagerName))
            .ForMember(d => d.IsHeadquarters, o => o.MapFrom(s => s.IsHeadquarters));

        // Search mappings
        CreateMap<BranchSearchRequestDto, Branch>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Name, o => o.Ignore())
            .ForMember(d => d.Code, o => o.Ignore())
            .ForMember(d => d.Address, o => o.Ignore())
            .ForMember(d => d.Phone, o => o.Ignore())
            .ForMember(d => d.ManagerName, o => o.Ignore())
            .ForMember(d => d.IsHeadquarters, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Users, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.FinancialTransactions, o => o.Ignore())
            .ForMember(d => d.InventoryItems, o => o.Ignore())
            .ForMember(d => d.CashDrawerBalances, o => o.Ignore())
            .ForMember(d => d.ProductOwnerships, o => o.Ignore());

        // Complex DTO mappings
        CreateMap<Branch, BranchInventorySummaryDto>()
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.BranchCode, o => o.MapFrom(s => s.Code))
            .ForMember(d => d.TotalProducts, o => o.MapFrom(s => s.InventoryItems != null ? s.InventoryItems.Count() : 0))
            .ForMember(d => d.TotalWeight, o => o.MapFrom(s => s.InventoryItems != null ? s.InventoryItems.Sum(i => i.WeightOnHand) : 0))
            .ForMember(d => d.TotalValue, o => o.MapFrom(s => s.InventoryItems != null ? s.InventoryItems.Sum(i => i.QuantityOnHand * (i.Product != null ? i.Product.UnitPrice : 0)) : 0))
            .ForMember(d => d.LowStockItems, o => o.MapFrom(s => s.InventoryItems != null ? s.InventoryItems.Count(i => i.QuantityOnHand <= i.ReorderPoint) : 0))
            .ForMember(d => d.OutOfStockItems, o => o.MapFrom(s => s.InventoryItems != null ? s.InventoryItems.Count(i => i.QuantityOnHand <= 0) : 0))
            .ForMember(d => d.TopItems, o => o.Ignore()); // Would need custom resolver for top items

        CreateMap<Inventory, BranchInventoryItemDto>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId))
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.ProductCode : string.Empty))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.QuantityOnHand, o => o.MapFrom(s => s.QuantityOnHand))
            .ForMember(d => d.WeightOnHand, o => o.MapFrom(s => s.WeightOnHand))
            .ForMember(d => d.EstimatedValue, o => o.MapFrom(s => s.QuantityOnHand * (s.Product != null ? s.Product.UnitPrice : 0)))
            .ForMember(d => d.IsLowStock, o => o.MapFrom(s => s.QuantityOnHand <= s.ReorderPoint))
            .ForMember(d => d.IsOutOfStock, o => o.MapFrom(s => s.QuantityOnHand <= 0));

        CreateMap<Branch, BranchPerformanceDto>()
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.BranchCode, o => o.MapFrom(s => s.Code))
            .ForMember(d => d.ReportDate, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.DailySales, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.DailyTransactions, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.MonthlySales, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.MonthlyTransactions, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.AverageTransactionValue, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.ActiveCustomers, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.InventoryTurnover, o => o.Ignore()) // Would need custom resolver
            .ForMember(d => d.RecentTransactions, o => o.Ignore()); // Would need custom resolver
    }
}
