using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Inventory;

namespace FatturaFacile.Application.Mappings;

public sealed class TransportDocumentProfile : Profile
{
    public TransportDocumentProfile()
    {
        _ = CreateMap<TransportDocument, TransportDocumentDto>()
            .ForCtorParam(nameof(TransportDocumentDto.ClientName), opt => opt.MapFrom(src => $"Client #{src.ClientId.ToString().Substring(0, 6)}"))
            .ForCtorParam(nameof(TransportDocumentDto.DestinationAddressFormatted), opt => opt.MapFrom(src => src.DestinationAddress.ToString()));
    }
}
