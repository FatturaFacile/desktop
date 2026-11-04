using AutoMapper;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Entities;

namespace FatturaFacile.Invoicing.Application.Mappings;

public sealed class InvoicingMappingProfile : Profile
{
    public InvoicingMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.InvoiceType, opt => opt.MapFrom(s => s.InvoiceType.ToString()))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.Jurisdiction, opt => opt.MapFrom(s => s.Jurisdiction.ToString()))
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));

        CreateMap<InvoiceItem, InvoiceItemDto>();
    }
}
