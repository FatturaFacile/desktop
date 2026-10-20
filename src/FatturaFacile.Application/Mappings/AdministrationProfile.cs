using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Application.Mappings;

public sealed class AdministrationProfile : Profile
{
    public AdministrationProfile()
    {
        _ = CreateMap<Tenant, TenantDto>();
        _ = CreateMap<User, UserDto>();
        _ = CreateMap<AppTranslation, AppTranslationDto>()
            .ForCtorParam(nameof(AppTranslationDto.EffectiveValue), opt => opt.MapFrom(src => src.EffectiveValue));
    }
}
