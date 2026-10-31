// <copyright file="TenantProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring projections between tenant domain entities and tenant selection DTOs.
/// </summary>
public class TenantProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantProfile"/> class and registers mapping configurations.
    /// </summary>
    public TenantProfile()
    {
        _ = CreateMap<Tenant, TenantDto>();

        _ = CreateMap<Tenant, TenantSelectionItemDto>()
            .ForMember(d => d.TaxIdDisplay, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.VatNumber) ? s.VatNumber : (s.UidNumber ?? s.FiscalCode ?? "N/A")))
            .ForMember(d => d.Initials, opt => opt.MapFrom(s => string.IsNullOrWhiteSpace(s.Name) ? "??" : string.Join("", s.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])))));
    }
}
