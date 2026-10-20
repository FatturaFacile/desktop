using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Inventory;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// Represents transport document profile.
/// </summary>
public sealed class TransportDocumentProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransportDocumentProfile"/> class.
    /// </summary>
    public TransportDocumentProfile()
    {
        _ = CreateMap<TransportDocument, TransportDocumentDto>()
            .ForCtorParam(nameof(TransportDocumentDto.ClientName), opt => opt.MapFrom(src => $"Client #{src.ClientId.ToString().Substring(0, 6)}"))
            .ForCtorParam(nameof(TransportDocumentDto.DestinationAddressFormatted), opt => opt.MapFrom(src => src.DestinationAddress.ToString()));
    }
}
