using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

public record GetTireRecordsQuery() : IRequest<IReadOnlyList<TireHotelDto>>;