// <copyright file="InvoicingMappingProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Entities;

namespace FatturaFacile.Invoicing.Application.Mappings;

/// <summary>
/// AutoMapper profile mapping domain invoice aggregates and entities to application layer DTOs.
/// </summary>
public sealed class InvoicingMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicingMappingProfile"/> class with mapping configurations.
    /// </summary>
    public InvoicingMappingProfile()
    {
        _ = CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.InvoiceType, opt => opt.MapFrom(s => s.InvoiceType.ToString()))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.Jurisdiction, opt => opt.MapFrom(s => s.Jurisdiction.ToString()))
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));

        _ = CreateMap<InvoiceItem, InvoiceItemDto>();
    }
}
