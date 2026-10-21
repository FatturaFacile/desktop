// <copyright file="GetTimeEntriesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Represents get time entries query handler.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="mapper">The mapper.</param>
public sealed class GetTimeEntriesQueryHandler(ITimeEntryRepository repository, IMapper mapper)
    : IRequestHandler<GetTimeEntriesQuery, IReadOnlyList<TimeEntryDto>>
{
    private readonly ITimeEntryRepository repository = repository;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<TimeEntryDto>> Handle(
        GetTimeEntriesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entries = request.EmployeeId.HasValue
            ? await repository.GetEntriesByEmployeeAndDateRangeAsync(
                request.EmployeeId.Value,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                cancellationToken)
            : await repository.GetAllAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<TimeEntryDto>>(entries);
    }
}
