// <copyright file="ClockOutCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;

using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Handles <see cref="ClockOutCommand"/> by finalizing the active time entry for an employee, calculating worked hours, and updating the record.
/// </summary>
/// <param name="repository">The time entry repository.</param>
/// <param name="mapper">The mapper used to transform the updated domain entity to <see cref="TimeEntryDto"/>.</param>
public sealed class ClockOutCommandHandler(ITimeEntryRepository repository, IMapper mapper)
    : IRequestHandler<ClockOutCommand, TimeEntryDto>
{

    /// <summary>
    /// The repository used to query and update time entry records.
    /// </summary>
    private readonly ITimeEntryRepository repository = repository;

    /// <summary>
    /// The mapper used to transform the updated domain entity to <see cref="TimeEntryDto"/>.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Executes the clock-out operation, stamping the end timestamp, deducting breaks, and persisting the updated entry.
    /// </summary>
    /// <param name="request">The command containing employee identifier and break time duration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the operation returning the finalized <see cref="TimeEntryDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no active clock-in session exists for the employee.</exception>
    public async Task<TimeEntryDto> Handle(
        ClockOutCommand   request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var active = await repository.GetActiveClockInAsync(
                                                            employeeId: request.EmployeeId,
                                                            cancellationToken: cancellationToken)
                  ?? throw new InvalidOperationException(message: "No active clock-in session found for this employee.");

        active.ClockOut(
                        clockOutUtc: DateTime.UtcNow,
                        breakMinutes: request.BreakMinutes);

        await repository.UpdateAsync(
                                     entity: active,
                                     cancellationToken: cancellationToken);

        return mapper.Map<TimeEntryDto>(source: active);
    }

}
