using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Represents clock out command.
/// </summary>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="BreakMinutes">The break minutes.</param>
public record ClockOutCommand(
    Guid EmployeeId,
    int BreakMinutes = 0) : IRequest<TimeEntryDto>;
