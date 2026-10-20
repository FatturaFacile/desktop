using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

public record ClockOutCommand(
    Guid EmployeeId,
    int BreakMinutes = 0) : IRequest<TimeEntryDto>;