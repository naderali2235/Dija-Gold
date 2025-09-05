using AutoMapper;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Mappings;

/// <summary>
/// AutoMapper profile for GoldRate entity and related DTOs
/// </summary>
public class GoldRateProfile : Profile
{
    public GoldRateProfile()
    {
        // Entity to DTO mappings
        CreateMap<GoldRate, GoldRateDto>()
            .ForMember(d => d.KaratTypeId, o => o.MapFrom(s => s.KaratTypeId))
            .ForMember(d => d.KaratType, o => o.MapFrom(s => s.KaratType != null ? s.KaratType.Name : string.Empty))
            .ForMember(d => d.RatePerGram, o => o.MapFrom(s => s.RatePerGram))
            .ForMember(d => d.EffectiveFrom, o => o.MapFrom(s => s.EffectiveFrom))
            .ForMember(d => d.EffectiveTo, o => o.MapFrom(s => s.EffectiveTo))
            .ForMember(d => d.IsCurrent, o => o.MapFrom(s => s.IsCurrent))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy));

        // DTO to Entity mappings
        CreateMap<CreateGoldRateRequestDto, GoldRate>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.EffectiveTo, o => o.Ignore())
            .ForMember(d => d.IsCurrent, o => o.MapFrom(_ => false)) // Will be set by service logic
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.KaratType, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore());

        CreateMap<UpdateGoldRateRequestDto, GoldRate>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.EffectiveTo, o => o.Ignore())
            .ForMember(d => d.IsCurrent, o => o.MapFrom(_ => false))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.KaratType, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.KaratTypeId, o => o.MapFrom(s => s.KaratTypeId))
            .ForMember(d => d.RatePerGram, o => o.MapFrom(s => s.Rate))
            .ForMember(d => d.EffectiveFrom, o => o.MapFrom(s => s.EffectiveDate));

        // Search mappings
        CreateMap<GoldRateSearchRequestDto, GoldRate>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.KaratTypeId, o => o.Ignore())
            .ForMember(d => d.RatePerGram, o => o.Ignore())
            .ForMember(d => d.EffectiveFrom, o => o.Ignore())
            .ForMember(d => d.EffectiveTo, o => o.Ignore())
            .ForMember(d => d.IsCurrent, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedAt, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.KaratType, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore());

        // Current gold rates mapping
        CreateMap<GoldRate, CurrentGoldRatesDto>()
            .ForMember(d => d.KaratTypeId, o => o.MapFrom(s => s.KaratTypeId))
            .ForMember(d => d.KaratType, o => o.MapFrom(s => s.KaratType != null ? s.KaratType.Name : string.Empty))
            .ForMember(d => d.CurrentRate, o => o.MapFrom(s => s.RatePerGram))
            .ForMember(d => d.LastUpdated, o => o.MapFrom(s => s.EffectiveFrom))
            .ForMember(d => d.UpdatedBy, o => o.MapFrom(s => s.CreatedBy));
    }
}
