// <copyright file="TransportDocumentProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Inventory;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring mappings for transport documents (DDT) to data transfer objects.
/// </summary>
public sealed class TransportDocumentProfile : Profile
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TransportDocumentProfile"/> class and configures mapping expressions.
    /// </summary>
    public TransportDocumentProfile()
    {
        _ = CreateMap<TransportDocument, TransportDocumentDto>()
           .ForCtorParam(
                         ctorParamName: nameof(TransportDocumentDto.ClientName),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => $"Client #{src.ClientId.ToString().Substring(0, 6)}"))
           .ForCtorParam(
                         ctorParamName: nameof(TransportDocumentDto.DestinationAddressFormatted),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.DestinationAddress.ToString()));
    }

}
