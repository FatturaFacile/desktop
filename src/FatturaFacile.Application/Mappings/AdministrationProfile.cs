using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// Represents administration profile.
/// </summary>
public sealed class AdministrationProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdministrationProfile"/> class.
    /// </summary>
    public AdministrationProfile()
    {
        _ = CreateMap<Tenant, TenantDto>();
        _ = CreateMap<User, UserDto>();
        _ = CreateMap<AppTranslation, AppTranslationDto>()
            .ForCtorParam(nameof(AppTranslationDto.EffectiveValue), opt => opt.MapFrom(src => src.EffectiveValue));
    }
}
