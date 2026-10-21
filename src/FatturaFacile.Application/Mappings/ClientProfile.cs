// <copyright file="ClientProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// Represents client profile.
/// </summary>
public sealed class ClientProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientProfile"/> class.
    /// </summary>
    public ClientProfile()
    {
        _ = CreateMap<Client, ClientDto>()
            .ForCtorParam(
                nameof(ClientDto.FiscalCode),
                opt => opt.MapFrom(src => src.FiscalCode != null ? src.FiscalCode.Value : null))
            .ForCtorParam(
                nameof(ClientDto.VatNumber),
                opt => opt.MapFrom(src => src.VatNumber != null ? src.VatNumber.Value : null))
            .ForCtorParam(
                nameof(ClientDto.QrIban),
                opt => opt.MapFrom(src => src.QrIban != null ? src.QrIban.Value : null))
            .ForCtorParam(
                nameof(ClientDto.AddressFormatted),
                opt => opt.MapFrom(src => src.Address != null ? src.Address.ToString() : string.Empty))
            .ForCtorParam(
                nameof(ClientDto.CreditLimitAmount),
                opt => opt.MapFrom(src => src.CreditLimit.Amount))
            .ForCtorParam(
                nameof(ClientDto.CreditLimitCurrency),
                opt => opt.MapFrom(src => src.CreditLimit.Currency.ToString()));
    }
}
