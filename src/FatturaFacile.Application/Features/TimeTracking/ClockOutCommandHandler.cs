// <copyright file="ClockOutCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Represents clock out command handler.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="mapper">The mapper.</param>
public sealed class ClockOutCommandHandler(ITimeEntryRepository repository, IMapper mapper)
    : IRequestHandler<ClockOutCommand, TimeEntryDto>
{
    private readonly ITimeEntryRepository repository = repository;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when active clock in session is not found.</exception>
    public async Task<TimeEntryDto> Handle(
        ClockOutCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var active = await repository.GetActiveClockInAsync(
                         request.EmployeeId,
                         cancellationToken)
                     ?? throw new InvalidOperationException("No active clock-in session found for this employee.");

        active.ClockOut(
            DateTime.UtcNow,
            request.BreakMinutes);
        await repository.UpdateAsync(
            active,
            cancellationToken);
        return mapper.Map<TimeEntryDto>(active);
    }
}
