// <copyright file="MappingProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Common.Mappings;

/// <summary>
/// AutoMapper configuration profile defining strict, explicit object transformations across domain entities and DTOs.
/// </summary>
public sealed class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfile"/> class.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<Tenant, TenantDto>();
        CreateMap<TenantDto, Tenant>()
            .ForMember(d => d.Settings, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        CreateMap<Contact, ContactDto>()
            .ForMember(d => d.DisplayName, opt => opt.MapFrom(s => s.DisplayName));
        CreateMap<ContactDto, Contact>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        CreateMap<Product, ProductDto>();
        CreateMap<ProductDto, Product>()
            .ForMember(d => d.MinStock, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        CreateMap<InvoiceItem, InvoiceItemDto>();
        CreateMap<InvoiceItemDto, InvoiceItem>()
            .ForMember(d => d.TenantId, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Invoice, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore());

        CreateMap<InvoiceQrBillData, InvoiceQrBillDto>();
        CreateMap<InvoiceQrBillDto, InvoiceQrBillData>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Invoice, opt => opt.Ignore());

        CreateMap<InvoiceSdiMetadata, InvoiceSdiDto>();
        CreateMap<InvoiceSdiDto, InvoiceSdiMetadata>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Invoice, opt => opt.Ignore());

        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.ContactName, opt => opt.MapFrom(s => s.Contact != null ? s.Contact.DisplayName : string.Empty))
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items))
            .ForMember(d => d.QrBillData, opt => opt.MapFrom(s => s.QrBillData))
            .ForMember(d => d.SdiMetadata, opt => opt.MapFrom(s => s.SdiMetadata));

        CreateMap<ChartOfAccount, ChartOfAccountDto>();
        CreateMap<ChartOfAccountDto, ChartOfAccount>()
            .ForMember(d => d.AccountCode, opt => opt.MapFrom(s => s.Code))
            .ForMember(d => d.AccountClass, opt => opt.Ignore())
            .ForMember(d => d.AccountType, opt => opt.MapFrom(s => s.Type))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        CreateMap<GeneralLedgerLine, GeneralLedgerLineDto>()
            .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account != null ? $"{s.Account.Code} - {s.Account.Name}" : string.Empty));
        CreateMap<GeneralLedgerLineDto, GeneralLedgerLine>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.GeneralLedgerEntry, opt => opt.Ignore())
            .ForMember(d => d.Account, opt => opt.Ignore());

        CreateMap<GeneralLedgerEntry, GeneralLedgerEntryDto>()
            .ForMember(d => d.TotalDebit, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Debit)))
            .ForMember(d => d.TotalCredit, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Credit)))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));

        CreateMap<HrEmployee, HrEmployeeDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName));
        CreateMap<HrEmployeeDto, HrEmployee>()
            .ForMember(d => d.TerminationDate, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());
    }
}
