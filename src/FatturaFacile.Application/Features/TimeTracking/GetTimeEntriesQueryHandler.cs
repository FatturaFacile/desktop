// <copyright file="GetTimeEntriesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;

using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Handles <see cref="GetTimeEntriesQuery"/> by fetching time entries from the repository based on employee filtering criteria.
/// </summary>
/// <param name="repository">The time entry repository.</param>
/// <param name="mapper">The mapper used to transform domain entities into <see cref="TimeEntryDto"/>.</param>
public sealed class GetTimeEntriesQueryHandler(ITimeEntryRepository repository, IMapper mapper)
    : IRequestHandler<GetTimeEntriesQuery, IReadOnlyList<TimeEntryDto>>
{

    /// <summary>
    /// The time entry repository.
    /// </summary>
    private readonly ITimeEntryRepository repository = repository;

    /// <summary>
    /// The mapper used to transform domain entities into DTOs.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Processes the query, retrieving recent entries for the specified employee or all stored entries.
    /// </summary>
    /// <param name="request">The query containing optional employee filter constraints.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the query execution returning a read-only list of <see cref="TimeEntryDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<IReadOnlyList<TimeEntryDto>> Handle(
        GetTimeEntriesQuery request,
        CancellationToken   cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var entries = request.EmployeeId.HasValue
                          ? await repository.GetEntriesByEmployeeAndDateRangeAsync(
                                                                                   employeeId: request.EmployeeId.Value,
                                                                                   startUtc: DateTime.UtcNow.AddDays(value: -30),
                                                                                   endUtc: DateTime.UtcNow,
                                                                                   cancellationToken: cancellationToken)
                          : await repository.GetAllAsync(cancellationToken: cancellationToken);

        return mapper.Map<IReadOnlyList<TimeEntryDto>>(source: entries);
    }

}
