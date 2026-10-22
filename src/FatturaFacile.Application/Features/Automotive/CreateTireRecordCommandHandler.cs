// <copyright file="CreateTireRecordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Handles <see cref="CreateTireRecordCommand"/> by instantiating a new tire hotel domain entity and storing it via the repository.
/// </summary>
/// <param name="repository">The automotive tire record repository.</param>
/// <param name="tenantProvider">The tenant provider supplying the active tenant isolation boundary.</param>
/// <param name="mapper">The mapper used to transform the saved domain entity into <see cref="TireHotelDto"/>.</param>
public sealed class CreateTireRecordCommandHandler(
    ITireRecordRepository repository,
    ITenantProvider tenantProvider,
    IMapper mapper) : IRequestHandler<CreateTireRecordCommand, TireHotelDto>
{
    private readonly ITireRecordRepository repository = repository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Processes tire set check-in, validates parameters, creates the domain entity, and persists the record.
    /// </summary>
    /// <param name="request">The command describing the tire set condition and storage coordinates.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the operation returning the saved <see cref="TireHotelDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<TireHotelDto> Handle(
        CreateTireRecordCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var record = TireHotelRecord.CheckIn(
            tenantProvider.CurrentTenantId,
            request.VehicleId,
            request.Season,
            request.Brand,
            request.Dimension,
            request.DotCode,
            request.TreadDepthFrontLeftMm,
            request.TreadDepthFrontRightMm,
            request.TreadDepthRearLeftMm,
            request.TreadDepthRearRightMm,
            request.WarehouseRackCoordinate);

        await repository.AddAsync(
            record,
            cancellationToken);
        return mapper.Map<TireHotelDto>(record);
    }
}
