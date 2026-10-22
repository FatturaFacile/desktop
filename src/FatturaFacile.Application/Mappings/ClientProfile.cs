// <copyright file="ClientProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring mappings from CRM <see cref="Client"/> domain models and nested value objects to <see cref="ClientDto"/>.
/// </summary>
public sealed class ClientProfile : Profile
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientProfile"/> class, setting value object deconstructions.
    /// </summary>
    public ClientProfile()
    {
        _ = CreateMap<Client, ClientDto>()
           .ForCtorParam(
                         ctorParamName: nameof(ClientDto.FiscalCode),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.FiscalCode != null ? src.FiscalCode.Value : null))
           .ForCtorParam(
                         ctorParamName: nameof(ClientDto.VatNumber),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.VatNumber != null ? src.VatNumber.Value : null))
           .ForCtorParam(
                         ctorParamName: nameof(ClientDto.QrIban),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.QrIban != null ? src.QrIban.Value : null))
           .ForCtorParam(
                         ctorParamName: nameof(ClientDto.AddressFormatted),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.Address != null ? src.Address.ToString() : string.Empty))
           .ForCtorParam(
                         ctorParamName: nameof(ClientDto.CreditLimitAmount),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.CreditLimit.Amount))
           .ForCtorParam(
                         ctorParamName: nameof(ClientDto.CreditLimitCurrency),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.CreditLimit.Currency.ToString()));
    }

}
