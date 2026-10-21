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
/// Represents create tire record command handler.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="tenantProvider">The tenant provider.</param>
/// <param name="mapper">The mapper.</param>
public sealed class CreateTireRecordCommandHandler(
    ITireRecordRepository repository,
    ITenantProvider tenantProvider,
    IMapper mapper) : IRequestHandler<CreateTireRecordCommand, TireHotelDto>
{
    private readonly ITireRecordRepository repository = repository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
