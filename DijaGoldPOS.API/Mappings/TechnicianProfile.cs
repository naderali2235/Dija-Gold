using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for Technician entity and related DTOs
/// </summary>
public class TechnicianProfile : Profile
{
    public TechnicianProfile()
    {
        // Entity to DTO mappings
        CreateMap<Technician, TechnicianDto>()
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy));

        // DTO to Entity mappings
        CreateMap<CreateTechnicianRequestDto, Technician>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.RepairJobs, o => o.Ignore())
            .ForMember(d => d.QualityCheckedRepairJobs, o => o.Ignore());

        CreateMap<UpdateTechnicianRequestDto, Technician>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.RepairJobs, o => o.Ignore())
            .ForMember(d => d.QualityCheckedRepairJobs, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.Specialization, o => o.MapFrom(s => s.Specialization))
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.BranchId));

        // Search mappings
        CreateMap<TechnicianSearchRequestDto, Technician>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.Ignore())
            .ForMember(d => d.PhoneNumber, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.Specialization, o => o.Ignore())
            .ForMember(d => d.BranchId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Branch, o => o.Ignore())
            .ForMember(d => d.RepairJobs, o => o.Ignore())
            .ForMember(d => d.QualityCheckedRepairJobs, o => o.Ignore());
    }
}
