using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

public record GetTimeEntriesQuery(Guid? EmployeeId = null) : IRequest<IReadOnlyList<TimeEntryDto>>;