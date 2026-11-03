// <copyright file="CreateTireRecordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Automotive;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Handles the <see cref="CreateTireRecordCommand"/> request to check in a seasonal customer tire set
/// to the workshop warehouse depository.
/// </summary>
/// <param name="repository">The tire record repository abstraction.</param>
/// <param name="tenantProvider">The tenant context provider.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateTireRecordCommandHandler(
    ITireRecordRepository repository,
    ITenantProvider       tenantProvider,
    IMapper               mapper) : IRequestHandler<CreateTireRecordCommand, TireHotelDto>
{

    /// <summary>
    /// The repository responsible for persisting <see cref="TireHotelRecord"/> entities.
    /// </summary>
    private readonly ITireRecordRepository repository = repository;

    /// <summary>
    /// The tenant provider supplying the active tenant isolation boundary.
    /// </summary>
    private readonly ITenantProvider tenantProvider = tenantProvider;

    /// <summary>
    /// The mapper used to transform the saved domain entity into <see cref="TireHotelDto"/>.
    /// </summary>
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
        CancellationToken       cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var data = new TireHotelCheckInData(
            TenantId: tenantProvider.CurrentTenantId,
            VehicleId: request.VehicleId,
            Season: request.Season,
            Brand: request.Brand,
            Dimension: request.Dimension,
            DotCode: request.DotCode,
            FlMm: request.TreadDepthFrontLeftMm,
            FrMm: request.TreadDepthFrontRightMm,
            RlMm: request.TreadDepthRearLeftMm,
            RrMm: request.TreadDepthRearRightMm,
            RackCoordinate: request.WarehouseRackCoordinate);

        var record = TireHotelRecord.CheckIn(data: data);

        await repository.AddAsync(
                                  entity: record,
                                  cancellationToken: cancellationToken);

        return mapper.Map<TireHotelDto>(source: record);
    }

}
