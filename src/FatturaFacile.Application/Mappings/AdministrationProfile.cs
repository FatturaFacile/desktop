// <copyright file="AdministrationProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring projections between administration domain entities and data transfer objects.
/// </summary>
public sealed class AdministrationProfile : Profile
{

    /// <summary>
    /// Initializes a new instance of the <see cref="AdministrationProfile"/> class and registers mapping definitions.
    /// </summary>
    public AdministrationProfile()
    {
        _ = CreateMap<Tenant, TenantDto>();
        _ = CreateMap<User, UserDto>();

        _ = CreateMap<AppTranslation, AppTranslationDto>()
           .ForCtorParam(
                         ctorParamName: nameof(AppTranslationDto.EffectiveValue),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.EffectiveValue));
    }

}
