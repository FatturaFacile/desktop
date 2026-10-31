// <copyright file="CustomerProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring mappings between <see cref="Contact"/> domain entities and customer DTO representations.
/// </summary>
public class CustomerProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerProfile"/> class and registers mapping configurations.
    /// </summary>
    public CustomerProfile()
    {
        _ = CreateMap<Contact, CustomerDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));

        _ = CreateMap<Contact, ContactDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));

        _ = CreateMap<Contact, CustomerLookupDto>()
            .ForMember(d => d.DisplayName, opt => opt.MapFrom(s => s.DisplayName))
            .ForMember(d => d.VatOrUid, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.VatNumber) ? s.VatNumber : s.UidNumber));
    }
}
