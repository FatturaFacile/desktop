// <copyright file="InvoiceProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring projections for Italian electronic invoices, Swiss QR-bills, and invoice lines to DTO representations.
/// </summary>
public sealed class InvoiceProfile : Profile
{

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceProfile"/> class with mapping rules for sales documents.
    /// </summary>
    public InvoiceProfile()
    {
        _ = CreateMap<InvoiceLine, InvoiceLineDto>()
           .ForCtorParam(
                         ctorParamName: nameof(InvoiceLineDto.LineTotal),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.LineTotal));

        _ = CreateMap<ItalianElectronicInvoice, ItalianInvoiceDto>()
           .ForCtorParam(
                         ctorParamName: nameof(ItalianInvoiceDto.Currency),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.Currency.ToString()))
           .ForCtorParam(
                         ctorParamName: nameof(ItalianInvoiceDto.Lines),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.Lines));

        _ = CreateMap<SwissQrBill, SwissQrBillDto>()
           .ForCtorParam(
                         ctorParamName: nameof(SwissQrBillDto.CreditorAddressFormatted),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.CreditorAddress.ToString()))
           .ForCtorParam(
                         ctorParamName: nameof(SwissQrBillDto.CreditorQrIban),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.CreditorQrIban.Value))
           .ForCtorParam(
                         ctorParamName: nameof(SwissQrBillDto.DebtorAddressFormatted),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.DebtorAddress.ToString()))
           .ForCtorParam(
                         ctorParamName: nameof(SwissQrBillDto.Amount),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.Amount.Amount))
           .ForCtorParam(
                         ctorParamName: nameof(SwissQrBillDto.Currency),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.Amount.Currency.ToString()))
           .ForCtorParam(
                         ctorParamName: nameof(SwissQrBillDto.AdditionalMessage),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.AdditionalUnstructuredMessage));
    }

}
