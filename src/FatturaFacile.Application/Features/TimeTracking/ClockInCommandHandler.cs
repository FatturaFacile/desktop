// <copyright file="ClockInCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Repositories;

using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Handles <see cref="ClockInCommand"/> by verifying that no open session exists, initializing a new time entry, and persisting it.
/// </summary>
/// <param name="repository">The time entry repository.</param>
/// <param name="tenantProvider">The tenant provider supplying the active tenant isolation boundary.</param>
/// <param name="mapper">The mapper used to project the created time entry entity into a <see cref="TimeEntryDto"/>.</param>
public sealed class ClockInCommandHandler(
    ITimeEntryRepository repository,
    ITenantProvider      tenantProvider,
    IMapper              mapper) : IRequestHandler<ClockInCommand, TimeEntryDto>
{

    /// <summary>
    /// The repository used to persist and query time entry records.
    /// </summary>
    private readonly ITimeEntryRepository repository = repository;

    /// <summary>
    /// The tenant provider supplying the active tenant isolation boundary.
    /// </summary>
    private readonly ITenantProvider tenantProvider = tenantProvider;

    /// <summary>
    /// The mapper used to project the created time entry entity into a <see cref="TimeEntryDto"/>.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Executes the clock-in operation, ensuring the employee does not already have an open shift.
    /// </summary>
    /// <param name="request">The command describing the clock-in event details and geolocation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the clock-in operation returning the saved <see cref="TimeEntryDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the employee already has an uncompleted active clock-in session.</exception>
    public async Task<TimeEntryDto> Handle(
        ClockInCommand    request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var existing = await repository.GetActiveClockInAsync(
                                                              employeeId: request.EmployeeId,
                                                              cancellationToken: cancellationToken);

        if (existing != null)
        {
            throw new InvalidOperationException(message: "Employee already has an active clock-in session.");
        }

        var entry = TimeEntry.ClockIn(
                                      tenantId: tenantProvider.CurrentTenantId,
                                      employeeId: request.EmployeeId,
                                      clockInUtc: DateTime.UtcNow,
                                      recordingType: request.RecordingType,
                                      lat: request.Latitude,
                                      lon: request.Longitude,
                                      badgeSerial: request.BadgeSerial);

        await repository.AddAsync(
                                  entity: entry,
                                  cancellationToken: cancellationToken);

        return mapper.Map<TimeEntryDto>(source: entry);
    }

}
