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
        _ = CreateMap<Tenant, TenantDto>();
        _ = CreateMap<TenantDto, Tenant>()
            .ForMember(d => d.Settings, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        _ = CreateMap<Contact, ContactDto>()
            .ForMember(d => d.DisplayName, opt => opt.MapFrom(s => s.DisplayName));
        _ = CreateMap<ContactDto, Contact>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        _ = CreateMap<Product, ProductDto>();
        _ = CreateMap<ProductDto, Product>()
            .ForMember(d => d.MinStock, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        _ = CreateMap<InvoiceItem, InvoiceItemDto>();
        _ = CreateMap<InvoiceItemDto, InvoiceItem>()
            .ForMember(d => d.TenantId, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Invoice, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore());

        _ = CreateMap<InvoiceQrBillData, InvoiceQrBillDto>();
        _ = CreateMap<InvoiceQrBillDto, InvoiceQrBillData>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Invoice, opt => opt.Ignore());

        _ = CreateMap<InvoiceSdiMetadata, InvoiceSdiDto>();
        _ = CreateMap<InvoiceSdiDto, InvoiceSdiMetadata>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Invoice, opt => opt.Ignore());

        _ = CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.ContactName, opt => opt.MapFrom(s => s.Contact != null ? s.Contact.DisplayName : string.Empty))
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items))
            .ForMember(d => d.QrBillData, opt => opt.MapFrom(s => s.QrBillData))
            .ForMember(d => d.SdiMetadata, opt => opt.MapFrom(s => s.SdiMetadata));

        _ = CreateMap<ChartOfAccount, ChartOfAccountDto>();
        _ = CreateMap<ChartOfAccountDto, ChartOfAccount>()
            .ForMember(d => d.AccountCode, opt => opt.MapFrom(s => s.Code))
            .ForMember(d => d.AccountClass, opt => opt.Ignore())
            .ForMember(d => d.AccountType, opt => opt.MapFrom(s => s.Type))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());

        _ = CreateMap<GeneralLedgerLine, GeneralLedgerLineDto>()
            .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account != null ? $"{s.Account.Code} - {s.Account.Name}" : string.Empty));
        _ = CreateMap<GeneralLedgerLineDto, GeneralLedgerLine>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.GeneralLedgerEntry, opt => opt.Ignore())
            .ForMember(d => d.Account, opt => opt.Ignore());

        _ = CreateMap<GeneralLedgerEntry, GeneralLedgerEntryDto>()
            .ForMember(d => d.TotalDebit, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Debit)))
            .ForMember(d => d.TotalCredit, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Credit)))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));

        _ = CreateMap<HrEmployee, HrEmployeeDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName));
        _ = CreateMap<HrEmployeeDto, HrEmployee>()
            .ForMember(d => d.TerminationDate, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());
    }
}
