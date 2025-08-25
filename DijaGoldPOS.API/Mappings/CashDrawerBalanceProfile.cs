using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for CashDrawerBalance entity and related DTOs
/// </summary>
public class CashDrawerBalanceProfile : Profile
{
    public CashDrawerBalanceProfile()
    {
        // Entity to DTO mappings
        CreateMap<CashDrawerBalance, CashDrawerBalanceDto>()
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : string.Empty))
            .ForMember(d => d.BalanceDate, o => o.MapFrom(s => s.BalanceDate))
            .ForMember(d => d.OpeningBalance, o => o.MapFrom(s => s.OpeningBalance))
            .ForMember(d => d.ExpectedClosingBalance, o => o.MapFrom(s => s.ExpectedClosingBalance))
            .ForMember(d => d.ActualClosingBalance, o => o.MapFrom(s => s.ActualClosingBalance))
            .ForMember(d => d.CashOverShort, o => o.MapFrom(s => s.CashOverShort))
            .ForMember(d => d.OpenedByUserId, o => o.MapFrom(s => s.OpenedByUserId))
            .ForMember(d => d.ClosedByUserId, o => o.MapFrom(s => s.ClosedByUserId))
            .ForMember(d => d.OpenedAt, o => o.MapFrom(s => s.OpenedAt))
            .ForMember(d => d.ClosedAt, o => o.MapFrom(s => s.ClosedAt))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SettledAmount, o => o.MapFrom(s => s.SettledAmount))
            .ForMember(d => d.CarriedForwardAmount, o => o.MapFrom(s => s.CarriedForwardAmount))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes))
            .ForMember(d => d.SettlementNotes, o => o.MapFrom(s => s.SettlementNotes))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy));

        CreateMap<CashDrawerBalance, CashDrawerSummaryDto>()
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : string.Empty))
            .ForMember(d => d.BalanceDate, o => o.MapFrom(s => s.BalanceDate))
            .ForMember(d => d.OpeningBalance, o => o.MapFrom(s => s.OpeningBalance))
            .ForMember(d => d.ExpectedClosingBalance, o => o.MapFrom(s => s.ExpectedClosingBalance))
            .ForMember(d => d.ActualClosingBalance, o => o.MapFrom(s => s.ActualClosingBalance))
            .ForMember(d => d.CashOverShort, o => o.MapFrom(s => s.CashOverShort))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.IsReconciled, o => o.MapFrom(s => s.ActualClosingBalance.HasValue));

        // DTO to Entity mappings
        CreateMap<CreateCashDrawerBalanceRequestDto, CashDrawerBalance>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId))
            .ForMember(d => d.BalanceDate, o => o.MapFrom(s => s.BalanceDate))
            .ForMember(d => d.OpeningBalance, o => o.MapFrom(s => s.OpeningBalance))
            .ForMember(d => d.ExpectedClosingBalance, o => o.MapFrom(_ => 0m)) // Will be calculated
            .ForMember(d => d.ActualClosingBalance, o => o.Ignore())
            .ForMember(d => d.OpenedByUserId, o => o.Ignore())
            .ForMember(d => d.ClosedByUserId, o => o.Ignore())
            .ForMember(d => d.OpenedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.ClosedAt, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(_ => CashDrawerStatus.Open))
            .ForMember(d => d.SettledAmount, o => o.Ignore())
            .ForMember(d => d.CarriedForwardAmount, o => o.Ignore())
            .ForMember(d => d.SettlementNotes, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore());

        CreateMap<UpdateCashDrawerBalanceRequestDto, CashDrawerBalance>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.BalanceDate, o => o.Ignore())
            .ForMember(d => d.OpeningBalance, o => o.Ignore())
            .ForMember(d => d.ExpectedClosingBalance, o => o.Ignore())
            .ForMember(d => d.ActualClosingBalance, o => o.MapFrom(s => s.ActualClosingBalance))
            .ForMember(d => d.ClosedByUserId, o => o.Ignore())
            .ForMember(d => d.ClosedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
            .ForMember(d => d.SettledAmount, o => o.MapFrom(s => s.SettledAmount))
            .ForMember(d => d.CarriedForwardAmount, o => o.MapFrom(s => s.CarriedForwardAmount))
            .ForMember(d => d.SettlementNotes, o => o.MapFrom(s => s.SettlementNotes))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes));

        // Search mappings
        CreateMap<CashDrawerBalanceSearchRequestDto, CashDrawerBalance>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.BalanceDate, o => o.Ignore())
            .ForMember(d => d.OpeningBalance, o => o.Ignore())
            .ForMember(d => d.ExpectedClosingBalance, o => o.Ignore())
            .ForMember(d => d.ActualClosingBalance, o => o.Ignore())
            .ForMember(d => d.OpenedByUserId, o => o.Ignore())
            .ForMember(d => d.ClosedByUserId, o => o.Ignore())
            .ForMember(d => d.OpenedAt, o => o.Ignore())
            .ForMember(d => d.ClosedAt, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.SettledAmount, o => o.Ignore())
            .ForMember(d => d.CarriedForwardAmount, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.SettlementNotes, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore());

        // Settlement mapping
        CreateMap<ProcessCashDrawerSettlementRequestDto, CashDrawerBalance>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.BalanceDate, o => o.Ignore())
            .ForMember(d => d.OpeningBalance, o => o.Ignore())
            .ForMember(d => d.ExpectedClosingBalance, o => o.Ignore())
            .ForMember(d => d.ActualClosingBalance, o => o.MapFrom(s => s.ActualClosingBalance))
            .ForMember(d => d.ClosedByUserId, o => o.Ignore())
            .ForMember(d => d.ClosedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Status, o => o.MapFrom(_ => CashDrawerStatus.Closed))
            .ForMember(d => d.SettledAmount, o => o.MapFrom(s => s.SettledAmount))
            .ForMember(d => d.CarriedForwardAmount, o => o.MapFrom(s => s.CarriedForwardAmount))
            .ForMember(d => d.SettlementNotes, o => o.MapFrom(s => s.SettlementNotes));
    }
}
