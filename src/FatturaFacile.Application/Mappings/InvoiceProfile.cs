// <copyright file="InvoiceProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring projections between invoice entities, line items, commands, and DTOs.
/// </summary>
public class InvoiceProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceProfile"/> class and registers mapping configurations.
    /// </summary>
    public InvoiceProfile()
    {
        CreateMap<Money, decimal>().ConvertUsing(m => m.Amount);

        _ = CreateMap<InvoiceItem, InvoiceItemDto>();

        _ = CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.ContactName, opt => opt.MapFrom(s => s.Contact != null ? s.Contact.DisplayName : "Unknown"))
            .ForMember(d => d.InvoiceType, opt => opt.MapFrom(s => s.InvoiceType.ToString()))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.ItemsCount, opt => opt.MapFrom(s => s.Items.Count))
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));

        _ = CreateMap<Invoice, InvoiceDetailDto>()
            .ForMember(d => d.ContactName, opt => opt.MapFrom(s => s.Contact != null ? s.Contact.DisplayName : "Unknown"))
            .ForMember(d => d.InvoiceType, opt => opt.MapFrom(s => s.InvoiceType.ToString()))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));

        _ = CreateMap<Invoice, InvoiceListItemDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Contact != null ? s.Contact.DisplayName : "Unknown"))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.ItemsCount, opt => opt.MapFrom(s => s.Items.Count));

        _ = CreateMap<CreateInvoiceCommand, Invoice>()
            .ForMember(d => d.Items, opt => opt.Ignore())
            .ForMember(d => d.Contact, opt => opt.Ignore())
            .ForMember(d => d.Tenant, opt => opt.Ignore());

        _ = CreateMap<InvoiceLine, InvoiceLineDto>();

        _ = CreateMap<ItalianElectronicInvoice, ItalianInvoiceDto>()
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Currency.ToString()))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));

        _ = CreateMap<SwissQrBill, SwissQrBillDto>()
            .ConstructUsing(s => new SwissQrBillDto(
                s.Id,
                s.TenantId,
                s.BillNumber,
                s.BillDate,
                s.ClientId,
                s.CreditorName,
                s.CreditorAddress.ToString(),
                s.CreditorQrIban.Value,
                s.DebtorName,
                s.DebtorAddress.ToString(),
                s.Amount.Amount,
                s.Amount.Currency.ToString(),
                s.ReferenceNumber,
                s.AdditionalUnstructuredMessage,
                s.DueDate,
                s.IsPaid));
    }
}
