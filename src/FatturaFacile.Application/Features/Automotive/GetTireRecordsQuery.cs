using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Represents get tire records query.
/// </summary>
public record GetTireRecordsQuery() : IRequest<IReadOnlyList<TireHotelDto>>;
