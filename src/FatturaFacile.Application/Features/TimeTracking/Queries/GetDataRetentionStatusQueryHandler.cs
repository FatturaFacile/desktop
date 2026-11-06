// <copyright file="GetDataRetentionStatusQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Handles <see cref="GetDataRetentionStatusQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The active company.</param>
public sealed class GetDataRetentionStatusQueryHandler(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService)
    : IRequestHandler<GetDataRetentionStatusQuery, DataRetentionStatusDto>
{
    /// <inheritdoc />
    public async Task<DataRetentionStatusDto> Handle(GetDataRetentionStatusQuery request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(currentTenantService.TenantId, cancellationToken).ConfigureAwait(false);
        var years = DataRetentionPolicy.EffectiveYears(tenant?.TimeRecordRetentionYears ?? DataRetentionPolicy.MinimumYears);
        return new DataRetentionStatusDto(years, DataRetentionPolicy.Cutoff(DateTime.Today, years), tenant?.LastRetentionRunAt);
    }
}
