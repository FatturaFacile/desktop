// <copyright file="GetTireRecordsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;

using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Handles <see cref="GetTireRecordsQuery"/> by querying tire warehouse storage records.
/// </summary>
/// <param name="repository">The tire record repository.</param>
/// <param name="mapper">The mapper instance used to convert domain entities to DTOs.</param>
public sealed class GetTireRecordsQueryHandler(ITireRecordRepository repository, IMapper mapper)
    : IRequestHandler<GetTireRecordsQuery, IReadOnlyList<TireHotelDto>>
{

    /// <summary>
    /// The repository used to query tire storage records.
    /// </summary>
    private readonly ITireRecordRepository repository = repository;

    /// <summary>
    /// The mapper instance used to convert domain entities to DTOs.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Retrieves all tire hotel records that are currently in storage.
    /// </summary>
    /// <param name="request">The query request instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the query execution returning a read-only list of stored <see cref="TireHotelDto"/> records.</returns>
    public async Task<IReadOnlyList<TireHotelDto>> Handle(
        GetTireRecordsQuery request,
        CancellationToken   cancellationToken)
    {
        var records = await repository.GetAllInStorageAsync(cancellationToken: cancellationToken);

        return mapper.Map<IReadOnlyList<TireHotelDto>>(source: records);
    }

}
