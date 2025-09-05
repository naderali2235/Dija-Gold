using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.SalesModels;

namespace DijaGoldPOS.API.Mappings;

public class RepairJobProfile : Profile
{
    public RepairJobProfile()
    {
        // Map RepairJob to RepairJobDto
        CreateMap<RepairJob, RepairJobDto>()
            .ForMember(d => d.StatusDisplayName, o => o.MapFrom(s => s.Status != null ? s.Status.Name : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
            .ForMember(d => d.PriorityDisplayName, o => o.MapFrom(s => s.Priority != null ? s.Priority.Name : string.Empty))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority))
            .ForMember(d => d.AssignedTechnicianName, o => o.MapFrom(s => s.AssignedTechnician != null ? s.AssignedTechnician.FullName : null))
            .ForMember(d => d.QualityCheckerName, o => o.MapFrom(s => s.QualityChecker != null ? s.QualityChecker.FullName : null))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy))
            .ForMember(d => d.FinancialTransactionNumber, o => o.MapFrom(s => s.FinancialTransaction != null ? s.FinancialTransaction.TransactionNumber : null))
            .ForMember(d => d.RepairDescription, o => o.MapFrom(s => s.Notes))
            .ForMember(d => d.RepairAmount, o => o.MapFrom(s => s.EstimatedCost))
            .ForMember(d => d.AmountPaid, o => o.MapFrom(s => s.FinancialTransaction != null ? s.FinancialTransaction.AmountPaid : 0))
            .ForMember(d => d.EstimatedCompletionDate, o => o.MapFrom(s => s.EstimatedCompletionDate))
            .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.FinancialTransaction != null ? s.FinancialTransaction.BusinessEntityId : null))
            .ForMember(d => d.CustomerName, o => o.Ignore()) // Customer info should be accessed through BusinessEntityId
            .ForMember(d => d.CustomerPhone, o => o.Ignore()) // Customer info should be accessed through BusinessEntityId
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.FinancialTransaction != null ? s.FinancialTransaction.BranchId : 0))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.FinancialTransaction != null && s.FinancialTransaction.Branch != null ? s.FinancialTransaction.Branch.Name : string.Empty));

        // Map CreateRepairJobRequestDto to RepairJob
        CreateMap<CreateRepairJobRequestDto, RepairJob>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.StartedDate, o => o.Ignore())
            .ForMember(d => d.CompletedDate, o => o.Ignore())
            .ForMember(d => d.ReadyForPickupDate, o => o.Ignore())
            .ForMember(d => d.DeliveredDate, o => o.Ignore())
            .ForMember(d => d.ActualCost, o => o.Ignore())
            .ForMember(d => d.MaterialsUsed, o => o.Ignore())
            .ForMember(d => d.HoursSpent, o => o.Ignore())
            .ForMember(d => d.QualityCheckedBy, o => o.Ignore())
            .ForMember(d => d.QualityCheckDate, o => o.Ignore())
            .ForMember(d => d.CustomerNotified, o => o.Ignore())
            .ForMember(d => d.CustomerNotificationDate, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.FinancialTransaction, o => o.Ignore())
            .ForMember(d => d.AssignedTechnician, o => o.Ignore())
            .ForMember(d => d.QualityChecker, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.Priority, o => o.Ignore());
    }
}
