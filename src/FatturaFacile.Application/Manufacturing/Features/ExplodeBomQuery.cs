// <copyright file="ExplodeBomQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Manufacturing.Bom;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Explodes a cached BOM for a quantity and rolls the costs up (offline).
/// </summary>
/// <param name="BomId">The BOM identifier.</param>
/// <param name="Quantity">The quantity to produce.</param>
public sealed record ExplodeBomQuery(Guid BomId, decimal Quantity) : IRequest<BomExplosionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingRead;
}
